# Architecture

Lyuze is a self-hosted C# Discord bot built with the .NET Generic Host, dependency injection, Discord.NET, and MongoDB.

## Core boundaries

- **Features:** User-facing modules and feature services under `Core/Features/`.
- **Infrastructure:** Discord.NET event wiring, startup, configuration, database, HTTP, and logging under `Core/Infrastructure/`.
- **Shared:** Reusable cross-feature helpers under `Core/Shared/`.
- **Abstractions:** Cross-feature interfaces under `Core/Abstractions/Interfaces/`.
- **Persistence:** MongoDB access through `DatabaseContext` and feature/application services.

## Current structure

```text
Core/
├── Abstractions/
│   └── Interfaces/
├── Features/
│   ├── Admin/
│   ├── Anime/
│   ├── Examples/
│   ├── Leveling/
│   │   └── Services/
│   │       └── LevelingService.cs
│   ├── Players/
│   │   ├── Models/
│   │   │   └── PlayerModel.cs
│   │   └── Services/
│   │       └── PlayerService.cs
│   ├── Profiles/
│   ├── Roles/
│   └── Utility/
├── Infrastructure/
│   ├── Configuration/
│   ├── Database/
│   ├── DiscordNet/
│   │   └── Handlers/
│   ├── Http/
│   └── Logging/
└── Shared/
    ├── Embeds/
    ├── Images/
    └── Status/
```

## Application startup

`Program.cs` creates the Generic Host and registers services with dependency injection.

The main startup path is:

1. `Host.CreateDefaultBuilder()` creates the application host.
2. `ConfigureServices` registers configuration, database, Discord.NET, feature services, and handlers.
3. `DiscordStartupService` starts and stops the Discord client.
4. `InteractionHandler` discovers and executes interaction modules.
5. `EventHandler` subscribes to Discord.NET events that are not handled directly by `InteractionService`.

## Discord event flow

### Slash command

```text
Discord Gateway
    -> DiscordSocketClient.InteractionCreated
    -> InteractionHandler
    -> InteractionService
    -> Feature module
    -> Feature service
    -> MongoDB or external API when needed
    -> Discord interaction response
```

### Message XP

```text
DiscordSocketClient.MessageReceived
    -> Infrastructure EventHandler
    -> bot/DM eligibility checks
    -> Players service for profile lookup
    -> LevelingService for cooldown and XP behavior
    -> MongoDB persistence
    -> in-channel level-up notification when enabled
```

### Member join

```text
DiscordSocketClient.UserJoined
    -> Infrastructure EventHandler
    -> IPlayerService.HasProfileAsync
    -> PlayerService.CreateProfileAsync when needed
    -> welcome/role behavior
```

## Feature responsibilities

### Players

`Core/Features/Players/` owns the player model and persistence service.

- `PlayerModel` is the MongoDB player document.
- `PlayerService` creates, reads, and updates player records.
- Player and XP identity is based on Discord user ID for this self-hosted deployment model.

The future application-boundary migration may replace Discord.NET user objects with `ulong` IDs in lower-level service methods.

### Leveling

`Core/Features/Leveling/` owns the XP and progression rules.

The current service handles message cooldowns, XP awards, the existing quadratic threshold, carryover, and level-up notifications. Atomic XP updates and additional XP sources are planned follow-up work.

### Profiles

`Core/Features/Profiles/` owns profile commands and profile-specific orchestration.

Profile and leveling presentation overlap today. A future task may extract profile/leveling-specific presentation methods from the shared embed service without moving generic embed helpers unnecessarily.

### Discord.NET handlers

Discord.NET event wiring remains under:

```text
Core/Infrastructure/DiscordNet/Handlers/
```

`EventHandler` currently handles several event categories. A future maintenance task may split focused classes under `Handlers/Events/`, but event handlers should continue translating Discord events into feature-service calls rather than owning feature rules.

## Configuration and persistence

- `SettingsService` loads private runtime settings from `Resources/Settings/settings.json`.
- `DatabaseContext` creates MongoDB collections from the configured database settings.
- Player data is not generally partitioned by guild.
- Guild IDs are used when a record refers to a guild-owned channel, role, or message.

## Logging

The current application has an `ILoggingService` compatibility layer. The roadmap moves new code toward Microsoft `ILogger`, a custom console formatter, structured properties, rotating file output, and a separate audit service. That migration is gradual.

## Design rules

- Keep Discord.NET adapters in infrastructure.
- Keep business rules in feature services.
- Keep persistence behind service/database boundaries.
- Avoid adding new public features before the current refactor and verification work is complete.
- Never introduce DM-based workflows.
- Never commit runtime secrets.
