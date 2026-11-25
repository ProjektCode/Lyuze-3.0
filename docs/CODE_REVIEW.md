# Lyuze 3.0 Code Review

## Review status

- **Review type:** Read-only bug, security, and dependency review
- **Repository:** `ProjektCode/Lyuze-3.0`
- **Reviewed branch:** `master`
- **Reviewed commit:** `1884033` (`docs: updated AGENTS.md`)
- **Roadmap changes:** None
- **Implementation changes:** None as part of the review
- **Primary reviewer:** OpenCode, followed by independent source, dependency, and build verification

This document records findings from the current repository state. It is internal project documentation and is intentionally excluded from `.github/public-docs.txt`; it must not be published to the GitHub Wiki.

## Executive summary

The repository builds successfully, but the review found four actionable application issues:

1. **High:** User-controlled profile background URLs are fetched by the bot without SSRF protections or response-size/content validation.
2. **High:** Example user context-menu commands are registered from the production assembly and can grant hard-coded roles without permission checks.
3. **Medium:** `/roles remove_role` loses the selected target user and removes the selected role from the component clicker instead.
4. **Medium:** The reaction-role cache uses a mutable `Dictionary` across asynchronous initialization, administration, and gateway-event paths without synchronization.

The review also found:

- **Low:** Exception messages are returned to Discord users in several error embeds/responses.
- **Low:** The anime/image documentation says the bot downloads input images internally, but the current trace and SauceNAO request paths pass the source URL to providers; only a SauceNAO thumbnail is downloaded locally.
- **Hardening:** The bot requests `GatewayIntents.All`; the required intent set should be narrowed.
- **Dependency advisories:** `SharpCompress 0.30.1` and `Snappier 1.0.0` are flagged transitively through `MongoDB.Driver 3.5.2`. The packages are confirmed present and advisory-affected, but direct exploitability in this application was not demonstrated because the application does not directly use archive extraction or `SnappyStream`, and no explicit MongoDB compressor configuration was found.

## Method and evidence

The review used two OpenCode passes:

1. **Initial repository review:** OpenCode inspected source, call sites, persistence, Discord interaction handling, external HTTP paths, lifecycle behavior, logging, CI, and dependency metadata.
2. **Second-pass triage:** OpenCode re-opened the referenced files and classified each initial finding as a confirmed vulnerability, confirmed bug, hardening recommendation, documentation drift, or false positive. It corrected inaccurate initial line references and narrowed the dependency claims.

The required `/context full` refresh was attempted in an interactive OpenCode session. It hung for nearly eight minutes while resolving the linked vault context and was stopped without repository changes. The review therefore treated the repository's `AGENTS.md`, `docs/TASKS.md`, and `docs/ROADMAP.md` as the current project sources, consistent with the repository instructions.

Independent verification performed after OpenCode's review:

```text
dotnet list "Lyuze 3.0.csproj" package --vulnerable --include-transitive
dotnet list "Lyuze 3.0.csproj" package --include-transitive
dotnet list "Lyuze 3.0.csproj" package --outdated --include-transitive
dotnet build "Lyuze 3.0.sln" --no-restore --configuration Release
git diff --check
```

Results:

- Release build: **passed** with 0 warnings and 0 errors.
- `git diff --check`: **passed** before this report was written.
- Dependency advisory scan: **two vulnerable transitive packages reported**.
- No runtime Discord or MongoDB integration test was executed during this review.
- Secret-bearing runtime settings were intentionally not read or printed.

## Findings requiring action

### CR-001 — User-controlled profile background URL can cause SSRF and unbounded image fetching

- **Severity:** High
- **Confidence:** High
- **Classification:** Confirmed vulnerability
- **Affected paths:**
  - `Core/Features/Profiles/ProfileModule.cs:29-40`
  - `Core/Features/Profiles/Services/ProfileService.cs:37-46`
  - `Core/Shared/Embeds/EmbedService.cs:36-47`
  - `Core/Shared/Images/ColorUtils.cs:17-38`
  - `Core/Shared/Images/Primitives/ImageFetcher.cs:10-29`
  - `Core/Infrastructure/Http/ApiClient.cs:77-94`

#### Evidence

`/profile setprofilebg` accepts an arbitrary string URL and stores it in the player profile. When a profile is rendered, `EmbedService.ProfileEmbedAsync` uses the stored URL as both the embed image and the input to `RandomColorFromUrlAsync`. That method fetches and decodes the URL server-side through `ImageFetcher` and `ApiClient`.

