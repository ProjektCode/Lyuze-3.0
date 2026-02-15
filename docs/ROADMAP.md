# Lyuze Bot Roadmap

Welcome to the Lyuze Bot roadmap! This document outlines exciting features and improvements planned for the bot. Your feedback is always welcome as we build the best bot experience together!

## Mini Games
- [ ] Rock, Paper, Scissors
- [ ] Hangman
- [ ] Tic-Tac-Toe
- [ ] War
- [ ] Trivia Quizzes
- [ ] Integrate JokesAPI for fun jokes

## Leveling System
- [ ] Implement a comprehensive leveling system
- [ ] Assign roles automatically at certain levels
- [ ] Create customizable profile cards for users

## Image Based Commands
- [ ] Integrate Waifu.pics for various image commands
- [ ] Integrate Nekos.Best for more image commands
- [ ] Integrate OtakuGifs for animated GIF commands

## Anime & Otaku Features
- [ ] `/anime search` / `/anime info`: Find and view detailed information about anime series (AniList or Jikan.Net API).
- [ ] `/manga search` / `/manga info`: Find and view detailed information about manga series (AniList or Jikan.Net API).
- [ ] `/character search` / `/character info`: Find and view details about anime/manga characters.
- [ ] `/staff search` / `/staff info`: Find and view details about anime/manga production staff.
- [ ] `/studio search` / `/studio info`: Find and view details about anime/manga animation studios.
- [ ] `/seasonal` / `/airing` / `/schedule`: Get information on currently airing or upcoming seasonal anime (AniList or Jikan.Net API).
- [ ] `/watchlist add` / `/watchlist remove` / `/watchlist list`: Track your personal anime/manga watchlists.
- [ ] `/notify add` / `/notify remove` / `/notify list`: Subscribe to notifications for airing and release dates of anime/manga.

## Utility Commands
- [ ] `/help`: Display a paginated list of commands and their uses.
- [ ] `/ping`: Check the bot's response time and latency.
- [ ] `/uptime`: Show how long the bot has been running since its last restart.
- [ ] `/botinfo`: Provide general information about the bot, its version, and developer.
- [ ] `/invite`: Get the official invite link to add the bot to your server.

## Info Commands
- [ ] `/serverinfo`: Display detailed information about the current Discord server.
- [ ] `/userinfo`: Show comprehensive information about a specific user.
- [ ] `/avatar`: Get the full-sized avatar of a user.
- [ ] `/banner`: Retrieve the banner image of a user.

## Setup & Config Commands
- [ ] `/setup`: Initialize essential bot features and default settings for your server.
- [ ] `/config get`: View the current value of a specific configuration setting.
- [ ] `/config set`: Change the value of a configuration setting for your server.
- [ ] `/config reset`: Reset a specific configuration setting to its default value.
- [ ] `/config validate`: Check your server's configuration for potential issues or missing settings.
- [ ] `/config export`: Export your server's current configuration to a file.
- [ ] `/config import`: Import configuration settings from a file to your server.

## Feature Flags
- [ ] `/feature list`: Display all available feature flags and their current status.
- [ ] `/feature enable`: Activate a specific feature flag for your server.
- [ ] `/feature disable`: Deactivate a specific feature flag for your server.

## Community
- [ ] `/tag create`: Create a new custom tag with specified content.
- [ ] `/tag edit`: Modify the content of an existing tag.
- [ ] `/tag delete`: Remove a custom tag.
- [ ] `/tag list`: View all available custom tags.
- [ ] `/tag <name>`: Use a custom tag by typing its name to display its content.
- [ ] `/suggest`: Submit a suggestion or idea for the server or bot.
- [ ] `/suggestions list`: View all submitted suggestions and their status.
- [ ] `/suggestions approve`: Approve a submitted suggestion.
- [ ] `/suggestions deny`: Deny a submitted suggestion.
- [ ] `/poll`: Create an interactive poll for server members to vote on.
- [ ] `/vote`: Cast your vote on an active poll.
- [ ] `/giveaway`: Start a giveaway for server members.

