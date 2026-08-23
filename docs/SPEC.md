# Lyuze 3.0 Specification

## Status

Lyuze 3.0 is a personal-first, self-hosted Discord bot. This specification describes the current product boundary and technical baseline; planned work remains in `docs/ROADMAP.md` and `docs/TASKS.md`.

## Goals

- Provide moderation and administration commands.
- Provide user profiles and leveling.
- Provide reaction roles.
- Provide anime, image lookup, and utility commands.
- Persist installation data in MongoDB.
- Remain practical for self-hosted installations.

## Non-goals

- Public SaaS hosting.
- Multi-tenant hosting assumptions.
- DM-based user workflows.
- Committing real settings, credentials, tokens, or connection strings.

## Hard product rules

- The bot must never DM users.
- Player profiles and XP are keyed by Discord user ID.
- Guild IDs are used for guild-specific Discord resources, not as a default partition key for every record.
- Bootstrap secrets and startup configuration remain private and are not committed.
- Mutable runtime configuration may move to MongoDB only with an explicit schema and migration plan.

## Runtime and dependencies

- .NET 8 SDK or newer.
- Target framework: `net8.0-windows`.
- Discord.NET 3.18.0.
- .NET Generic Host and Microsoft dependency injection.
- MongoDB.Driver 3.5.2.
- Private runtime settings at `Resources/Settings/settings.json`.
- A Discord application/bot and MongoDB instance for runtime use.

Never share or commit the real `settings.json`.

## Architecture

- `Program.cs` configures the Generic Host, dependency injection, Discord client, interaction service, HTTP clients, logging, and hosted startup service.
- `Core/Features/` contains feature modules and services.
- `Core/Infrastructure/` contains configuration, database, HTTP, logging, and Discord.NET integration.
- `Core/Shared/` contains reusable embeds, images, status, and other cross-feature components.
- MongoDB persistence is accessed through the existing database/context and service boundaries.

## Current feature areas

The repository currently contains administration/moderation, profiles, leveling, reaction roles, anime/image commands, utility/help behavior, Discord event handling, and MongoDB persistence.

The Players/Leveling split is currently in progress. It must not be treated as complete until the repository builds and the affected profile, XP, embed, event, and dependency-injection flows are verified.

## Acceptance criteria for meaningful changes

A meaningful change is acceptable when:

- the intended behavior and non-goals are explicit;
- applicable project rules and current worktree state were respected;
- the smallest relevant automated checks pass;
- affected runtime behavior is manually checked when practical;
- persistence changes include compatibility or migration notes;
- no DM workflow or secret exposure was introduced;
- documentation and task status match the verified implementation.
