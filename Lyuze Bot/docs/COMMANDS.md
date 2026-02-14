# Command Reference

Lyuze 3.0 provides a rich set of commands categorized by their functionality. This document lists all available slash commands, user commands, and message commands, along with their descriptions, parameters, and required permissions.

## Table of Contents

*   [Admin Commands](#admin-commands)
*   [Anime Commands](#anime-commands)
*   [Profile Commands](#profile-commands)
*   [Roles Commands](#roles-commands)

*   [Example Commands](#example-commands)

---

## Admin Commands

These commands are primarily for server administration and moderation.

| Command | Description | Parameters | Permissions |
| :------ | :---------- | :--------- | :---------- |
| `/purge <amount>` | Purges a specified `amount` of messages from the current channel within the last 14 days. | `amount` (int, default: 200) | Bot: `ManageMessages`<br>User: `ManageMessages` |
| `/kick <user> [reason]` | Kicks a specified user from the server. | `user` (User)<br>`reason` (string, optional, default: "No reason given.") | Bot: `KickMembers`<br>User: `KickMembers` |
| `/ban <user> <pruneDays> [reason]` | Bans a specified user from the server and optionally deletes their messages from the last `pruneDays`. | `user` (User)<br>`pruneDays` (int)<br>`reason` (string, optional, default: "No reason given.") | Bot: `BanMembers`<br>User: `BanMembers` |
| `/unban` | Initiates an interactive process to unban a user. Presents a select menu of up to 25 recently banned users. | None | Bot: `BanMembers`<br>User: `BanMembers` |
| `/unban-id <user-id> [reason]` | Unbans a user by their Discord ID. Useful for unbanning users not in the recent ban list. | `user-id` (string)<br>`reason` (string, optional) | Bot: `BanMembers`<br>User: `BanMembers` |
| `/kill` | Shuts down the bot process. | None | Bot Owner Only |
| `/test` | A test command, currently generates a welcome banner image. | None | Bot Owner Only |

---

## Anime Commands

Commands related to anime, image tracing, and waifu images.

| Command | Description | Parameters | Permissions |
| :------ | :---------- | :--------- | :---------- |
| `/aquote` | Gets a random anime quote. | None | Everyone |
| `/trace [image] [url]` | Finds the anime source from an image. Provide either an `image` attachment or a direct `url`. | `image` (Attachment, optional)<br>`url` (string, optional) | Everyone |
| `/sauce [image] [url]` | Finds the source of an image. Provide either an `image` attachment or a direct `url`. | `image` (Attachment, optional)<br>`url` (string, optional) | Everyone |
| `/waifu <tag>` | Gets a random waifu image based on a specified `tag`. | `tag` (string) | Everyone |

---

## Profile Commands

Commands for viewing and customizing user profiles.

| Command | Description | Parameters | Permissions |
| :------ | :---------- | :--------- | :---------- |
| `/profile [user]` | Views a user's profile. If no user is specified, it shows the command invoker's profile. | `user` (User, optional) | Everyone |
| `/setprofilebg <url>` | Sets your profile background image using a direct `url`. | `url` (string) | Everyone |
| `/setprofileaboutme <aboutme>` | Sets the text for your profile "About Me" section. | `aboutme` (string) | Everyone |
| `/setprofilestatus <ispublic>` | Sets whether your profile is public (`true`) or private (`false`). | `ispublic` (boolean) | Everyone |
| `/setplvlnotifications <isNotify>` | Sets whether you receive level-up notifications (`true`) or not (`false`). | `isNotify` (boolean) | Everyone |

---

## Roles Commands

Commands for managing Discord roles, especially reaction roles.

| Command | Description | Parameters | Permissions |
| :------ | :---------- | :--------- | :---------- |
| `/remove_role <user>` | Allows a user to remove a role from themselves, or an admin to remove a role from another user via a select menu. | `user` (User) | User: `ManageRoles` (for others) |
| `/setup_reaction_roles` | Sets up the initial embed message for reaction roles. The bot will store the message ID for future edits. | None | User: `Administrator` |
| `/add_reaction_roles <selectedEmoji> <role>` | Adds a new reaction role to the previously set up reaction roles message. Associates an `emoji` with a Discord `role`. | `selectedEmoji` (string)<br>`role` (Role) | User: `Administrator` |
| `/add_bot_reaction <selectedEmoji> <role>` | *Note: This command's description suggests it only adds a reaction, but its implementation also registers a reaction role. It appears functionally similar to `/add_reaction_roles`.* Adds a reaction to the reaction roles message and registers the associated role. | `selectedEmoji` (string)<br>`role` (Role) | User: `Administrator` |


---

## Example Commands

These commands are primarily for demonstration purposes of Discord.NET's interaction capabilities.

| Command | Type | Description | Parameters | Permissions |
| :------ | :--- | :---------- | :--------- | :---------- |
| `/ping` | Slash | A simple ping-pong command. | None | Everyone |
| `/components` | Slash | Demonstrates Discord components (buttons and selectable menus). | None | Everyone |
| `Add Switch Plays Role` | User | Adds a "Switch Plays" role to the selected user. | `user` (User, implicit) | Everyone |
| `Add Gamer Role` | User | Adds a "Gamer" role to the selected user. | `user` (User, implicit) | Everyone |
| `Add Weeb Role` | User | Adds a "Weeb" role to the selected user. | `user` (User, implicit) | Everyone |
| `Message Command Test` | Message | Responds with the author of the selected message. | `msg` (Message, implicit) | Everyone |
