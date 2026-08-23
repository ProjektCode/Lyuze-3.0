# Development guide

This guide explains how to build, run, debug, and extend Lyuze 3.0.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or newer.
- Git.
- An IDE such as Visual Studio or Visual Studio Code.
- A private Discord bot application and test guild.
- A local or remote MongoDB instance.

Clone the repository:

```bash
git clone https://github.com/ProjektCode/Lyuze-3.0.git
cd Lyuze-3.0
```

The solution file is:

```text
Lyuze 3.0.sln
```

The main project file is:

```text
Lyuze 3.0.csproj
```

## Build and run

Restore and build the solution:

```bash
dotnet restore "Lyuze 3.0.sln"
dotnet build "Lyuze 3.0.sln" --no-restore
```

Run the bot project:

```bash
dotnet run --project "Lyuze 3.0.csproj"
```

The application targets `net8.0-windows`. Use a Windows development environment for the supported runtime path.

Do not commit `Resources/Settings/settings.json` or copy its contents into issues and pull requests.

## Debugging

1. Configure a private `settings.json` using [Configuration](CONFIGURATION.md).
2. Start the bot from Visual Studio, Visual Studio Code, or `dotnet run`.
3. Watch the console for startup, Discord, database, and command-registration errors.
4. Place breakpoints in command modules, infrastructure handlers, and feature services.
5. Reproduce behavior in a dedicated test guild rather than a production community.
6. Remove or redact sensitive information before sharing logs.

## Architecture conventions

- Put user-facing commands in `Core/Features/<Feature>/`.
- Put feature business rules in feature services.
- Put Discord.NET event wiring in `Core/Infrastructure/DiscordNet/Handlers/`.
- Put database and external-service integration in infrastructure or feature-owned adapters.
- Put reusable cross-feature helpers in `Core/Shared/`.
- Register services in `Program.cs`.
- Keep persistence behind service/database boundaries.
- Preserve the no-DM product rule.

## Adding a command module

1. Create a feature folder under `Core/Features/`.
2. Add a class inheriting from `InteractionModuleBase<SocketInteractionContext>`.
3. Add slash, user, message, modal, or component attributes as appropriate.
4. Inject only the services the module needs.
5. Put business logic in a service when it involves persistence, external APIs, or more than simple response formatting.
6. Register custom services in `Program.cs`.
7. Build the solution and verify the command in a test guild.

Example shape:

```csharp
using Discord.Interactions;

namespace Lyuze.Core.Features.Example;

public sealed class ExampleModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("example", "Run an example command")]
    public async Task ExampleAsync()
    {
        await RespondAsync("Example response", ephemeral: true);
    }
}
```

The interaction handler discovers modules from the entry assembly. A module normally does not need a separate registration, but any injected custom service does.

## Adding a service

1. Place the service beside the feature that owns its behavior.
2. Add an interface under `Core/Abstractions/Interfaces/` when the service needs an abstraction for multiple implementations or testing.
3. Inject dependencies through the constructor.
4. Register the service in `Program.cs` with the appropriate lifetime.
5. Add persistence and error-handling behavior in the service rather than in the command module.
6. Add or update verification coverage.

For new logging, use Microsoft's `ILogger<T>` pipeline. `ILoggingService` is a compatibility layer during the migration and should not receive new call sites unless there is a specific reason.

## Discord event handling

Keep Discord.NET subscriptions in infrastructure handlers.

Current examples include:

- `UserJoined` for profile creation and welcome behavior.
- `MessageReceived` for message eligibility, moderation checks, and message XP.
- `Ready` for command registration, status rotation, and startup initialization.
- `SlashCommandExecuted` for post-command XP behavior.

Handlers should translate events into feature-service calls. Do not move all feature rules into the central event handler. A future refactor may split focused classes under `Core/Infrastructure/DiscordNet/Handlers/Events/`.

## Verification baseline

For source changes, run:

```bash
dotnet build "Lyuze 3.0.sln" --no-restore
git diff --check
```

When a change affects runtime behavior, also verify the affected Discord flow in a test guild and check MongoDB compatibility when persistence is involved.

For documentation-only changes, check:

- No secrets or real settings are present.
- Paths and command names match the repository.
- Internal task tracking is not being presented as public user documentation.
- Links resolve from the document's location.

## Wiki publishing

The public documentation allowlist is stored in `.github/public-docs.txt`. The future wiki publisher reads that file and syncs only the approved documents to the GitHub Wiki. Keep `docs/TASKS.md`, vault notes, runtime settings, and other internal material out of the allowlist.

Treat the repository as the documentation source of truth. Direct wiki edits may be overwritten by the next successful publisher run.

The publisher runs on `master` documentation changes and through manual dispatch. Its write permission is limited to the publishing job. If the repository token cannot write to the wiki repository, configure a narrowly scoped `WIKI_PUSH_TOKEN` GitHub secret instead of placing a token in source control.

## Pull requests and branches

Use short-lived branches with descriptive names:

```text
refactor/players-leveling-split
fix/atomic-xp-awards
feat/github-integration
docs/public-wiki
```

Keep a pull request focused on one feature, refactor, or documentation milestone. Include:

- What changed.
- Why it changed.
- How it was verified.
- Known runtime limitations.
- Any persistence or configuration migration notes.

Do not commit, push, or merge changes without the repository owner's approval.
