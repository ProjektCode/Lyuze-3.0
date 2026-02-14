# Architecture Overview

Lyuze 3.0 is designed with a modular and extensible architecture, leveraging modern .NET Core patterns such as the Generic Host and Dependency Injection (DI). This document outlines the key components and their interactions.

## Core Principles

*   **Clean Architecture:** Separation of concerns, making the codebase easier to understand, test, and maintain.
*   **Dependency Injection:** Services are registered and resolved through a central container, promoting loose coupling.
*   **Asynchronous Programming:** Extensive use of `async`/`await` for non-blocking I/O operations, crucial for responsive Discord bot interactions.
*   **Modularity:** Features are organized into distinct modules, allowing for independent development and easier addition/removal of functionality.

## High-Level Component Map

```
+-------------------+       +-------------------+       +-------------------+
|   Discord Client  | <---> |  Discord.NET API  | <---> |   Lyuze 3.0 Bot   |
| (Users, Guilds)   |       | (Gateway, REST)   |       |                   |
+-------------------+       +-------------------+       +-------------------+
                                     ^
                                     |
                                     v
+---------------------------------------------------------------------------+
|                           .NET Generic Host                               |
|   (Manages application lifecycle, DI container, IHostedServices)          |
+---------------------------------------------------------------------------+
      ^                                                              ^
      |                                                              |
      v                                                              v
+---------------------+           +---------------------+           +---------------------+
| DiscordStartupService |           |   InteractionHandler  |           |     EventHandler      |
| (IHostedService)    | <-------> | (Registers commands,  | <-------> | (Handles events:      |
| - Bot Lifecycle     |           |   processes slash,    |           |   Ready, Message,     |
| - Init Handlers     |           |   component, modal)   |           |   UserJoin, etc.)     |
+---------------------+           +---------------------+           +---------------------+
      ^                                     ^                                     ^
      |                                     |                                     |
      v                                     v                                     v
+-------------------------------------------------------------------------------------------+
|                                    Core Services                                          |
| (Logging, Settings, Player, Embed, Image, API Clients, ReactionRoles, Leveling, etc.) |
+-------------------------------------------------------------------------------------------+
      ^                                                              ^
      |                                                              |
      v                                                              v
+---------------------+           +---------------------+           +---------------------+
|   Feature Modules   | <-------> |   Configuration     | <-------> |     Persistence     |
| (Admin, Anime,      |           | (SettingsService,   |           | (DatabaseContext,   |
|  Roles, Profiles,   |           |  SettingsConfig)    |           |  MongoDB Models)    |
+---------------------+           +---------------------+           +---------------------+
```

## Key Components

### 1. .NET Generic Host

*   **Role:** The foundation of the application. It manages the application's startup, shutdown, configuration, and Dependency Injection container.
*   **Implementation:** Configured in `Program.cs` using `Host.CreateDefaultBuilder()`.
*   **Benefits:** Provides a robust and standardized way to build console applications, especially those with long-running background tasks.

### 2. DiscordStartupService (`Core/Infrastructure/DiscordNet/DiscordStartupService.cs`)

*   **Role:** An `IHostedService` that orchestrates the Discord bot's lifecycle. It's responsible for logging in, starting the Discord client, and initializing core handlers.
*   **Flow:**
    1.  Initializes `InteractionHandler` and `EventHandler`.
    2.  Logs into Discord using the bot token from `SettingsConfig`.
    3.  Starts the `DiscordSocketClient`.
    4.  Handles graceful shutdown when the host stops.

### 3. DiscordSocketClient (from Discord.NET)

*   **Role:** The primary interface for interacting with the Discord API. It manages connections to the Discord Gateway and handles raw events.
*   **Configuration:** Configured in `Program.cs` with specific `GatewayIntents` (e.g., `GatewayIntents.All`), `WebSocketProvider`, and `LogLevel`.

### 4. InteractionService (from Discord.NET.Interactions)

*   **Role:** Manages and dispatches Discord interactions (slash commands, user commands, message commands, component interactions, modals) to the appropriate modules and methods.
*   **Registration:** Modules are discovered and registered by `InteractionHandler` using `commands.AddModulesAsync(Assembly.GetEntryAssembly(), services)`.

### 5. InteractionHandler (`Core/Infrastructure/DiscordNet/Handlers/InteractionHandler.cs`)

*   **Role:** Bridges raw `DiscordSocketClient.InteractionCreated` events to the `InteractionService`.
*   **Flow:**
    1.  Subscribes to `_client.InteractionCreated` event.
    2.  When an interaction occurs, it creates a `SocketInteractionContext` and passes it to `_commands.ExecuteCommandAsync()`.
    3.  Handles exceptions during command execution.

