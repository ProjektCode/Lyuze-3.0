# Development Guide

This guide provides instructions for setting up your development environment, building, running, debugging, and extending Lyuze 3.0.

## Table of Contents

*   [Prerequisites](#prerequisites)
*   [Setting Up Your Environment](#setting-up-your-environment)
*   [Building and Running](#building-and-running)
*   [Debugging](#debugging)
*   [Adding a New Command Module](#adding-a-new-command-module)
*   [Adding a New Service](#adding-a-new-service)
*   [Coding Conventions (Inferred)](#coding-conventions-inferred)

---

## Prerequisites

*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or newer.
*   An IDE like [Visual Studio](https://visualstudio.microsoft.com/) (recommended) or [Visual Studio Code](https://code.visualstudio.com/).
*   Git for version control.
*   A local or remote MongoDB instance for persistence.

## Setting Up Your Environment

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/your-repo/Lyuze-Bot.git
    cd "Lyuze 3.0/Lyuze Bot"
    ```
2.  **Open in IDE:**
    *   **Visual Studio:** Open the `Lyuze 3.0.sln` solution file.
    *   **Visual Studio Code:** Open the `Lyuze Bot` folder.
3.  **Configure `settings.json`:**
    *   The bot expects a `settings.json` file in `Resources/Settings/` relative to the executable. If you run the bot once, it will generate a template.
    *   Populate this file with your Discord bot token, `GuildID`, MongoDB connection string, and any API keys required for features you want to test.
    *   Refer to [`docs/CONFIGURATION.md`](./docs/CONFIGURATION.md) for details on each setting.

## Building and Running

### Build

To build the project from the command line:

```bash
dotnet build "Lyuze Bot.csproj"
```

Or, if you are in the solution directory:

```bash
dotnet build
```

### Run

To run the bot from the command line:

```bash
dotnet run --project "Lyuze Bot.csproj"
```

If running from Visual Studio, simply press `F5` or the "Start" button.

## Debugging

1.  **Set `settings.json`:** Ensure your `settings.json` is correctly configured, especially the Discord token and `GuildID`.
2.  **Start Debugging:**
    *   **Visual Studio:** Set `Lyuze Bot` as the startup project and press `F5`.
    *   **Visual Studio Code:** Configure a `launch.json` for a .NET Core application and start debugging.
3.  **Breakpoints:** Place breakpoints in your code (e.g., in command methods, event handlers) to inspect variables and execution flow.
4.  **Logging:** Monitor the console output for logs from the `ILoggingService`. The `EventHandler` pipes Discord.NET's internal logs and `InteractionService` logs to this service.

## Adding a New Command Module

To add a new set of slash commands or other interactions:

1.  **Create a new Module class:**
    *   Create a new folder under `Core/Features/YourFeatureName` (e.g., `Core/Features/MyNewFeature`).
    *   Create a new class (e.g., `MyNewModule.cs`) that inherits from `Discord.Interactions.InteractionModuleBase<SocketInteractionContext>`.
    *   Example structure:
        ```csharp
        using Discord.Interactions;
        using Discord.WebSocket;

        namespace Lyuze.Core.Features.MyNewFeature
        {
            public class MyNewModule : InteractionModuleBase<SocketInteractionContext>
            {
                // Inject any services your module needs via constructor
                public MyNewModule(/* MyService myService */)
                {
                    // _myService = myService;
                }

                [SlashCommand("mycommand", "A description for my new command")]
                // [RequireUserPermission(GuildPermission.Administrator)] // Optional: Add permissions
                public async Task MyCommandAsync(
                    [Summary("param1", "Description for param1")] string param1,
                    [Summary("param2", "Description for param2")] int param2)
                {
                    await DeferAsync(ephemeral: true); // Acknowledge the interaction
                    // Your command logic here
                    await FollowupAsync($"You used mycommand with {param1} and {param2}!", ephemeral: true);
                }

                // Add more slash commands, user commands, message commands, or component interactions
            }
        }
        ```
2.  **Register Dependencies (if any):**
    *   If your new module requires custom services, ensure those services are registered in `Program.cs` within the `ConfigureServices` method.
3.  **No explicit module registration needed:** The `InteractionHandler` automatically discovers and registers all `InteractionModuleBase` classes in the entry assembly.
4.  **Run and Test:** Start the bot. The new commands should appear in Discord after the bot registers them (usually on `Ready` event).

## Adding a New Service

To add a new service that encapsulates business logic:

1.  **Define an Interface (Optional but Recommended):**
    *   Create an interface in `Core/Abstractions/Interfaces` (e.g., `IMyNewService.cs`). This promotes loose coupling and testability.
    *   Example:
        ```csharp
        namespace Lyuze.Core.Abstractions.Interfaces
        {
            public interface IMyNewService
            {
                Task<string> DoSomethingAsync(string input);
            }
        }
        ```
2.  **Implement the Service:**
    *   Create the service class in an appropriate `Core/Features/YourFeatureName` or `Core/Shared` folder (e.g., `Core/Features/MyNewFeature/MyNewService.cs`).
    *   Implement the interface.
    *   Example:
        ```csharp
        using Lyuze.Core.Abstractions.Interfaces;

        namespace Lyuze.Core.Features.MyNewFeature
        {
            public class MyNewService : IMyNewService
            {
                private readonly ILoggingService _logger;

                public MyNewService(ILoggingService logger) // Inject dependencies
                {
                    _logger = logger;
                }

                public async Task<string> DoSomethingAsync(string input)
                {
                    _logger.LogInformationAsync("MyNewService", $"Doing something with: {input}");
                    await Task.Delay(100); // Simulate async work
                    return $"Processed: {input.ToUpper()}";
                }
            }
        }
        ```
3.  **Register the Service in DI:**
    *   Open `Program.cs`.
    *   In the `ConfigureServices` method, add a registration for your service. Use `AddSingleton`, `AddScoped`, or `AddTransient` based on its lifecycle requirements. Most bot services are `AddSingleton`.
    *   Example:
        ```csharp
        services.AddSingleton<IMyNewService, MyNewService>();
        // If no interface:
        // services.AddSingleton<MyNewService>();
        ```
4.  **Inject and Use:**
    *   You can now inject `IMyNewService` (or `MyNewService` if no interface) into any other service or command module that needs it via their constructors.

## Coding Conventions (Inferred)

Based on the existing codebase, the following conventions are observed:

*   **C# Language Features:** Uses modern C# features (e.g., `using` declarations, `required` keyword, `file-scoped namespaces`, `primary constructors` for services/modules).
*   **Asynchronous Programming:** Heavily relies on `async`/`await` for I/O operations.
*   **Dependency Injection:** Services are injected via constructor.
*   **Logging:** Uses `ILoggingService` for structured logging.
*   **Configuration:** Centralized in `settings.json` and accessed via `SettingsConfig`.
*   **Error Handling:** `try-catch` blocks are used for critical operations, often logging errors with `ILoggingService`.
*   **Discord.NET Usage:**
    *   Uses `InteractionModuleBase<SocketInteractionContext>` for command modules.
    *   `DeferAsync()` is commonly used to acknowledge interactions before processing.
    *   `FollowupAsync()` and `RespondAsync()` for sending responses.
    *   `Context.DelayDeleteOriginalAsync()` for ephemeral messages.
*   **Naming:**
    *   Interfaces start with `I` (e.g., `ILoggingService`).
    *   Private readonly fields start with `_` (e.g., `_client`).
    *   Methods use PascalCase.
    *   Local variables use camelCase.
*   **File Structure:** Features are typically organized into `Core/Features/FeatureName` folders, containing modules, services, and models related to that feature. Infrastructure concerns are in `Core/Infrastructure`.
