# Configuration

Lyuze 3.0 uses a `settings.json` file to manage all its configuration. This file is automatically created with default values if it doesn't exist when the bot starts.

## Location

The `settings.json` file is located in the `Resources/Settings/` directory, relative to the application's base directory.

Example path: `D:\_Programming\DiscordBots\CS-Lyuze\Lyuze 3.0\Lyuze Bot\Resources\Settings\settings.json`

## Structure

The `settings.json` file is a JSON document with several key sections:

```json
{
  "_Discord": {
    "Name": "Default Discord Name",
    "Token": "Default Discord Token",
    "GuildID": 0
  },
  "IDs": {
    "Owner ID": 0,
    "Welcome ID": 0,
    "Report ID": 0,
    "Error ID": 0,
    "Kick ID": 0,
    "Leave ID": 0,
    "DJ ID": 0,
    "Join Role ID": 0,
    "Reaction Roles Message ID": 0
  },
  "APIs": {
    "Tenor": null,


    "Sauce Nao": null,
    "Danbooru Login": null,
    "Danbooru Api Key": null
  },
  "Image Links": [],
  "Profile Banners": [],
  "Welcome Message": [],
  "Goodbye Message": [],
  "Status": [],
  "Database": {
    "MongoDB": "Default Mongodb",
    "Database": "Default Database Name",
    "PlayerCollection": "Default PlayerCollection"
  },

}
```

## Key Configuration Sections

### Discord Settings (`_Discord`)

*   `Name`: The name of your Discord bot.
*   `Token`: **Your Discord bot's authentication token.** This is a critical secret and should be kept confidential. Obtain this from the [Discord Developer Portal](https://discord.com/developers/applications).
*   `GuildID`: The ID of the primary Discord guild (server) where the bot will register its slash commands and operate.

### ID Settings (`IDs`)

This section contains various Discord IDs for specific channels, roles, or users that the bot interacts with. Replace the `0` values with the actual IDs from your Discord server.

*   `Owner ID`: The Discord User ID of the bot's owner. Used for owner-only commands.
*   `Welcome ID`: The Discord Channel ID where welcome messages/banners should be sent.
*   `Report ID`: The Discord Channel ID for sending reports or moderation logs.
*   `Error ID`: The Discord Channel ID for sending bot error notifications.
*   `Kick ID`: The Discord Channel ID for logging kick actions.
*   `Leave ID`: The Discord Channel ID where goodbye messages should be sent.
*   `DJ ID`: The Discord Role ID for DJ-specific functionalities (if any).
*   `Join Role ID`: The Discord Role ID that new users will automatically receive upon joining the server.
*   `Reaction Roles Message ID`: The Discord Message ID of the message used for reaction roles. This is set by the `/setup_reaction_roles` command.

### API Keys (`APIs`)

This section holds API keys for various external services the bot integrates with. **These are sensitive and should be kept confidential.**

*   `Tenor`: API key for Tenor GIF service.


*   `Sauce Nao`: API key for SauceNao image source tracing.
*   `Danbooru Login`: Login username for Danbooru (optional, for higher rate limits).
*   `Danbooru Api Key`: API key for Danbooru (optional, for higher rate limits).

### Image Links (`Image Links`, `Profile Banners`)

*   `Image Links`: A list of `Uri`s (URLs) to general-purpose images the bot might use.
*   `Profile Banners`: A list of `Uri`s (URLs) to images available for user profile backgrounds.

### Messages (`Welcome Message`, `Goodbye Message`, `Status`)

*   `Welcome Message`: A list of strings. One will be randomly selected and used in welcome messages for new users.
*   `Goodbye Message`: A list of strings. One will be randomly selected and used in goodbye messages when users leave.
*   `Status`: A list of strings. The bot will cycle through these strings as its "playing" or "listening to" status on Discord.

### Database Settings (`Database`)

*   `MongoDB`: **The MongoDB connection string.** This is a critical secret. Example: `mongodb://localhost:27017` or a cloud connection string.
*   `Database`: The name of the database to use within your MongoDB instance.
*   `PlayerCollection`: The name of the collection used to store player profile data.

## Database Settings (`Database`)

This section defines the connection parameters for the bot's MongoDB database.

*   `MongoDB`: **The MongoDB connection string.** This is a critical secret. Example: `mongodb://localhost:27017` or a cloud connection string.
*   `Database`: The name of the database to use within your MongoDB instance.
*   `PlayerCollection`: The name of the collection used to store player profile data.

For detailed, step-by-step instructions on setting up a MongoDB Atlas Free Tier cluster and obtaining your connection string, please refer to [`docs/SETUP.md#mongodb-atlas-free-tier`](./docs/SETUP.md#mongodb-atlas-free-tier).





## How to Add API Keys and Secrets

1.  Open your `settings.json` file.
2.  Locate the relevant section (e.g., `APIs`, `_Discord`, `Database`).
3.  Replace the `null` or default string values with your actual API keys, tokens, or connection strings.
    *   **Example for Discord Token:**
        ```json
        "_Discord": {
          "Name": "My Awesome Bot",
          "Token": "YOUR_DISCORD_BOT_TOKEN_HERE",
          "GuildID": 123456789012345678
        }
        ```
    *   **Example for MongoDB Connection:**
        ```json
        "Database": {
          "MongoDB": "mongodb+srv://user:password@cluster.mongodb.net/mydb?retryWrites=true&w=majority",
          "Database": "LyuzeBotDb",
          "PlayerCollection": "Players"
        }
        ```
4.  Save the `settings.json` file.
5.  Restart the bot for the changes to take effect.

**Remember:** Treat your `settings.json` file as highly confidential. Do not commit it to public version control systems.