## Starboard
- [ ] `/starboard set`: Configure the channel where starred messages will be posted.
- [ ] `/starboard show`: Display the current settings for the starboard feature.
- [ ] `/starboard threshold`: Set the minimum number of reactions a message needs to appear on the starboard.

## Reaction Roles
- [ ] `/setup_reaction_roles`: Set up a new message for reaction roles.
- [ ] `/add_reaction_roles`: Add a new reaction role to an existing reaction role message.
- [ ] `/remove_role`: Remove a specific reaction role from a message.
- [ ] `/reactionroles add`: (Alias/Cleanup) Add a reaction role to a message.
- [ ] `/reactionroles remove`: (Alias/Cleanup) Remove a reaction role from a message.
- [ ] `/reactionroles list`: (Alias/Cleanup) List all reaction roles configured.

## Reminders & Scheduling
- [ ] `/timezone set`: Store your timezone in your profile; validates against IANA tzdb IDs. If invalid, provides a source (e.g., Wikipedia tz database list or timeanddate) to find your timezone.
- [ ] `/timezone show`: Display your currently set timezone.
- [ ] `/remind`: Set a personal reminder. This command requires your timezone to be set first.
- [ ] `/event create`: Create a Discord Scheduled Event using Discord's Scheduled Events API.
- [ ] `/event list`: List upcoming Discord Scheduled Events using Discord's Scheduled Events API.


## Self-hosted Ops
- [ ] `/health`: Check the bot's operational status and core service health.
- [ ] `/diagnostics`: Get a snapshot of runtime, memory, database, and queue status for troubleshooting.
- [ ] `/logs tail`: View recent bot logs. Note: Currently logs to console; proposed approach is keeping an in-memory ring buffer or adding a file sink for tailing.
- [ ] `/latency`: Measure the bot's gateway and API response times to Discord.
- [ ] `/backup create`: Create a backup of bot configuration and data.
- [ ] `/backup list`: List available backups.
- [ ] `/backup restore`: Restore bot configuration and data from a selected backup.

## Moderation

### Planned Moderation Commands
- [ ] `timeout` / `untimeout`: Temporarily restrict a user's ability to send messages, react, or join voice channels.
- [ ] `warn` / `warnings` / `clear-warnings`: Manage user warnings, view warning history, and clear warnings.
- [ ] `slowmode`: Set a slowmode for a channel, limiting how frequently users can send messages.
- [ ] `lock` / `unlock`: Lock hides the channel from average users by denying `ViewChannel`, allows mods, caches current overwrites, and restores them on unlock.
- [ ] `softban`: Kick a user from the server, delete their recent messages, and immediately unban them (convenience for `/ban` with prune days).
- [ ] `massban`: Ban multiple users from the server at once.
- [ ] `modlog set` / `modlog show`: Configure and view moderation logs for your server.
- [ ] `nick` / `reset-nick`: Change a user's nickname or reset it to their original username.
- [ ] `role add` / `role remove`: Add or remove roles from a user.
- [ ] `purge user` / `purge contains (filters)`: Purge messages by a specific user or messages containing certain keywords/filters.
- [ ] `case view` / `case edit-reason` / `case delete`: View, edit the reason for, or delete moderation cases.

### Already Implemented Moderation Commands
These commands are already part of Lyuze Bot's moderation toolkit:
- [x] `purge`: Delete a specified number of messages from a channel.
- [x] `kick`: Remove a user from the server.
- [x] `ban`: Permanently ban a user from the server.
- [x] `unban`: Unban a user by their ID, allowing them to rejoin the server.

## Miscellaneous
- [ ] Implement a quote system for users to save and retrieve quotes.

## Database & Configuration (MongoDB)

Our goal is to enhance Lyuze Bot's flexibility and scalability by migrating most configuration options from `settings.json` to a robust MongoDB database. This will allow for dynamic, per-guild settings and easier management.

- [ ] Implement a robust, schema-driven configuration system using MongoDB.
  - Status: Partial
  - Evidence:
    - `Core/Infrastructure/Database/DatabaseContext.cs` creates Mongo client and uses `SettingsConfig.Database.MongoDb` and DB name.
    - `Core/Features/Profiles/PlayerService.cs` and `Core/Features/Roles/ReactionRolesService.cs` store data in Mongo.
    - No general config collections like `GuildSettings` exist yet.
    - `SettingsConfig.cs` defines the schema for `settings.json`, but not directly for MongoDB config.
