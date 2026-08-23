# Lyuze Bot Roadmap

This roadmap tracks what Lyuze Bot has, what still needs work, and what is out of scope for now. It is meant to be a practical planning document, not a list of every idea that might be useful someday.

## Status Legend

- `[x]` Implemented in the repository
- `[~]` Partially implemented; requires completion or hardening
- `[ ]` Planned
- **Out of scope**: Removed from the active roadmap unless explicitly reintroduced

## Priority and Sequencing

1. **Finish and verify the current Players/Leveling split**: Stabilize the work already in progress before adding a new feature. This clears namespace, dependency-injection, persistence, and behavior-verification risk for later work.
2. **Leveling foundation hardening**: Move player/application service boundaries toward `ulong` IDs, replace the XP read-modify-replace path with atomic updates, and add focused verification before adding more XP sources or user-facing commands.
3. **Configuration and safety foundation**: This is both a major milestone and a release gate. It can be built alongside or after the current refactor, but production readiness depends on it.
4. **Leveling system completion**: Turn the stabilized foundation into a complete, configurable server feature.
5. **Core utility, information, image, anime-airing, reaction-role, operations, moderation, and selected external integrations.**
6. **Documentation, migrations, testing, and deployment hardening.**

---

## Configuration and Safety Foundation

This bot is personal-first and self-hosted. Each installation owns its MongoDB connection, so player profiles and XP should be keyed by Discord user ID rather than by guild ID plus user ID. Guild IDs still belong on records that point to Discord resources such as channels, roles, or messages, but the roadmap should not add guild partitioning to every collection by default. The bot must never DM users; use guild channels or interaction responses instead.

The bot currently depends on `Resources/Settings/settings.json` for secrets, startup configuration, server IDs, messages, image URLs, and other mutable settings. Keep bootstrap values in the file while moving mutable runtime settings into MongoDB.

### Storage boundary

- `[ ]` Keep bootstrap and secret values in `settings.json` or environment-backed configuration:
  - Discord bot token and application identity
  - MongoDB connection string and database name
  - External API keys and credentials
  - Fixed startup/runtime settings required before the database is available
- `[ ]` Move mutable runtime values into MongoDB:
  - Channel and role IDs where a feature points to a Discord resource
  - Welcome, goodbye, and status messages
  - Feature flags and runtime feature settings
  - Reaction-role message and mapping configuration
  - Leveling rules, restrictions, and notification settings
  - Moderation settings
  - Wallpaper/profile image collections
  - Anime airing notification subscriptions
- `[ ]` Define explicit MongoDB schemas, collection names, indexes, ownership rules, and version fields.
- `[ ]` Keep player profiles and XP keyed by Discord user ID. Add guild IDs only when a record needs to identify a Discord server resource or a future multi-guild deployment makes that necessary.

### Validation and migration

- `[ ]` Build a startup configuration validator for required settings, API credentials, MongoDB connectivity, Discord IDs, URL formats, and permission assumptions.
- `[ ]` Add safe diagnostics that identify missing or invalid configuration without logging tokens, API keys, passwords, or connection strings.
- `[ ]` Create a versioned migration path from `settings.json` to MongoDB.
- `[ ]` Add dry-run, backup, rollback, and validation steps before destructive configuration changes.
- `[ ]` Add automated tests for configuration parsing, validation, migration, and failure reporting.

### Setup and configuration commands

These commands need the same safety work as the rest of the configuration system; they are not just convenience commands.

- `[ ]` `/setup`: Initialize required collections, default settings, and feature prerequisites for this bot instance.
- `[ ]` `/config get`: View safe, non-secret configuration values.
- `[ ]` `/config set`: Change an approved setting with type and permission validation.
- `[ ]` `/config reset`: Restore a setting to its default value.
- `[ ]` `/config validate`: Check APIs, database access, Discord IDs, permissions, and feature readiness.
- `[ ]` `/config export`: Export safe installation configuration without secrets.
- `[ ]` `/config import`: Import validated configuration with preview, confirmation, and rollback support.