The fetch path does not enforce:

- HTTPS-only or approved-host restrictions.
- Resolution of the hostname followed by rejection of loopback, private, link-local, or other local-network addresses.
- Redirect validation.
- Maximum response size or a safe `Content-Length` policy.
- Image content-type validation before buffering.
- A bounded image decode policy.

`ApiClient.GetBytesAsync` buffers the complete HTTP response with `ReadAsByteArrayAsync`, and `ImageFetcher` passes the resulting bytes to `Image.FromStream`.

#### Impact

A user can set their own background to an internal or otherwise sensitive URL. When the bot or an authorized viewer renders that profile, the bot host makes the request. Depending on the host network and reachable services, this can probe internal endpoints or consume excessive memory/CPU with a large or malformed response.

#### Recommended next step

Add a single validated image-fetching boundary that:

1. Accepts only approved HTTPS hosts or managed Discord/CDN URLs where possible.
2. Resolves and rejects private, loopback, link-local, multicast, and other non-public IP ranges.
3. Validates every redirect or disables redirects for untrusted URLs.
4. Enforces response-size, content-type, timeout, cancellation, and image-dimension limits before decoding.
5. Avoids fetching arbitrary user URLs during ordinary profile rendering where a stored/validated upload or provider URL can be used instead.

### CR-002 — Example user commands expose permissionless role grants

- **Severity:** High
- **Confidence:** High
- **Classification:** Confirmed vulnerability when the hard-coded roles exist and are manageable by the bot
- **Affected paths:**
  - `Core/Infrastructure/DiscordNet/Handlers/InteractionHandler.cs:10-12`
  - `Core/Features/Examples/UserCommandsModule.cs:6-38`

#### Evidence

`InteractionHandler.InitAsync` calls `AddModulesAsync(Assembly.GetEntryAssembly(), services)`, which loads interaction modules from the production assembly. `UserCommandsModule` is therefore registered along with the normal feature modules.

The module exposes three user context-menu commands:

- `Add Switch Plays Role`
- `Add Gamer Role`
- `Add Weeb Role`

They have no permission preconditions and directly call `AddRoleAsync` on the selected `IUser`. The role IDs are hard-coded in the module.

#### Impact

Any user who can invoke the context-menu command can attempt to grant one of those roles to an arbitrary selected member. If the role exists in the guild and is below the bot's highest role, this bypasses the intended administrative boundary for role assignment.

#### Recommended next step

Remove `Core/Features/Examples/UserCommandsModule.cs` from production module discovery, or move it to an explicitly opt-in development assembly. If the commands are intentionally retained, require appropriate permissions, validate guild/role ownership and hierarchy, and move role IDs into validated private configuration.

### CR-003 — `/roles remove_role` removes the role from the wrong user

- **Severity:** Medium
- **Confidence:** High
- **Classification:** Confirmed bug
- **Affected paths:**
  - `Core/Features/Roles/RolesModule.cs:18-50`
  - `Core/Features/Roles/RolesModule.cs:54-80`

#### Evidence

The slash command accepts a target `IUser`, checks the target's roles, and creates a select menu whose options contain only role IDs. The response uses a static component custom ID:

```text
select_role_to_remove
```

The component handler receives only the selected role ID and then resolves:

```csharp
var guildUser = (SocketGuildUser)Context.User;
```

It removes the role from the component clicker instead of the user selected by the original command.

#### Impact

The command can report and intend to remove a role from one member while actually removing it from the administrator or other user who clicks the component. This is incorrect behavior and can produce unexpected role changes.

#### Recommended next step

Carry the target user ID in a signed/scoped component custom ID or another short-lived interaction state. Validate that the component clicker is the original requester or has the required administrative permission, then resolve and modify the intended target member.

### CR-004 — Reaction-role cache is not synchronized

- **Severity:** Medium
- **Confidence:** Medium
- **Classification:** Confirmed concurrency bug/design flaw
- **Affected paths:**
  - `Core/Features/Roles/Services/ReactionRolesService.cs:19`
  - `Core/Features/Roles/Services/ReactionRolesService.cs:40-49`
  - `Core/Features/Roles/Services/ReactionRolesService.cs:54-68`
  - `Core/Features/Roles/Services/ReactionRolesService.cs:71-110`

#### Evidence

