# Moderation commands

Lyuze includes basic administration and moderation commands. Responses are normally ephemeral, while the Discord server's audit log records supported moderation actions when a reason is supplied.

The command names below use the current `/admin` command group.

## Permissions and safety

Before using moderation commands, confirm:

- The bot has the required Discord permission.
- The bot's role is higher than the target member's role.
- The target is not the server owner or otherwise protected by Discord.
- The reason does not contain secrets or unnecessary private information.
- The configured moderation/log destination is accessible only to the intended staff.

## Commands

### `/admin purge [amount]`

Deletes recent messages from the current text channel.

- Required bot permission: `Manage Messages`.
- Required user permission: `Manage Messages`.
- Default amount: 200.
- Maximum amount: 1000.
- Discord's bulk-delete age limit still applies.
- The command can only run in a text channel.

### `/admin kick <user> [reason]`

Kicks a guild member.

- Required bot permission: `Kick Members`.
- Required user permission: `Kick Members`.
- The reason is sent as the Discord audit-log reason.
- The command does not DM the target user.

### `/admin ban <user> <pruneDays> [reason]`

Bans a guild member and optionally removes recent messages according to `pruneDays`.

- Required bot permission: `Ban Members`.
- Required user permission: `Ban Members`.
- The reason is sent as the Discord audit-log reason.
- Validate the prune value and target hierarchy before execution.

### `/admin unban`

Opens a select menu containing up to 25 banned users.

- Required bot permission: `Ban Members`.
- Required user permission: `Ban Members`.
- The interaction is temporary and expires according to the component handler behavior.
- Use `/admin unban-id` when the target is not in the displayed list.

### `/admin unban-id <user-id> [reason]`

Unbans a user by numeric Discord ID.

- Required bot permission: `Ban Members`.
- Required user permission: `Ban Members`.
- The reason is sent as the Discord audit-log reason.
- The user ID must parse as a Discord snowflake.

### Owner-only administration

The repository currently contains `/admin kill` and `/admin test` owner-only commands. These are development/operations commands and should not be exposed casually in a public server.

## Planned moderation

The roadmap includes timeout, warnings, slowmode, lock/unlock, modlog, cases, and Discord AutoMod integration. Those features are planned and are not represented as completed behavior by this document.

Lyuze should use Discord's built-in AutoMod for supported keyword, spam, mention-spam, preset, and profile rules instead of duplicating every filter in custom message code. Custom checks such as invite filtering must be coordinated with AutoMod to avoid duplicate actions.