### 6. EventHandler (`Core/Infrastructure/DiscordNet/Handlers/EventHandler.cs`)

*   **Role:** A central hub for subscribing to and processing various Discord client events that are not directly handled by `InteractionService`.
*   **Key Events Handled:**
    *   `UserJoined`: Assigns join roles, sends welcome messages/banners.
    *   `Log`: Pipes Discord.NET logs to the bot's `ILoggingService`.
    *   `Ready`: Registers commands, sets bot status, initializes reaction roles, starts status rotation.
    *   `MessageReceived`: Handles leveling, XP gain, and basic anti-invite checks.
    *   `SlashCommandExecuted`: Grants XP for successful command executions.
    *   `InteractionService.Log`: Pipes interaction service logs to `ILoggingService`.

### 7. Feature Modules (`Core/Features/*Module.cs`)

*   **Role:** Group related commands and functionalities. Each module typically inherits from `InteractionModuleBase<SocketInteractionContext>`.
*   **Examples:** `AdminModule`, `AnimeModule`, `RolesModule`, `ProfileModule`.
*   **Structure:** Contains methods decorated with `[SlashCommand]`, `[UserCommand]`, `[MessageCommand]`, or `[ComponentInteraction]` to define specific bot actions.

### 8. Core Services (`Core/Abstractions/Interfaces`, `Core/Shared`, `Core/Infrastructure`)

*   **Role:** Provide reusable business logic and infrastructure concerns.
*   **Examples:**
    *   `ILoggingService`/`LoggingService`: Centralized logging.
    *   `ISettingsService`/`SettingsService`: Manages configuration loading and saving.
    *   `IPlayerService`/`PlayerService`: Manages user profile data.
    *   `IEmbedService`/`EmbedService`: Helper for creating rich Discord embeds.
    *   `ApiClient`: Generic HTTP client for external API calls.
    *   `ImageFetcher`, `ColorUtils`: Utilities for image manipulation.
    *   `ReactionRolesService`, `LevelingService`: Business logic for specific features.

### 9. Configuration (`Core/Infrastructure/Configuration/SettingsConfig.cs`, `SettingsService.cs`)

*   **Role:** Defines the structure and provides access to application settings.
*   **`SettingsConfig`:** A POCO (Plain Old C# Object) that maps directly to the `settings.json` file structure.
*   **`SettingsService`:** Loads `settings.json` from disk, provides access to the `SettingsConfig` object, and handles saving changes.

### 10. Persistence (`Core/Infrastructure/Database/DatabaseContext.cs`, `Core/Models/*.cs`, `Core/Features/*/Models/*.cs`)

*   **Role:** Stores and retrieves application data.
*   **Technology:** MongoDB is used as the primary data store.
*   **`DatabaseContext`:** Manages connections to MongoDB and provides access to `IMongoCollection` instances for different data models.
*   **Models:** POCOs like `PlayerModel` and `ReactionRoleModel` define the structure of data stored in MongoDB, often decorated with `MongoDB.Bson.Serialization.Attributes`.

## Request Flow (Example: Slash Command)

1.  **User executes `/command`:** Discord sends an interaction event to the bot.
2.  **`DiscordSocketClient` receives event:** The raw interaction is received by the `DiscordSocketClient`.
3.  **`InteractionHandler` processes event:** The `client.InteractionCreated` event triggers `InteractionHandler.HandleInteraction()`.
4.  **`InteractionService` dispatches:** `InteractionHandler` calls `_commands.ExecuteCommandAsync()`, which identifies the correct `InteractionModule` and method (`[SlashCommand]`) to handle the `/command`.
5.  **Module method executes:** The corresponding method in the `Feature Module` (e.g., `AdminModule.PurgeCmd()`) is invoked.
6.  **Service logic:** The module method often delegates business logic to a dedicated `Core Service` (e.g., `AdminService`).
7.  **Persistence/External APIs:** Services might interact with `DatabaseContext` (for MongoDB) or `ApiClient` (for external APIs).
8.  **Response to Discord:** The module method sends a response back to Discord (e.g., `FollowupAsync`, `RespondAsync`).
9.  **XP Gain (Post-execution):** `EventHandler.OnSlashCommandAsync()` is triggered after the command execution, granting XP to the user if successful.

This layered architecture ensures that the bot is scalable, maintainable, and easy to extend with new features.