- [ ] Migrate existing hard-coded settings and `settings.json` values to MongoDB.
  - Status: Partial
  - Evidence:
    - `Core/Infrastructure/Database/DatabaseContext.cs` uses `SettingsConfig.Database.MongoDb` and `SettingsConfig.Database.DatabaseName`.
    - `SettingsConfig.Database.PlayerCollection` is used for `Players` collection name.
    - `ReactionRoles` collection is hard-coded "ReactionRoles".
    - `IDs.ReactionRoleMessageId` is still in `settings.json` and set by `/setup_reaction_roles` in `Core/Features/Roles/RolesModule.cs`.
    - Many other `SettingsConfig` fields are still in `settings.json`.
- [ ] Develop an efficient caching strategy (in-memory with intelligent invalidation) to minimize database round-trips and maintain high performance.
  - Status: Partial
  - Evidence:
    - `Core/Features/Roles/ReactionRolesService.cs` loads all mappings into an in-memory dictionary and updates on add, providing caching for reaction roles.
- [ ] Create an administrative interface or commands for managing configuration settings in real-time.
  - Status: Not started
  - Evidence: No evidence found for this.
- [ ] Define a clear configuration schema and collections (e.g., `GuildSettings`, `BotSettings`) with versioning and migration strategies to ensure data integrity across updates.
  - Status: Not started
  - Evidence: No general config collections like `GuildSettings` exist yet.
- [ ] Implement administrative commands within Discord for seamless management of settings (view, set, reset) by authorized users.
  - Status: Not started
  - Evidence: No evidence found for this.
- [ ] Create a safe and reliable migration plan to import existing `settings.json` values into MongoDB, ensuring no sensitive information (like tokens or API keys) is logged during the process.
  - Status: Not started
  - Evidence: No evidence found for this.
- [ ] Clearly delineate which configurations remain file/environment-based (e.g., secrets, tokens, API keys) and which will be managed in the database (e.g., guild-specific options, feature toggles, channel/role IDs, thresholds).
  - Status: Not started
  - Evidence: This document aims to start this delineation.
- [ ] Build tools for easy backup and export of the database configuration.
  - Status: Not started
  - Evidence: No evidence found for this.
- [ ] Implement comprehensive tests and verification steps for all database interactions and configuration management features.
  - Status: Not started
  - Evidence: No tests project found.

### SettingsConfig Migration Map

This section maps every field in `Core/Infrastructure/Configuration/SettingsConfig.cs` to its proposed storage location (file/environment or MongoDB), along with a rationale and suggested target document/collection name where applicable.

#### Suggested First DB-Migrated Fields (Low Risk)

These fields are ideal candidates for initial migration due to their low risk (non-secret, frequently changed, per-guild scope) and immediate benefit for dynamic configuration. Secrets like API keys and tokens should always remain in file/environment variables.

- `SettingsConfig.IDs.WelcomeId`: Channel ID for welcome messages, often customized per guild.
- `SettingsConfig.IDs.ReportId`: Channel ID for reports, can vary per guild and be managed by admins.
- `SettingsConfig.IDs.ErrorId`: Channel ID for error logging, useful for per-guild customization.
- `SettingsConfig.IDs.ReactionRoleMessageId`: Message ID for reaction roles, frequently updated by admins.
- `SettingsConfig.WelcomeMessage`: Customizable welcome message template, high user impact.
- `SettingsConfig.GoodbyeMessage`: Customizable goodbye message template, high user impact.
- `SettingsConfig.Status`: Bot's status message, frequently updated for events or maintenance.
- `SettingsConfig.ImageLinks`: URLs for various images, often updated.
- `SettingsConfig.ProfileBanners`: URLs for profile banners, often updated.

- **`SettingsConfig.Discord`**
  - `DiscordStuff.Name`: `Keep in file/env`. Rationale: Bot's name is fundamental, rarely changes, and part of bootstrapping.
  - `DiscordStuff.Token`: `Keep in file/env`. Rationale: Secret, critical for bot operation, should not be in DB.
  - `DiscordStuff.GuildId`: `Keep in file/env`. Rationale: Core to the bot's operation on a specific guild, bootstrapping. (If multi-guild support is added, this would move to DB per-guild settings).