---

## Leveling System

### Current evaluation

`[~]` The current system works, but it is not comprehensive.

What exists today:

- XP from qualifying messages with a three-second per-user cooldown.
- XP from successful slash commands.
- A quadratic level equation and level-up notifications.
- MongoDB persistence through `PlayerModel` and `PlayerService`.
- A user preference for level-up notifications.

Gaps to close:

- `PlayerModel` is keyed by Discord user ID, which fits the self-hosted instance model. Keep that identity unless the project later targets shared multi-guild hosting.
- XP values and cooldowns are hard-coded instead of being configurable for the installation.
- A large XP increase can produce only one level-up pass; this is not urgent while normal events award small amounts, but bulk admin grants or future daily/voice/reaction awards may need a defined policy.
- There are no `/level`, `/rank`, `/leaderboard`, or progress commands.
- There is no combined profile/rank card.
- `[x]` `/setplvlnotifications` calls the level-notification update path and remains an in-channel toggle. Keep verifying that it never routes through the profile-visibility update path.
- There are no XP source restrictions for channels, roles, threads, forums, voice, slash commands, DMs, or system messages.
- There is no robust anti-spam/anti-farming policy, XP audit trail, atomic update strategy, or repair/resynchronization flow.
- There is no leveling migration plan or automated test coverage.

### Comprehensive leveling target

A complete leveling system needs the following pieces:

#### XP foundation

- `[ ]` Keep the current quadratic equation as the default curve unless testing shows a better fit.
- `[ ]` Make the curve configurable through named formulas and parameters, not arbitrary user-entered code. A curve maps a level to the total XP required to reach it.
- `[ ]` Replace the vague `XP range` term with `XP award range`: the minimum and maximum XP granted by one event, such as a message, command, reaction, voice interval, or daily claim. It is not a level.
- `[ ]` Make the message cooldown and XP awarded by each source configurable for the installation.
- `[ ]` Make total XP the canonical player value and derive level/progress from it. Use MongoDB's atomic `$inc` for XP awards so simultaneous events cannot overwrite one another.
- `[ ]` Define level-up behavior: when a user crosses the next threshold, preserve carryover XP, increase the level once, and send one in-channel notification if notifications are enabled. Never DM the user.
- `[ ]` Do not prioritize multiple notifications for normal awards. If a bulk grant crosses several thresholds, calculate the final level from total XP and define whether to send one summary notification or no notification.
- `[ ]` Add cooldown cleanup and bounded memory behavior for active installations.
- `[ ]` Define XP restrictions for bots, DMs, system/moderation messages, ignored channels, ignored roles, threads, forums, voice, and slash commands.
- `[ ]` Do not reject every short message by default. Use context-sensitive rules such as repeated-content detection, rapid duplicate messages, and configurable exclusions.
- `[ ]` Add optional XP multipliers for selected roles, events, and server boosters. Do not add channel XP multipliers unless a real use case appears; channel restrictions are enough for now.
- `[ ]` Add time-based voice XP with anti-idle rules and safeguards for empty, AFK, muted, or deafened sessions.
- `[ ]` Add reaction XP with per-message/per-user limits, self-reaction rules, duplicate prevention, and daily or interval caps.

#### User-facing commands

- `[ ]` `/level` or `/rank`: Show current XP, level, progress to the next level, and rank among users visible in the current server. The XP record remains keyed by user ID; the server filter is only for presentation.
- `[ ]` `/leaderboard`: Show the top users with pagination, using the current server as the display filter when appropriate.
- `[ ]` Combine profile and leveling information into one custom `/profile` card with avatar, background, about-me text, level, XP progress, and rank.
- `[ ]` Keep `/setplvlnotifications` as an in-channel toggle. The bot must never DM level-up notifications.
- `[ ]` `/xp`: Claim a simple once-per-day XP award. Store the last claim time and define whether the daily amount is fixed or follows the configured award rules.