`_emojiToRoleId` is a mutable `Dictionary<(ulong MessageId, string Emoji), ulong>`. It is:

- Cleared and repopulated by `InitializeAsync`.
- Written by `AddReactionRoleAsync`.
- Read by `OnReactionAddedAsync` and `OnReactionRemovedAsync`.

Those methods are reached from startup, administrator interactions, and Discord gateway callbacks. No lock or concurrent collection protects the cache.

#### Impact

A read can overlap with a write or `Clear()` during initialization/reload or administration. This can cause inconsistent lookups or runtime failures because `Dictionary` does not support unsynchronized concurrent reads and writes.

#### Recommended next step

Use a `ConcurrentDictionary` for independent updates, or replace the cache atomically with an immutable snapshot during initialization. Add synchronization around reloads and tests that overlap cache initialization, admin updates, and reaction events.

## Additional confirmed information-disclosure hardening issue

### CR-005 — Raw exception messages are returned to Discord users

- **Severity:** Low
- **Confidence:** High
- **Classification:** Confirmed information-disclosure hardening issue
- **Affected paths:**
  - `Core/Features/Profiles/Services/ProfileService.cs:31-34,47-50,63-66,80-83,97-100`
  - `Core/Shared/Embeds/EmbedService.cs:74-76,96-99,117-119`
  - `Core/Features/Examples/UserCommandsModule.cs:25-36`
  - `Core/Features/Roles/RolesModule.cs:210`

#### Evidence

Several catch blocks log the exception and also put `ex.Message` into an embed or interaction response. Exception messages can contain provider URLs, database details, local paths, role identifiers, or other implementation information.

#### Impact

Users can receive internal diagnostic details. This is not currently confirmed as credential disclosure, but it unnecessarily expands the information exposed through Discord responses.

#### Recommended next step

Return a generic user-facing error with a correlation ID, while logging the full exception to the private operator log. Add redaction tests for database, HTTP, and filesystem exception messages.

## Confirmed documentation drift

### CR-006 — Anime/image documentation does not match the current request flow

- **Severity:** Low
- **Confidence:** High
- **Classification:** Documentation drift, not a security vulnerability
- **Affected paths:**
  - `docs/ANIME-IMAGE-SEARCH.md:22-26,40-45`
  - `Core/Features/Anime/AnimeModule.cs:26-47,52-84`
  - `Core/Features/Anime/Services/TraceMoeService.cs:15-22`
  - `Core/Features/Anime/Services/SauceNaoService.cs:18-32,97-115`

#### Evidence

The documentation says the bot downloads an attached image internally before sending it to the configured provider. The current command path passes the Discord CDN URL directly to TraceMoe and SauceNAO. SauceNAO separately downloads a result thumbnail for the response, but that is not the same as downloading the user's input image before provider submission.

#### Recommended next step

Either update the documentation to describe the current URL-forwarding behavior, or implement and verify the documented download/proxy path with size, content-type, timeout, and privacy controls.

## Hardening recommendations

### CR-007 — Narrow the gateway intents

- **Severity:** Low / hardening
- **Confidence:** High
- **Classification:** Confirmed configuration hardening opportunity
- **Reference:** `Program.cs:45-54`

The client requests `GatewayIntents.All`. The bot should enumerate only the intents required by its implemented features and document those requirements. This reduces data exposure, lowers operational privilege requirements, and makes intent-related failures easier to diagnose. This is not by itself a confirmed exploitable vulnerability.

### CR-008 — Add input and resource limits to all external image paths

- **Severity:** Low / hardening
- **Confidence:** High
- **Classification:** Hardening recommendation
- **References:** `Core/Features/Anime/AnimeModule.cs`, `Core/Infrastructure/Http/ApiClient.cs`, `Core/Shared/Images/Primitives/ImageFetcher.cs`

TraceMoe, SauceNAO, Danbooru IQDB, profile backgrounds, avatars, and generated banners all involve external URLs or image decoding. The shared HTTP/image boundary should enforce URL validation, response limits, content-type checks, cancellation, and decode limits consistently rather than relying on each caller.

## Dependency vulnerability review

### Command result

`dotnet list "Lyuze 3.0.csproj" package --vulnerable --include-transitive` reported:

```text
Transitive Package      Resolved   Severity   Advisory URL
SharpCompress           0.30.1     Moderate   https://github.com/advisories/GHSA-6c8g-7p36-r338
Snappier                1.0.0      High       https://github.com/advisories/GHSA-pggp-6c3x-2xmx
```