- **`SettingsConfig.IDs`**
  - `OwnerId`: `Keep in file/env`. Rationale: Identifies the bot owner, critical for administrative commands, bootstrapping.
  - `WelcomeId`: `Move to DB`. Rationale: Channel ID that can change per guild or be managed by admins. Target: `GuildSettings` collection, `DiscordChannelSettings` document.
  - `ReportId`: `Move to DB`. Rationale: Channel ID that can change per guild or be managed by admins. Target: `GuildSettings` collection, `DiscordChannelSettings` document.
  - `ErrorId`: `Move to DB`. Rationale: Channel ID that can change per guild or be managed by admins. Target: `GuildSettings` collection, `DiscordChannelSettings` document.
  - `KickId`: `Move to DB`. Rationale: Channel ID that can change per guild or be managed by admins. Target: `GuildSettings` collection, `DiscordChannelSettings` document.
  - `LeaveId`: `Move to DB`. Rationale: Channel ID that can change per guild or be managed by admins. Target: `GuildSettings` collection, `DiscordChannelSettings` document.
  - `DjId`: `Move to DB`. Rationale: Role ID that can change per guild or be managed by admins. Target: `GuildSettings` collection, `DiscordRoleSettings` document.
  - `JoinRoleId`: `Move to DB`. Rationale: Role ID that can change per guild or be managed by admins. Target: `GuildSettings` collection, `DiscordRoleSettings` document.
  - `ReactionRoleMessageId`: `Move to DB`. Rationale: Specific message ID tied to a feature, can change, and is managed by admins. Evidence: Currently in `settings.json` and set by `/setup_reaction_roles` in `Core/Features/Roles/RolesModule.cs`. Target: `ReactionRoles` collection (or a dedicated `FeatureSettings` document within `GuildSettings`).

- **`SettingsConfig.ApIs`**
  - `Tenor`: `Keep in file/env`. Rationale: API key, secret, bootstrapping.
  - `UnsplashAccess`: `Keep in file/env`. Rationale: API key, secret, bootstrapping. Note: planned removal.
  - `UnsplashSecret`: `Keep in file/env`. Rationale: API key, secret, bootstrapping. Note: planned removal.
  - `SauceNao`: `Keep in file/env`. Rationale: API key, secret, bootstrapping.
  - `DanbooruLogin`: `Keep in file/env`. Rationale: API credential, secret, bootstrapping.
  - `DanbooruApiKey`: `Keep in file/env`. Rationale: API key, secret, bootstrapping.

- **`SettingsConfig.ImageLinks`**: `Move to DB`. Rationale: URLs for various images used by the bot, can be updated by admins. Target: `BotSettings` or `ImageSettings` collection/document.

- **`SettingsConfig.ProfileBanners`**: `Move to DB`. Rationale: URLs for profile banners, can be updated by admins. Target: `BotSettings` or `ImageSettings` collection/document.

- **`SettingsConfig.WelcomeMessage`**: `Move to DB`. Rationale: Customizable message, can be updated by admins. Target: `GuildSettings` collection, `WelcomeMessageSettings` document.

- **`SettingsConfig.GoodbyeMessage`**: `Move to DB`. Rationale: Customizable message, can be updated by admins. Target: `GuildSettings` collection, `GoodbyeMessageSettings` document.

- **`SettingsConfig.Status`**: `Move to DB`. Rationale: Bot's status message, can be updated by admins. Target: `BotSettings` or `StatusSettings` collection/document.

- **`SettingsConfig.Database`**
  - `MongoDb`: `Keep in file/env`. Rationale: Connection string, critical for bootstrapping database access, should not be in DB.
  - `DatabaseName`: `Keep in file/env`. Rationale: Core database name, bootstrapping.
  - `PlayerCollection`: `Keep in file/env`. Rationale: Collection name, part of database schema definition, bootstrapping. Evidence: `Players` collection name from `SettingsConfig.Database.PlayerCollection`.

