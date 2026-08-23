# Lyuze Tasks

This is the compact repository-local task index. It records current status, verification evidence, and the next milestone. Detailed implementation instructions belong in the Obsidian vault task sheets.

Use `[ ]` for not started, `[~]` for in progress, and `[x]` only after the stated check is verified.

## Current task

### [x] Complete the Players/Leveling split

- **Branch:** `master` (the Players/Leveling refactor was carried into this cleanup worktree)
- **Detailed task sheet:** Project vault → `Task_Sheets/Task Sheet - Players Leveling Refactor.md`
- **Scope:** Complete and verify the current file/namespace/DI split without adding new leveling sources or integrations.
- **Repo status:** The working tree contains the intentional refactor and project-packet changes. Do not reset or clean it blindly.

### Verification evidence

- [x] `dotnet build "Lyuze 3.0.sln" --no-restore` passes.
- [x] Source check confirms `/setplvlnotifications` uses `UpdateLevelNotificationsAsync`.
- [x] `git diff --check` passes.
- [x] Detailed implementation and verification plan exists in the vault task sheet.
- [x] Dependency injection resolves the moved player and leveling services at runtime.
- [x] Profile creation and lookup work after the move.
- [x] Profile background, about-me, and public-status updates still work.
- [x] `/setplvlnotifications` behaves correctly at runtime and does not change `PublicProfile`.
- [x] Message XP cooldown and successful slash-command XP still work.
- [x] Profile embed level and XP display still work.
- [x] Bot users and DM channels do not receive XP.
- [x] Level-up notifications remain in-channel and never become DMs.
- [x] Existing player documents remain compatible after profile/leveling updates and a bot restart.

### User-confirmed runtime notes

- Profile flows were confirmed working in Discord.
- Leveling flows were confirmed working in Discord, including cooldowns and command XP.
- A real level-up notification was confirmed in the same channel where the user was chatting.
- MongoDB was observed updating the player record during profile and leveling runtime checks.
- The profile-private visibility issue was corrected with a temporary owner/admin validation bandaid. The authorization helper still has a follow-up TODO to return a boolean/result instead of an error embed.
- A dedicated level-up channel or mention-based notification is deferred. The current same-channel behavior is accepted for this refactor.

## Execution order

1. Finish the current Players/Leveling runtime and persistence verification.
2. Move player/application service boundaries toward `ulong` Discord IDs.
3. Replace the XP read-modify-replace path with atomic MongoDB updates.
4. Add focused automated or ad-hoc verification for the leveling foundation.
5. Begin the first new integration feature. GitHub is preferred, followed by Minecraft status and music.

## Deferred roadmap work

- Finish the profile visibility authorization TODO in `ProfileService`.
- Configuration validation and migration.
- Discord event-handler split under `Core/Infrastructure/DiscordNet/Handlers/Events/`.
- Microsoft `ILogger` migration and the custom `ConsoleFormatter`.
- Generic RSS/Webhook notification infrastructure.
- Discord AutoMod configuration and audit integration.
- User-facing leveling commands, restrictions, anti-farming, voice XP, and reaction XP.
- Reaction-role persistence and repair hardening.

## Tracking rules

- Keep detailed task instructions, design reasoning, file maps, call flows, smoke-test procedures, risks, and rollback guidance in vault task sheets.
- Keep this file short enough to read at the start of a repository session.
- Update a checkbox only when the corresponding evidence exists.
- Keep `docs/ROADMAP.md` as the committed project direction and this file as the current implementation/verification index.
- Do not copy secrets, tokens, passwords, or real settings into this file.
