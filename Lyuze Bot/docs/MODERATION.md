# Moderation Commands

[&#8592; Back to README](../README.md) | [&#8592; Back to Commands List](./COMMANDS.md)

Lyuze 3.0 provides a suite of moderation commands to help server administrators manage their communities effectively. All moderation command responses are ephemeral, meaning only the user who invoked the command can see them.

## Commands

### `/purge`
- **Description**: Deletes a specified number of messages from the channel within the last 14 days.
- **Permissions**:
    - Bot: `Manage Messages`
    - User: `Manage Messages`
- **Behavior**:
    - Responses are ephemeral.
    - Can only be used in text channels.
    - Messages older than 14 days cannot be purged due to Discord API limitations.
- **Parameters**:
    - `amount`: The number of messages to delete (default: 200, maximum: 1000).

### `/kick`
- **Description**: Kicks a specified user from the server.
- **Permissions**:
    - Bot: `Kick Members`
    - User: `Kick Members`
- **Behavior**:
    - Responses are ephemeral.
- **Parameters**:
    - `user`: The user to kick (required).
    - `reason`: The reason for the kick (optional, defaults to "No reason given."). This reason will appear in the server's audit log.

### `/ban`
- **Description**: Bans a specified user from the server.
- **Permissions**:
    - Bot: `Ban Members`
    - User: `Ban Members`
- **Behavior**:
    - Responses are ephemeral.
- **Parameters**:
    - `user`: The user to ban (required).
    - `pruneDays`: The number of days to delete messages from the banned user (required).
    - `reason`: The reason for the ban (optional, defaults to "No reason given."). This reason will appear in the server's audit log.

### `/unban`
- **Description**: Unbans a user from the server using an interactive dropdown menu.
- **Permissions**:
    - Bot: `Ban Members`
    - User: `Ban Members`
- **Behavior**:
    - Responses are ephemeral.
    - Presents a dropdown menu listing up to 25 currently banned users.
    - If more than 25 users are banned, the command will suggest using `/unban-id` for users not listed in the dropdown.
    - The dropdown menu expires after 15 minutes.

### `/unban-id`
- **Description**: Unbans a user by their Discord ID. This command serves as a fallback for large ban lists or when a user is not present in the `/unban` dropdown.
- **Permissions**:
    - Bot: `Ban Members`
    - User: `Ban Members`
- **Behavior**:
    - Responses are ephemeral.
- **Parameters**:
    - `user-id`: The Discord ID of the user to unban (required, numeric).
    - `reason`: The reason for the unban (optional). This reason will appear in the server's audit log.