The dependency graph shows both packages arrive through `MongoDB.Driver 3.5.2`:

```text
MongoDB.Driver 3.5.2
├── SharpCompress 0.30.1
└── Snappier 1.0.0
```

### CR-009 — SharpCompress transitive advisory

- **Package:** `SharpCompress 0.30.1`
- **Severity reported by NuGet tooling:** Moderate
- **Advisory:** [GHSA-6c8g-7p36-r338](https://github.com/advisories/GHSA-6c8g-7p36-r338)
- **CVE:** CVE-2026-44788
- **Patched advisory version:** `0.48.0`
- **Classification:** Confirmed vulnerable resolved dependency; application exploitability not confirmed

The advisory concerns path traversal in `IArchive.WriteToDirectory()` for malicious archives. The Lyuze source contains no `SharpCompress` or archive extraction calls, so the reviewed application does not currently demonstrate the vulnerable usage path. The package remains present in the deployed dependency graph and should be remediated through a compatible MongoDB.Driver update or a carefully tested dependency override.

Do not suppress the advisory solely because the current application does not call the vulnerable API; keep the package graph clean and verify the resulting MongoDB behavior.

### CR-010 — Snappier transitive advisory

- **Package:** `Snappier 1.0.0`
- **Severity reported by NuGet tooling:** High
- **Advisory:** [GHSA-pggp-6c3x-2xmx](https://github.com/advisories/GHSA-pggp-6c3x-2xmx)
- **CVE:** CVE-2026-44302
- **Patched advisory version:** `1.3.1`
- **Classification:** Confirmed vulnerable resolved dependency; application exploitability context-dependent and not confirmed

The advisory concerns an infinite loop while decompressing malformed framed Snappy input. The Lyuze source does not directly use `Snappier`, `SnappyStream`, or configure MongoDB compressors. The package is nevertheless resolved transitively through MongoDB.Driver and is reported by the advisory scanner. A compromised or malicious data path would need to reach the vulnerable decompression code for application impact; that reachability was not established in this review.

Prioritize upgrading MongoDB.Driver to a release whose dependency graph resolves patched compression libraries, then rerun the vulnerability scan and verify MongoDB connectivity and persistence compatibility.

### Dependency freshness

`dotnet list package --outdated --include-transitive` also showed that the project is behind current package releases, including:

- Discord.NET `3.18.0` → `3.20.1`
- JikanDotNet `2.10.1` → `2.10.4`
- Microsoft.Extensions packages `10.0.0` → `10.0.11`
- MongoDB.Driver `3.5.2` → `3.11.0`
- System.Drawing.Common `10.0.0` → `10.0.11`
- SharpCompress `0.30.1` → `0.50.4` transitively
- Snappier `1.0.0` → `1.3.1` transitively

These are update recommendations, not proof that every outdated package is vulnerable. Dependency upgrades should be performed as a separate compatibility-tested task.

## Items reviewed and not confirmed as findings

- **DM behavior:** Source search found no `CreateDMChannelAsync`, `GetOrCreateDMChannelAsync`, or direct DM send path. Current notification code sends level-up messages through `ctx.Channel`.
- **General player-data guild partitioning:** Player lookups use `DiscordID`; the review did not find evidence of an incorrect guild-partitioned player key.
- **`/setplvlnotifications` regression:** The command calls `UpdateLevelNotificationsAsync`, not the profile-visibility update method.
- **Secret committed to source:** No secret-bearing runtime settings file was read. No hard-coded token or connection string was identified in the reviewed source. The example role IDs are identifiers, not credentials.
- **Initial claim that the profile background is fetched while saving:** The second OpenCode pass corrected this. The background is stored first and fetched later during profile rendering.

## Recommended triage order

1. Remove or isolate `UserCommandsModule` from production module registration.
2. Fix profile background URL validation and image-fetch resource limits.
3. Fix the role-removal component to preserve and authorize the original target user.
4. Make the reaction-role cache synchronized or atomically replaceable.
5. Upgrade MongoDB.Driver and verify the SharpCompress/Snappier advisories clear.
6. Stop returning raw exception messages to Discord users.
7. Narrow gateway intents.
8. Correct the anime/image documentation or implement the documented download behavior.

No fixes were applied during this review. The roadmap was not modified.
