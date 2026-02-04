using Discord;

namespace Lyuze.Core.Features.Anime;

public sealed record EmbedWithFile(Embed Embed, byte[]? FileBytes, string? FileName);