#### Rewards and administration

- `[ ]` Defer automatic level-based role rewards for now. There is no shop or reward system yet, so do not add placeholder rewards just to fill this section.
- `[ ]` Revisit rewards later with a concrete design, such as cosmetic profile changes, role assignments, unlockable backgrounds, or other benefits that fit the personal project.
- `[ ]` Add `/xpadmin add`, `/xpadmin remove`, `/xpadmin set`, and `/xpadmin reset` for controlled XP administration.
- `[ ]` Add `/xpadmin config` for the curve, award amounts, cooldowns, daily XP, voice/reaction rules, notification behavior, and multipliers.
- `[ ]` Add `/xpadmin restrictions` for channels, roles, threads, forums, voice, slash commands, DMs, bots, system messages, and anti-farming rules.
- `[ ]` Add level-up message templates and a configured announcement channel.
- `[ ]` Keep all administrative actions permission-checked and logged without exposing unnecessary message content.

#### Integrity and verification

- `[ ]` Add anti-farming tests for cooldowns, repeated messages, command XP, daily claims, voice sessions, reaction XP, and ignored contexts.
- `[ ]` Add concurrency tests proving simultaneous XP awards do not overwrite one another.
- `[ ]` Add migration tests for existing player records and any total-XP conversion.
- `[ ]` Add leaderboard/index performance tests.
- `[ ]` Add profile-card rendering tests for missing backgrounds, private profiles, long about-me text, and unavailable avatars.
- `[ ]` Verify that level-up notifications stay in-channel and never become DMs.
- `[ ]` Add logging and diagnostics that do not expose user secrets or excessive message content.

