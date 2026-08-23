# Setup

This guide covers the prerequisites and first-run setup for a self-hosted Lyuze 3.0 installation.

## Prerequisites

Install:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or newer.
- [Git](https://git-scm.com/downloads).
- A Discord account that can create and configure an application.
- A MongoDB instance. MongoDB Atlas or a self-hosted MongoDB server can be used.

## Create the Discord application

1. Open the [Discord Developer Portal](https://discord.com/developers/applications).
2. Create an application.
3. Open the **Bot** page and add a bot user.
4. Copy the bot token into your private runtime settings. Never commit or share it.
5. Enable the gateway intents required by the features you use. Message leveling requires message-related access, and member/welcome behavior requires member access.
6. Create an invite under **OAuth2 → URL Generator**.
7. Select the `bot` and `applications.commands` scopes.
8. Grant only the permissions the installation needs. Avoid using `Administrator` outside temporary development testing.

The bot registers commands for the configured guild ID during startup.

## Clone and build

```bash
git clone https://github.com/ProjektCode/Lyuze-3.0.git
cd Lyuze-3.0
dotnet restore "Lyuze 3.0.sln"
dotnet build "Lyuze 3.0.sln" --no-restore
```

## First run and settings

The application looks for `Resources/Settings/settings.json` relative to its executable directory. `SettingsService` creates a default template when the file does not exist.

Before starting the bot, configure the private values described in [Configuration](CONFIGURATION.md).

Run the bot with:

```bash
dotnet run --project "Lyuze 3.0.csproj"
```

Do not commit the generated runtime settings file. Keep tokens, API keys, and MongoDB connection strings outside public documentation and source control.

## MongoDB

You may use MongoDB Atlas or a self-hosted MongoDB instance.

For MongoDB Atlas:

1. Create an Atlas account and project.
2. Create a cluster appropriate for your installation.
3. Create a database user with only the access required by the bot.
4. Add the bot host's network location to the Atlas access list. Prefer a narrow address or private route over `0.0.0.0/0`.
5. Copy the application connection string into the private `Database.MongoDB` setting.
6. Choose a database name and player collection name.

The bot creates and uses its configured player collection. Back up the database before changing production settings or model fields.

## Verify the installation

1. Run `dotnet build "Lyuze 3.0.sln"`.
2. Start the bot with `dotnet run --project "Lyuze 3.0.csproj"` or your IDE.
3. Confirm the process connects to Discord without configuration or permission errors.
4. Confirm commands register in the configured guild.
5. Run `/help`.
6. Run `/profile` and confirm a player document can be created or read.
7. Check the console for startup and database errors without sharing the output publicly if it contains installation details.

## Troubleshooting

### The bot does not connect

- Confirm the token is current and stored in the private settings file.
- Confirm the bot is invited to the intended guild.
- Confirm the required gateway intents are enabled.
- Confirm the host can reach Discord and MongoDB.

### Commands do not appear

- Confirm the `applications.commands` scope was included in the invite.
- Confirm the configured guild ID is correct.
- Restart the bot and review command-registration logs.

### MongoDB connection fails

- Verify the connection string and database user.
- Verify the database host allows the bot host's network location.
- Confirm the database name and player collection are present in the settings file.
- Do not paste the connection string into an issue or public chat.
