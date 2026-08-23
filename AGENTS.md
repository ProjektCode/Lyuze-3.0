# Lyuze 3.0 Agent Instructions

Project-local instructions for the Lyuze 3.0 self-hosted Discord bot.

## Authority and scope

- Global OpenCode safety, approval, secrets, and Git rules always win over this file.
- This file adds Lyuze-specific paths, commands, architecture, and product constraints.
- The user must explicitly approve implementation edits, commits, pushes, and destructive actions.
- Preserve unrelated and pre-existing uncommitted work.
- Use the repo-root `AGENTS.md` location; do not create `./.opencode/AGENTS.md` unless explicitly required.

## Project packet

Keep these project documents aligned for meaningful work:

- `AGENTS.md` — local rules and commands.
- `docs/SPEC.md` — product and technical requirements.
- `docs/ROADMAP.md` — ordered project direction and milestones.
- `docs/TASKS.md` — current actionable tasks and verification evidence.

## Project identity

- Self-hosted C# Discord bot for personal/server use.
- .NET 8 with nullable reference types enabled.
- Discord.NET 3.18.0, Generic Host, dependency injection, and MongoDB.Driver 3.5.2.
- Main project file: `Lyuze 3.0.csproj`.
- Runtime configuration: `Resources/Settings/settings.json`.

## Product constraints

- Lyuze must never DM users.
- Profile and XP records are keyed by Discord user ID.
- Use guild IDs only for records that identify guild-specific Discord resources.
- Do not add multi-tenant SaaS assumptions to this personal/self-hosted project.
- Prefer guild-channel responses or interaction responses over direct messages.

## Secrets and configuration

- Never commit, print, paste, or log real tokens, API keys, passwords, or connection strings.
- Never share the real `Resources/Settings/settings.json`.
- Use sanitized placeholders for examples.
- Keep bootstrap/secrets in private settings or environment-backed configuration.
- Move mutable runtime configuration into MongoDB only with an explicit design and migration path.

## Architecture

- Keep feature code under `Core/Features/`.
- Keep infrastructure code under `Core/Infrastructure/`.
- Keep shared cross-feature code under `Core/Shared/`.
- Keep service registration and host setup in `Program.cs`.
- Keep Discord.NET event wiring in the infrastructure layer.
- Keep feature rules in feature services rather than growing one central event handler indefinitely.
- Preserve async patterns and nullable-reference correctness.
- Keep MongoDB access behind the existing database/context and service boundaries.

## Current worktree state

The Players/Leveling split is in progress. Treat it as incomplete until verification proves otherwise.

Known affected areas:

- `Core/Features/Players/`
- `Core/Features/Leveling/`
- `Core/Abstractions/Interfaces/IPlayerService.cs`
- `Core/Features/Profiles/ProfileModule.cs`
- `Core/Infrastructure/Database/DatabaseContext.cs`
- `Core/Infrastructure/DiscordNet/Handlers/EventHandler.cs`
- `Core/Shared/Embeds/EmbedService.cs`
- `Program.cs`

Do not discard or reset these changes. Do not claim the refactor is complete without a successful build and review of affected profile/leveling flows.

## Verification

For code changes, run the smallest relevant checks and record exact results. The normal baseline is:

```text
dotnet build
```

When runtime behavior changes, manually check the affected Discord flows and confirm that no DM behavior was introduced. Persistence/model changes require compatibility or migration notes.

For documentation-only changes, verify that `AGENTS.md`, `docs/SPEC.md`, `docs/ROADMAP.md`, and `docs/TASKS.md` agree on product constraints, current work status, secrets handling, and verification expectations.
