# SETUP

This document provides comprehensive instructions for setting up and running the Lyuze 3.0 Discord bot, including prerequisites, Discord application setup, and MongoDB Atlas configuration.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Discord Application and Bot Creation](#discord-application-and-bot-creation)
- [Runtime Settings File Location](#runtime-settings-file-location)
- [MongoDB Atlas Free Tier Setup](#mongodb-atlas-free-tier-setup)
  - [1. Create an Atlas Account](#1-create-an-atlas-account)
  - [2. Create a New Cluster (M0 Free Tier)](#2-create-a-new-cluster-m0-free-tier)
  - [3. Create a Database User](#3-create-a-database-user)
  - [4. Add IP Address to IP Access List](#4-add-ip-address-to-ip-access-list)
  - [5. Get the Connection String](#5-get-the-connection-string)
- [Linking the Bot to MongoDB](#linking-the-bot-to-mongodb)
- [Verification Steps](#verification-steps)
- [Common Errors and Troubleshooting](#common-errors-and-troubleshooting)

---

## Prerequisites

Before you begin, ensure you have the following installed:

*   **.NET SDK 8.0 or later**: Required to build and run the bot.
    *   [Download .NET SDK](https://dotnet.microsoft.com/download)
*   **Git**: For cloning the repository.
    *   [Download Git](https://git-scm.com/downloads)
*   **A Discord Account**: To create and manage your bot application.
*   **A MongoDB Atlas Account**: For the bot's database.

## Discord Application and Bot Creation

1.  **Go to the Discord Developer Portal**: Navigate to [Discord Developer Portal](https://discord.com/developers/applications).
2.  **Create a New Application**: Click on "New Application" and give it a name (e.g., "Lyuze 3.0 Dev").
3.  **Create a Bot User**:
    *   In your application, go to the "Bot" tab.
    *   Click "Add Bot".
    *   **IMPORTANT**: Enable "PRESENCE INTENT", "SERVER MEMBERS INTENT", and "MESSAGE CONTENT INTENT" under "Privileged Gateway Intents". Without these, the bot will not function correctly.
    *   Copy the **Token**. This will be used in your bot's configuration. **Keep this token secret!**
4.  **Invite the Bot to Your Server**:
    *   Go to the "OAuth2" -> "URL Generator" tab.
    *   Select `bot` under "Scopes".
    *   Under "Bot Permissions", select the necessary permissions for your bot (e.g., `Administrator` for full control during development, or specific permissions like `Send Messages`, `Manage Roles`, etc.).
    *   Copy the generated URL and paste it into your browser to invite the bot to your desired Discord server.

## Runtime Settings File Location

The bot's configuration settings are typically stored in a `settings.json` file. This file is located in the `Resources/Settings` directory relative to the executable.

Example path: `Lyuze Bot/bin/Debug/net8.0-windows/Resources/Settings/settings.json`

You will need to edit this file to provide your Discord bot token and MongoDB connection string.

## MongoDB Atlas Free Tier Setup

This section guides you through setting up a free MongoDB Atlas (M0) cluster, which is suitable for development and small-scale personal projects.

### 1. Create an Atlas Account

If you don't have one, sign up for a free account at [MongoDB Atlas](https://www.mongodb.com/cloud/atlas/register).

### 2. Create a New Cluster (M0 Free Tier)

1.  Once logged in, click "Build a Database" or "Create" to start creating a new cluster.
2.  Choose the **Shared** option (this is the M0 Free Tier).
3.  Select a **Cloud Provider** and **Region** closest to you or your bot's hosting location for optimal performance.
4.  You can leave the other settings at their defaults.
5.  Click "Create Cluster". This process may take a few minutes.

### 3. Create a Database User

1.  In your Atlas project, navigate to "Database Access" under the "Security" section.
2.  Click "Add New Database User".
3.  Choose an **Authentication Method**:
    *   **Password**: Recommended. Enter a strong username and password. **Remember these credentials**, as you will need them for your connection string.
    *   **Autogenerate Secure Password**: Atlas can generate a strong password for you. Make sure to copy it.
4.  Under "Database User Privileges", select "Read and write to any database" for simplicity during development. For production, consider more granular access.
5.  Click "Add User".

### 4. Add IP Address to IP Access List

MongoDB Atlas restricts access to your database to specific IP addresses for security.

1.  In your Atlas project, navigate to "Network Access" under the "Security" section.
2.  Click "Add IP Address".
3.  Choose one of the following:
    *   **Allow Access from Anywhere**: For development, this is the easiest option but less secure. It allows connections from any IP address (`0.0.0.0/0`).
    *   **Add Current IP Address**: If your bot will always run from the same machine with a static IP.
    *   **Add a Different IP Address**: If your bot is hosted on a server with a known static IP.
4.  Click "Confirm".

### 5. Get the Connection String

1.  Go back to the "Databases" section and click "Connect" on your cluster.
2.  Select "Connect your application".
3.  Choose "C#" as your driver and select the latest version.
4.  Copy the provided connection string. It will look something like this:
    `mongodb+srv://<username>:<password>@<cluster-name>.mongodb.net/?retryWrites=true&w=majority`
    *   **Replace `<username>`** with the database username you created in step 3.
    *   **Replace `<password>`** with the database user's password you created in step 3.
    *   **`<cluster-name>`** will be specific to your cluster.
    *   **Do NOT include angle brackets (`< >`) in your final connection string.**

## Linking the Bot to MongoDB

Open your `settings.json` file (refer to [Runtime Settings File Location](#runtime-settings-file-location)) and locate the relevant keys for your Discord token and MongoDB connection string.

The keys you need to configure are:

*   `DiscordSettings:Token`: Your Discord bot token.
*   `MongoDbSettings:ConnectionString`: Your MongoDB Atlas connection string.
*   `MongoDbSettings:DatabaseName`: The name of the database your bot will use (e.g., `LyuzeBotDb`).

Example `settings.json` snippet:

```json
{
  "DiscordSettings": {
    "Token": "YOUR_DISCORD_BOT_TOKEN_HERE"
  },
  "MongoDbSettings": {
    "ConnectionString": "mongodb+srv://YOUR_MONGO_USERNAME:YOUR_MONGO_PASSWORD@YOUR_CLUSTER_NAME.mongodb.net/?retryWrites=true&w=majority",
    "DatabaseName": "LyuzeBotDb"
  },
  // ... other settings
}
```

**IMPORTANT**: Never commit your `settings.json` file with sensitive information directly to a public repository. Use environment variables or a secure configuration management system for production deployments.

## Verification Steps

After configuring your `settings.json`:

1.  **Build the project**: Ensure there are no compilation errors.
2.  **Run the bot**: Start the bot application.
3.  **Check console output**: Look for messages indicating successful connection to Discord and MongoDB.
4.  **Interact with the bot**: Try a simple command (e.g., `/ping` if available) in your Discord server to confirm it responds.
5.  **Check MongoDB Atlas**: Verify that your bot has created collections or inserted data in your specified `DatabaseName`.

## Common Errors and Troubleshooting

*   **Bot not coming online**:
    *   Double-check your Discord bot token in `settings.json`.
    *   Ensure all "Privileged Gateway Intents" are enabled in the Discord Developer Portal.
    *   Verify the bot has been invited to the server with correct permissions.
*   **MongoDB connection issues**:
    *   Ensure your connection string is correct, with username and password replaced.
    *   Check your IP Access List in MongoDB Atlas to ensure your bot's IP is allowed.
    *   Verify the database user has "Read and write to any database" privileges (or more specific ones if configured).
    *   Check for network firewalls blocking outbound connections from your bot's host.
*   **Bot not responding to commands**:
    *   Ensure the bot is online in Discord.
    *   Check the console output for any command-related errors.
    *   Verify the bot has the necessary permissions in the Discord server.

For further configuration details, refer to `docs/CONFIGURATION.md`.
