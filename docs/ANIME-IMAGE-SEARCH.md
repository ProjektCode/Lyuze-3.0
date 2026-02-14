# Anime Image Search

[&#8592; Back to README](../README.md) | [&#8592; Back to Commands List](./COMMANDS.md)

Lyuze 3.0 offers advanced anime image tracing capabilities, integrating with multiple services to provide accurate source identification. The bot intelligently switches between SauceNAO and Danbooru IQDB based on confidence levels and service availability.

## How it Works

When you request an image search, the bot first attempts to use **SauceNAO**. If SauceNAO does not return a confident match (below a 70% similarity threshold), returns no results, or hits a rate limit, the bot automatically falls back to **Danbooru IQDB**.

### SauceNAO
- **Primary Service**: Used first for image tracing.
- **API Key**: Requires a `Sauce Nao` API key configured in `settings.json`. Without it, the bot will directly use Danbooru IQDB.
- **Threshold**: A minimum similarity of 70% is required for a SauceNAO result to be considered a confident match. If the top match falls below this, Danbooru IQDB is used as a fallback.

### Danbooru IQDB
- **Fallback Service**: Used when SauceNAO is unavailable or does not provide a confident match.
- **Scores**: Danbooru IQDB uses a different matching algorithm, and its similarity scores might appear lower than SauceNAO's, even for good matches. This is normal behavior.
- **Authentication (Optional)**: For higher rate limits and more reliable service, you can provide `Danbooru Login` and `Danbooru Api Key` in `settings.json`.

## Configuration

API keys and login details for the image search services are managed in the bot's runtime settings file.

- **Runtime Settings Location**: `Resources/Settings/settings.json` (relative to the bot's executable directory). For more details, see [`docs/CONFIGURATION.md`](./CONFIGURATION.md).

The relevant properties within the `APIs` section of `settings.json` are:

- `Sauce Nao`: Your SauceNAO API key.
- `Danbooru Login`: Your Danbooru username (optional, for higher rate limits).
- `Danbooru Api Key`: Your Danbooru API key (optional, for higher rate limits).

**Example `APIs` section in `settings.json` (do NOT include real keys):**
```json
{
  "APIs": {
    "Sauce Nao": "YOUR_SAUCENAO_API_KEY",
    "Danbooru Login": "YOUR_DANBOORU_USERNAME",
    "Danbooru Api Key": "YOUR_DANBOORU_API_KEY"
  }
}
```

## Troubleshooting

### 403 Forbidden Errors
If you encounter 403 Forbidden errors when the bot attempts to access image search APIs, it might be due to missing or incorrect `User-Agent` headers in the HTTP requests. The bot's internal API client is configured to handle this, but if you are debugging network issues, ensure that requests include a valid `User-Agent`.

### Discord Ephemeral Attachment Caveat
When using the image search command with an image attached to an *ephemeral* Discord message, direct links to that attachment are typically not accessible by external services. The bot handles this by downloading the image internally before sending it to SauceNAO or Danbooru IQDB. However, if you are manually providing links, ensure they are publicly accessible.
