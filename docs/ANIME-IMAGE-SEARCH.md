# Anime and image search

Lyuze provides anime quotes, anime-scene tracing, image-source lookup, and tagged image retrieval through external providers.

## Commands

The current commands are grouped under `/anime`:

- `/anime aquote`
- `/anime trace`
- `/anime sauce`
- `/anime waifu`

Check [Commands](COMMANDS.md) for the current parameters and permissions.

## Image tracing flow

### `/anime trace`

The bot accepts an image upload or a direct image URL and attempts to identify the anime scene.

### `/anime sauce`

The bot accepts an image upload or direct image URL and attempts to find an image source. The repository uses SauceNAO with a Danbooru IQDB fallback path.

When an image is attached to an ephemeral Discord response, the attachment URL may not be usable by an external provider. The bot downloads the image internally before sending it to the configured provider.

## Provider configuration

Some providers require API credentials. Configure them only in the private `APIs` section of `Resources/Settings/settings.json`:

- `Sauce Nao`: SauceNAO API key.
- `Danbooru Login`: Optional Danbooru username.
- `Danbooru Api Key`: Optional Danbooru API key.

Do not publish real keys, login details, or private image URLs. Use placeholders in examples.

## Fallback behavior

The image-source flow should:

1. Validate the upload or URL.
2. Download the image through the bot's HTTP client when needed.
3. Try the primary provider.
4. Fall back when the primary provider has no usable match, is unavailable, or is rate-limited.
5. Return a useful error without exposing provider credentials.

Similarity scores from different providers are not directly comparable. A lower score from a fallback provider does not necessarily mean the result is unusable.

## Troubleshooting

### Provider returns 403

Check that the bot's HTTP client sends a valid User-Agent and that the provider credentials, if required, are configured correctly.

### URL cannot be accessed

Confirm the URL is a direct, publicly reachable image URL. Discord ephemeral attachment URLs may require the bot to download the file before sending it to an external provider.

### Provider rate limit

Wait for the provider's limit window and use the fallback path when available. Do not add aggressive retry loops that can increase rate limiting.
