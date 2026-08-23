# Lyuze 3.0 Discord bot

Lyuze 3.0 is a self-hosted C# Discord bot built with Discord.NET, the .NET Generic Host, dependency injection, and MongoDB.

The project is intended for personal or small-server use. Each installation owns its own bot configuration and database. Player profiles and XP are keyed by Discord user ID, and the bot does not use DMs for notifications.

## Current features

- Administration and moderation commands.
- User profiles and message/slash-command leveling.
- Reaction roles.
- Anime quotes and image-source lookup.
- Profile backgrounds and about-me information.
- MongoDB persistence.
- Generic Host and dependency injection.

## Documentation

- [Setup](docs/SETUP.md)
- [Configuration](docs/CONFIGURATION.md)
- [Commands](docs/COMMANDS.md)
- [Moderation](docs/MODERATION.md)
- [Anime and image search](docs/ANIME-IMAGE-SEARCH.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Development](docs/DEVELOPMENT.md)
- [Specification](docs/SPEC.md)
- [Roadmap](docs/ROADMAP.md)

`docs/TASKS.md` is an internal repository task index. Detailed implementation task sheets are maintained separately in the project vault and are not public deployment documentation.

## Prerequisites

- .NET 8 SDK or newer.
- Git.
- A Discord application and bot token.
- A MongoDB instance.

## Quick start

Clone the repository and enter its root directory:

```bash
git clone https://github.com/ProjektCode/Lyuze-3.0.git
cd Lyuze-3.0
```

Create or run the settings template, configure the private runtime values described in [Configuration](docs/CONFIGURATION.md), then build and run:

```bash
dotnet build "Lyuze 3.0.sln"
dotnet run --project "Lyuze 3.0.csproj"
```

Never commit the runtime `settings.json` file or paste its contents into issues, pull requests, public documentation, or chat.

## Contributing

Read [Development](docs/DEVELOPMENT.md) before changing code. Keep Discord.NET event wiring under `Core/Infrastructure/DiscordNet/Handlers`, keep feature rules in feature services, and run the build before reporting a change as complete.
