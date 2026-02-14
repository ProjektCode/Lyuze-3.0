# Lyuze 3.0 Discord Bot

Lyuze 3.0 is a modern, modular, and extensible Discord bot built with C# and Discord.NET. It leverages .NET's Generic Host for robust application lifecycle management and Dependency Injection, making it easy to develop and maintain. The bot features a range of functionalities, including administrative tools, anime-related commands, user profiles with leveling, and reaction roles.

## Features

*   **Administration & Moderation:**
    *   Purge messages, kick, ban, and unban users.
    *   Owner-only commands for bot control (`/kill`).
*   **Anime & Image Search:**
    *   Get random anime quotes (`/aquote`).
    *   Trace anime source from images (`/trace`).
    *   Find image sources using SauceNao (`/sauce`).
    *   Fetch random waifu images by tag (`/waifu`).
*   **User Profiles & Leveling:**
    *   View and customize user profiles (`/profile`).
    *   Set custom profile backgrounds (`/setprofilebg`).
    *   Update "About Me" sections (`/setprofileaboutme`).
    *   Control profile visibility (`/setprofilestatus`).
    *   Toggle level-up notifications (`/setplvlnotifications`).
    *   XP gain from messages and slash commands.
*   **Reaction Roles:**
    *   Set up and manage reaction role messages (`/setup_reaction_roles`, `/add_reaction_roles`).
    *   Remove roles from users (`/remove_role`).
*   **Robust Architecture:**
    *   Clean architecture with clear separation of concerns.
    *   Extensive use of Dependency Injection.
    *   MongoDB for data persistence.
    *   Asynchronous event handling.

## Quick Start

To get Lyuze 3.0 up and running quickly, follow the detailed setup guide in [`docs/SETUP.md`](./docs/SETUP.md). This guide covers everything from prerequisites to Discord application setup and MongoDB Atlas configuration.

### Prerequisites

*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or newer.
*   A Discord Bot Token.
*   A MongoDB instance (e.g., a free tier cluster on MongoDB Atlas).

For detailed instructions on obtaining these and configuring them, refer to [`docs/SETUP.md`](./docs/SETUP.md).

### Build and Run

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/your-repo/Lyuze-Bot.git
    cd "Lyuze 3.0/Lyuze Bot"
    ```
2.  **Configure Settings:**
    *   Navigate to the `Resources/Settings/` directory (it will be created on first run if it doesn't exist).
    *   Create or edit `settings.json` with your bot's configuration. A template will be generated if the file is missing.
    *   Refer to [`docs/CONFIGURATION.md`](./docs/CONFIGURATION.md) for detailed configuration instructions.
3.  **Build the project:**
    ```bash
    dotnet build
    ```
4.  **Run the bot:**
    ```bash
    dotnet run --project "Lyuze Bot.csproj"
    ```
    The bot should connect to Discord and log its startup process.

## Configuration Overview

Lyuze 3.0 uses a `settings.json` file for all its configuration. This file is located in the `Resources/Settings/` directory relative to the application's base directory. It contains critical information such as your Discord bot token, API keys for external services, and database connection strings.

**IMPORTANT:** Never share your `settings.json` file, especially if it contains sensitive information like API keys or bot tokens.

For a detailed guide on configuring the bot, including all available keys and how to set them up, please see [`docs/CONFIGURATION.md`](./docs/CONFIGURATION.md).

## Commands Overview

Lyuze 3.0 supports a variety of slash commands, user commands, and message commands. These are organized into modules based on their functionality.

For a complete list of commands, their descriptions, and required permissions, refer to [`docs/COMMANDS.md`](./docs/COMMANDS.md).

## Troubleshooting

*   **Bot not coming online:**
    *   Check your `settings.json` for correct Discord bot token and `GuildId`.
    *   Ensure your bot has the necessary [Gateway Intents](https://discord.com/developers/docs/topics/gateway#gateway-intents) enabled in the Discord Developer Portal (especially `Message Content Intent` if you use message-based features).
    *   Review the console output for any error messages during startup.
*   **Commands not appearing/working:**
    *   Ensure your bot has the `applications.commands` scope authorized in its OAuth2 URL.
    *   Verify the `GuildId` in `settings.json` is correct for the guild where you expect commands to be registered.
    *   Check the bot's permissions in the Discord server.


## Contributing and Development Notes

If you plan to contribute to Lyuze 3.0 or develop new features, please refer to [`docs/DEVELOPMENT.md`](./docs/DEVELOPMENT.md) for guidelines on setting up your development environment, coding conventions, and adding new commands or services. This document is intended for contributors and maintainers, providing practical guidance for working with the codebase.

## Existing Documentation

*   [`docs/MODERATION.md`](./docs/MODERATION.md)
*   [`docs/ANIME-IMAGE-SEARCH.md`](./docs/ANIME-IMAGE-SEARCH.md)
