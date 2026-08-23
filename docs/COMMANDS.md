# Command reference

This page lists the commands currently defined in the repository. Grouped commands are shown using Discord's group/subcommand form.

Command registration is currently configured for the guild ID in the private runtime settings.

## General commands

| Command | Description | Access |
|---|---|---|
| `/help` | Opens the ephemeral help interface. | Everyone |
| `/ping` | Development/example ping command when enabled. | Everyone |
| `/components` | Development/example component command. | Everyone |

## Administration commands

These commands are grouped under `/admin`.

| Command | Description | Access |
|---|---|---|
| `/admin purge [amount]` | Deletes recent messages within Discord's bulk-delete age limit. | Manage Messages |
| `/admin kick <user> [reason]` | Kicks a guild member. | Kick Members |
| `/admin ban <user> <pruneDays> [reason]` | Bans a guild member and optionally prunes recent messages. | Ban Members |
| `/admin unban` | Opens a select menu for recently banned users. | Ban Members |
| `/admin unban-id <user-id> [reason]` | Unbans a user by numeric Discord ID. | Ban Members |
| `/admin kill` | Stops the bot process. | Bot owner |
| `/admin test` | Runs an owner-only image/banner test. | Bot owner |

All moderation actions should use a reason and should be tested against the bot's role hierarchy and permissions.

## Anime and image commands

These commands are grouped under `/anime`.

| Command | Description | Access |
|---|---|---|
| `/anime aquote` | Returns a random anime quote. | Everyone |
| `/anime trace [image] [url]` | Attempts to identify an anime scene from an upload or URL. | Everyone |
| `/anime sauce [image] [url]` | Attempts to find the source of an image. | Everyone |
| `/anime waifu <tag>` | Requests a tagged image from the configured provider. | Everyone |

See [Anime and image search](ANIME-IMAGE-SEARCH.md) for provider behavior and troubleshooting.

## Profile commands

These commands are grouped under `/profile`.

| Command | Description | Access |
|---|---|---|
| `/profile profile [user]` | Displays a user's profile. | Everyone |
| `/profile setprofilebg <url>` | Sets a profile background URL. | Everyone |
| `/profile setprofileaboutme <aboutme>` | Sets the profile About Me text. | Everyone |
| `/profile setprofilestatus <ispublic>` | Sets profile visibility. | Everyone |
| `/profile setplvlnotifications [user]` | Toggles in-channel level-up notifications. It is not the profile-visibility setting. | Everyone; verify target-user permissions before enabling delegated use |

The current level-notification command is a toggle. It does not use an `isNotify` boolean argument.

The bot must deliver level-up notifications in a configured guild channel or interaction context, never by DM.

## Role commands

These commands are grouped under `/roles`.

| Command | Description | Access |
|---|---|---|
| `/roles remove_role <user>` | Removes a selected role through the role-management flow. | Depends on target/role permissions |
| `/roles setup_reaction_roles` | Creates the reaction-role configuration message. | Administrator |
| `/roles add_reaction_roles <selectedEmoji> <role>` | Adds an emoji-to-role mapping. | Administrator |
| `/roles add_bot_reaction <selectedEmoji> <role>` | Adds a reaction and currently overlaps with reaction-role setup. | Administrator |

Reaction-role configuration is still being hardened. Verify role hierarchy, managed roles, message existence, and duplicate mappings before using it in production.

## Command registration notes

- Commands are discovered from the entry assembly by the interaction handler.
- Commands are registered on the configured guild during the Discord ready flow.
- Discord permissions and preconditions are part of the command behavior.
- Some demonstration commands may remain in the repository while the project is under development.