Arcane and Tatsu are useful comparisons here. Both go beyond message counting with XP controls, role rewards, leaderboards, and rank cards. See the [Arcane leveling documentation](https://docs.arcane.bot/plugins/leveling/), [Arcane role-reward documentation](https://docs.arcane.bot/plugins/leveling/setup/role-rewards), and [Tatsu leveling overview](https://tatsu.gg/leveling).

---

## Discord Event Architecture

The current `Core/Infrastructure/DiscordNet/Handlers/EventHandler.cs` centralizes member joins, message processing, leveling, moderation checks, slash-command results, command registration, reaction-role startup, status rotation, and Discord logging. That is acceptable while the bot is small, but it should not grow indefinitely.

- `[ ]` Refactor event-specific classes under `Core/Infrastructure/DiscordNet/Handlers/Events/` when the current handler becomes difficult to maintain.
- `[ ]` Split member-join, message, interaction, ready/startup, and logging concerns into focused event handlers while keeping Discord.Net event wiring in the infrastructure layer.
- `[ ]` Keep feature rules in their feature services. Event handlers should translate Discord events into calls to Profiles, Players, Leveling, Moderation, Roles, and other feature services rather than owning those rules.
- `[ ]` Add tests for event registration, duplicate ready handling, cancellation/shutdown behavior, and failure isolation between unrelated handlers.

---

## Image-Based Commands and Wallpapers

The image commands already exist and should stay. Each one should tell self-hosters which outside service it uses.

### Existing image commands

- `[x]` `/trace`: Trace an anime scene from an upload or URL using [trace.moe](https://trace.moe/) and its [API documentation](https://soruly.github.io/trace.moe-api/).
- `[x]` `/sauce`: Find image sources using [SauceNAO](https://saucenao.com/), with [Danbooru](https://danbooru.donmai.us/) IQDB as the fallback provider. See the [Danbooru API documentation](https://danbooru.donmai.us/wiki_pages/help:api).
- `[x]` `/waifu`: Return a tagged image using [Waifu.im](https://www.waifu.im/) and its [API documentation](https://docs.waifu.im/docs/api/). Keep the default behavior SFW and document the available [tags and NSFW parameter behavior](https://docs.waifu.im/docs/tags/).

### Image command hardening

- `[ ]` Add provider links to embeds, command help, and error messages where appropriate.
- `[ ]` Add provider-specific rate-limit handling, timeout behavior, fallback messages, and safe retries.
- `[ ]` Validate upload type, remote URL format, response content type, and maximum download size.
- `[ ]` Document provider attribution, content-safety behavior, API-key requirements, and self-hosting considerations.

### Wallpaper command

- `[ ]` Add a `/wallpaper` command with desktop and mobile modes, optional query/tag, and optional resolution/aspect-ratio filters.
- `[ ]` Use [Wallhaven](https://wallhaven.cc/) and its [official API documentation](https://wallhaven.cc/help/api) as the primary wallpaper provider.
- `[ ]` Use SFW-only defaults and make any broader content setting explicit and administrator-controlled.
- `[ ]` Map desktop requests to wide resolutions/aspect ratios such as `16x9` and mobile requests to portrait ratios such as `9x16`.
- `[ ]` Respect provider rate limits and return the source page, direct image, resolution, and attribution where available.
- `[ ]` Keep [Unsplash](https://unsplash.com/) and its [developer API](https://unsplash.com/developers) as a possible secondary source for general photography, not as the primary anime-wallpaper source.

---

## Anime, Otaku, Airing, and Notifications

### Keep

- `[x]` `/aquote`: Existing random anime quote command.
- `[x]` `/trace`, `/sauce`, and `/waifu` remain under Image-Based Commands above.

### Airing and schedule features

- `[ ]` `/seasonal`: Show currently airing or upcoming seasonal anime.
- `[ ]` `/airing`: Show upcoming episodes with time and episode information.
- `[ ]` `/schedule`: Provide a broader schedule view with filtering and pagination.
- `[ ]` Use [AniList](https://docs.anilist.co/) as the primary structured anime data source and its [AiringSchedule object](https://docs.anilist.co/reference/object/airingschedule), which exposes the future airing time, episode number, and associated media ID.
- `[ ]` Cache schedule results and handle API errors, missing data, and rate limits.

### Notification design

- `[ ]` `/notify add`: Subscribe a user to an anime airing notification.
- `[ ]` `/notify remove`: Remove a subscription.
- `[ ]` `/notify list`: List active subscriptions.
- `[ ]` Store subscriptions in MongoDB with guild ID, user ID, media ID, delivery target, enabled state, and last-notified airing ID.
- `[ ]` Implement a background hosted service that polls upcoming AniList airing data, calculates due notifications, prevents duplicates, and retries safely.
- `[ ]` Deliver notifications only to a configured guild channel. The bot must never DM users.
- `[ ]` Add opt-in limits, cleanup for finished/cancelled series, and administrator controls for notification channels and volume.
- `[ ]` Add tests for duplicate prevention, restart recovery, time boundaries, API outages, and subscription cleanup.

### Explicitly removed from active scope

- Anime and manga search/info commands
- Character search/info commands
- Staff search/info commands
- Studio search/info commands
- Watchlist add/remove/list commands

---

## Selected External Integrations

These integrations are selected for the personal/self-hosted scope. They should deliver notifications to configured guild channels or respond in-server; the bot must never DM users for these features.

### GitHub

- `[ ]` Add configurable repository watches and channel destinations.
- `[ ]` Add GitHub webhook handling for selected push, pull request, issue, release, and workflow-run events.
- `[ ]` Validate webhook signatures and reject untrusted payloads.
- `[ ]` Add `/github repo`, `/github issues`, `/github prs`, and `/github actions` read commands where useful.
- `[ ]` Add rate-limit handling, retry behavior, event deduplication, and safe rendering of repository content.
- `[ ]` Prefer webhooks over frequent polling for repositories that the installation owns.
- `[ ]` Use the [GitHub webhook documentation](https://docs.github.com/en/webhooks/webhook-events-and-payloads) and [REST API rate-limit documentation](https://docs.github.com/en/rest/using-the-rest-api/rate-limits-for-the-rest-api).

### Minecraft server status

- `[ ]` Add `/minecraft status` for configured Java and/or Bedrock servers.
- `[ ]` Show online state, player counts, version, MOTD, and other fields returned by the provider.
- `[ ]` Cache status responses and avoid repeated requests during the provider cache window.
- `[ ]` Add optional channel notifications for offline/online transitions.
- `[ ]` Use [mcsrvstat.us](https://api.mcsrvstat.us/) as the initial free provider and follow its cache and User-Agent requirements.

### Music with Lavalink4NET

- `[ ]` Use [Lavalink4NET](https://github.com/lavalink4net/Lavalink4NET) with the existing Discord.Net stack.
- `[ ]` Add music commands for join, play, pause, resume, skip, stop, queue, now-playing, and volume after the node lifecycle is stable.
- `[ ]` Support a Lavalink v4 node as an external process. Lavalink remains a separate Java application even when the bot launches it.
- `[ ]` Evaluate an optional bot-managed launcher using `ProcessStartInfo` for the personal installation. The launcher should own startup, readiness checks, stdout/stderr logging, graceful shutdown, configurable paths, and controlled restart behavior.
- `[ ]` Keep the Lavalink JAR and installation-specific `application.yml` outside normal source commits unless a deliberate packaging decision is made.
- `[ ]` Support an externally managed Lavalink process later without changing the music service API.
- `[ ]` Document Java 17+, node password/port configuration, source plugins, resource usage, and failure behavior.
- `[ ]` Add tests for node-unavailable startup, reconnects, process shutdown, queue behavior, and voice-channel permissions.

---

### Generic webhook and RSS notifications

- `[ ]` Define a generic notification subscription model with source, event identity, destination channel, enabled state, filters, and delivery history.
- `[ ]` Add inbound webhook endpoints for trusted self-hosted services, with authentication, signature validation where supported, replay protection, and rate limits.
- `[ ]` Add RSS/Atom polling with configurable intervals, conditional requests, deduplication, retry/backoff behavior, and stale-feed handling.
- `[ ]` Keep provider-specific parsing in adapters rather than putting Jellyfin, Suwayomi, GitHub, or other service logic into the core notification service.
- `[ ]` Add adapters for useful personal services such as Jellyfin, Suwayomi, Uptime Kuma, GitHub, and other RSS/Webhook-capable tools after the generic pipeline works.
- `[ ]` Deliver notifications only to configured guild channels or in-server responses. Never DM users.
- `[ ]` Add notification-preview, test-delivery, pause, resume, and remove operations with permission checks.
- `[ ]` Store secrets outside notification payloads and never write webhook credentials to logs.

## Utility Commands

- `[x]` `/help`: Category-first ephemeral help menu with permission-aware command visibility.
- `[ ]` `/ping`: Check response latency.
- `[ ]` `/uptime`: Show time since the current process started.
- `[ ]` `/botinfo`: Show bot version, runtime, and self-hosting information.
- `[ ]` `/invite`: Generate or display a self-hosting-friendly Discord OAuth2 invite link from configured application information and required permissions. Do not depend on a hosted Lyuze service.

---

## Info Commands

- `[ ]` `/serverinfo`: Display server information.
- `[ ]` `/userinfo`: Display information about a selected user.
- `[ ]` `/avatar`: Display a user's full-size avatar.
- `[ ]` `/banner`: Display a user's banner when available.

---

## Feature Flags

- `[ ]` `/feature list`: Display available feature flags and their current state.
- `[ ]` `/feature enable`: Enable a feature for the current guild.
- `[ ]` `/feature disable`: Disable a feature for the current guild.
- `[ ]` Store feature flags in the database after the configuration boundary is defined.
- `[ ]` Apply feature flags consistently to command registration, execution, scheduled jobs, and background services.

---

## Reaction Roles

`[~]` Reaction roles are partially implemented, not complete.

### Existing implementation

- `[x]` `/setup_reaction_roles` creates the reaction-role message and stores its message ID.
- `[x]` `/add_reaction_roles` adds an emoji/role mapping and persists it in MongoDB.
- `[x]` Reaction-added events assign the configured role.
- `[x]` Reaction-removed events remove the configured role.
- `[x]` Startup loads mappings into an in-memory cache.

### Completion work

- `[ ]` Add the Discord server ID and channel ID to reaction-role records only because the referenced message, channel, and role are server-owned; this is not a general player-data partition.
- `[ ]` Add duplicate prevention and update behavior for an existing message/emoji mapping.
- `[ ]` Add commands to list, edit, and remove reaction-role mappings.
- `[ ]` Clarify or remove the overlapping `/add_bot_reaction` command.
- `[ ]` Distinguish user role removal from reaction-role configuration removal; the current `/remove_role` command is not a reaction-role management command.
- `[ ]` Validate bot role hierarchy, managed roles, permissions, message existence, and emoji format.
- `[ ]` Add repair/resynchronization for deleted messages, missing reactions, and stale database mappings.
- `[ ]` Move mutable reaction-role configuration out of `settings.json` after the database configuration boundary is implemented.
- `[ ]` Add tests for startup reload, add/remove reactions, duplicate mappings, deleted messages, and multiple configured reaction-role messages.

---

## Self-Hosted Operations

- `[ ]` `/health`: Check bot and core-service health.
- `[ ]` `/diagnostics`: Show runtime, memory, database, queue, and service diagnostics.
- `[ ]` `/logs tail`: Show recent logs through an in-memory ring buffer; current logging is primarily console-based.
- `[~]` Keep `ILoggingService` as a compatibility facade during the logging migration. New logging code should use Microsoft's `ILogger` pipeline instead of adding new `ILoggingService` call sites.
- `[ ]` Gradually migrate existing `ILoggingService` consumers to `ILogger<T>` after the shared pipeline is working. Do not require an all-at-once rewrite.
- `[ ]` Replace custom message concatenation with structured `ILogger` message templates and named properties, including correct exception parameters.
- `[ ]` Add a custom Microsoft `ConsoleFormatter` for Lyuze's existing severity/source prefix style, such as `INFO [DISCD] message`.
- `[ ]` Preserve console color coding through the formatter: information green, warnings yellow, errors red, and critical errors a stronger red.
- `[ ]` Keep ANSI color codes limited to console output; file logs, audit records, and exported diagnostics must remain uncolored.
- `[ ]` Add formatter and structured-logging tests for severity mapping, source prefixes, exception output, scopes, redirected output, and sensitive-value redaction.
- `[ ]` Add rotating file logs alongside console output, with configurable path, retention, maximum size, minimum severity, and redaction of tokens, credentials, connection strings, and sensitive message content.
- `[ ]` Move file writes behind a bounded asynchronous queue or logging provider so Discord event handlers are not blocked by disk I/O.
- `[ ]` Keep the in-memory tail buffer bounded and safe for concurrent writes.
- `[ ]` Add structured log fields for source, severity, guild ID, channel ID, actor ID, target ID, command/event name, and correlation ID where available.
- `[ ]` Add durable audit records for moderation, AutoMod actions, XP administration, configuration changes, integration deliveries, and other security-relevant operations.
- `[ ]` Add `/audit recent` and filtered audit views with permission checks, pagination, retention rules, and content minimization.
- `[ ]` `/latency`: Measure gateway and API latency.
- `[ ]` `/backup create`: Back up configuration and data.
- `[ ]` `/backup list`: List available backups.
- `[ ]` `/backup restore`: Restore a selected backup with confirmation and safety checks.
- `[ ]` Add startup, shutdown, API, database, scheduled-job, and migration diagnostics suitable for self-hosted deployments.

---

## Moderation

### Already implemented

- `[x]` `purge`: Delete a specified number of messages.
- `[x]` `kick`: Remove a user from the server.
- `[x]` `ban`: Permanently ban a user.
- `[x]` `unban`: Unban a user through the interactive flow.

### Planned moderation commands

- `[ ]` `timeout` / `untimeout`
- `[ ]` `warn` / `warnings` / `clear-warnings`
- `[ ]` `slowmode`
- `[ ]` `lock` / `unlock`
- `[ ]` `softban`
- `[ ]` `massban`
- `[ ]` `modlog set` / `modlog show`
- `[ ]` `nick` / `reset-nick`
- `[ ]` `role add` / `role remove`
- `[ ]` `purge user` / `purge contains`
- `[ ]` `case view` / `case edit-reason` / `case delete`

All moderation work should include permission checks, bot hierarchy checks, audit-log reasons, failure handling, and tests for partial Discord API failures.

### Discord AutoMod integration

- `[ ]` Use Discord's built-in AutoMod as the first layer for keyword, spam, mention-spam, keyword-preset, and member-profile rules instead of duplicating all filtering inside Lyuze.
- `[ ]` Add administrator commands to inspect, create, update, enable, disable, and remove approved AutoMod rule types.
- `[ ]` Support configured exempt roles and channels, alert destinations, custom alert messages, and timeout/block actions where Discord permits them.
- `[ ]` Require `MANAGE_GUILD`, validate rule limits and bot permissions, and include Discord audit-log reasons for rule changes.
- `[ ]` Subscribe to Discord.Net AutoMod execution events when supported by the installed library and record rule, actor, channel, matched trigger, action, and outcome in the modlog/audit pipeline.
- `[ ]` Treat AutoMod event handling as an integration boundary because Discord.Net documents the execution event as subject to change; keep core enforcement in Discord's official AutoMod rules.
- `[ ]` Coordinate AutoMod with the existing anti-invite check so users do not receive duplicate actions or contradictory messages.
- `[ ]` Add tests for rule configuration validation, exempt contexts, alert delivery, event parsing, missing cached objects, and duplicate audit records.

---

## Database and Configuration Migration

This work is tied to the Configuration and Safety Foundation above. The default model is one self-hosted bot instance with one owned database. Do not add guild partitioning to player profiles or XP unless the project later chooses to support shared multi-guild hosting.

- `[ ]` Define the database document model for installation settings, player profiles, leveling, reaction roles, notifications, and moderation.
- `[ ]` Keep secrets and bootstrap connectivity in `settings.json` or environment-backed configuration.
- `[ ]` Move mutable runtime values out of `settings.json` after schemas and migration tooling are ready.
- `[ ]` Add schema versioning and migrations for existing player and reaction-role records.
- `[ ]` Add indexes for user lookups, leaderboards, notifications, and Discord-resource event handling.
- `[ ]` Refactor player/application services toward `ulong` Discord IDs instead of requiring `SocketGuildUser` for database and domain operations. Keep `SocketGuildUser` at the Discord adapter boundary during the transition.
- `[ ]` Add ID-based service overloads, migrate callers gradually, and remove the Discord-object overloads only after all feature paths and tests use the ID-based APIs.
- `[ ]` Add caching with explicit invalidation rules rather than unbounded process-local state.
- `[ ]` Add safe backup/export/import tooling.
- `[ ]` Add integration tests for database availability, migration failure, retry behavior, and partial writes.

---

## Out of Scope for Now

The following areas are intentionally removed from the active roadmap:

- Mini games
- Community tags, suggestions, polls, voting, and giveaways
- Starboard
- Generic timezone/reminder/event scheduling commands outside anime airing notifications
- User quote system
- Anime/manga/character/staff/studio search and info commands
- Watchlists

These items can return later only as deliberate roadmap changes; they should not stay as unchecked background work.

---

## Roadmap Maintenance

- Keep completed work marked `[x]` when verified in the repository.
- Use `[~]` for features that exist but need architectural completion or hardening.
- Add provider links whenever a feature depends on an external API or service.
- Update this roadmap after major feature completion, migration decisions, or scope changes.
- Keep the repository roadmap and the project vault context synchronized when the project state changes.
