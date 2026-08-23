# Configuration

Lyuze loads runtime configuration from:

```text
Resources/Settings/settings.json
```

The path is resolved relative to the application's executable directory. The file is private and must not be committed or shared.

## Configuration shape

The current settings model uses these top-level JSON sections:

```json
{
  "_Discord": {
    "Name": "Your bot name",
    "Token": "YOUR_DISCORD_BOT_TOKEN",
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
    "Unsplash Access": null,
    "Unsplash Secret": null,
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
    "MongoDB": "YOUR_MONGODB_CONNECTION_STRING",
    "Database": "LyuzeBot",
    "PlayerCollection": "Players"
  }
}
```

The example values above are placeholders. Replace them only in your private local settings file.

## Discord settings

### `_Discord`

| Property | Purpose |
|---|---|
| `Name` | Display name used by the installation. |
| `Token` | Discord bot token. Treat this as a secret. |
| `GuildID` | Guild where the bot registers and operates its configured commands. |

Never include a real token in a commit, screenshot, issue, pull request, or public message.

## Discord resource IDs

The `IDs` section stores Discord resources used by features such as welcome messages and reaction roles.

| Property | Purpose |
|---|---|
| `Owner ID` | Discord user ID used for owner-only behavior. |
| `Welcome ID` | Channel used for welcome messages. |
| `Report ID` | Configured report destination when used. |
| `Error ID` | Configured error destination when used. |
| `Kick ID` | Configured moderation-log destination when used. |
| `Leave ID` | Channel used for goodbye messages when implemented. |
| `DJ ID` | Configured DJ role when used. |
| `Join Role ID` | Role assigned to new members when configured. |
| `Reaction Roles Message ID` | Message used by reaction-role configuration. |

Guild and channel IDs are not credentials, but they still identify installation-specific resources. Do not publish your real server configuration unnecessarily.

## External APIs

The `APIs` section contains optional credentials for external providers.

| Property | Purpose |
|---|---|
| `Tenor` | Optional Tenor API key. |
| `Unsplash Access` | Optional Unsplash access key. |
| `Unsplash Secret` | Optional Unsplash secret. |
| `Sauce Nao` | Optional SauceNAO API key. |
| `Danbooru Login` | Optional Danbooru username for higher limits. |
| `Danbooru Api Key` | Optional Danbooru API key. |

Only configure providers used by your installation. Keep all credentials private.

## Image and message settings

- `Image Links`: General image URLs used by configured features.
- `Profile Banners`: Allowed/default profile background URLs.
- `Welcome Message`: Messages selected for new-member welcomes.
- `Goodbye Message`: Messages reserved for goodbye behavior.
- `Status`: Text values rotated through the bot presence.

Use HTTPS URLs from providers you trust. Avoid adding private or expiring URLs to a committed example file.

## Database settings

| Property | Purpose |
|---|---|
| `MongoDB` | MongoDB connection string. Secret when it contains credentials. |
| `Database` | Database name for this bot installation. |
| `PlayerCollection` | Collection storing player/profile documents. |

Each self-hosted installation owns its database. Player profiles and XP are keyed by Discord user ID, not by a general guild-plus-user partition.

## Safe configuration practices

- Keep `settings.json` out of public version control.
- Use environment-backed or secret-manager configuration when deploying outside a local development machine.
- Do not log tokens, API keys, passwords, or connection strings.
- Use narrow MongoDB network access rules.
- Use only the Discord permissions the enabled features require.
- Back up the database before changing collection names or model fields.
- When adding a new setting, update the settings model, the example structure, validation, and migration notes together.
