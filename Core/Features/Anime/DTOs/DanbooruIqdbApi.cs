using System;
using Lyuze.Core.Utilities.Serialization;
using Newtonsoft.Json;

namespace Lyuze.Core.Models.API;

public sealed class DanbooruIqdbPost {
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("source")]
    public string? Source { get; set; }

    [JsonProperty("rating")]
    public string? Rating { get; set; }

    [JsonProperty("tag_string_artist")]
    public string? TagStringArtist { get; set; }

    [JsonProperty("tag_string_copyright")]
    public string? TagStringCopyright { get; set; }

    [JsonProperty("tag_string_character")]
    public string? TagStringCharacter { get; set; }

    [JsonProperty("preview_file_url")]
    public string? PreviewFileUrl { get; set; }

    [JsonProperty("large_file_url")]
    public string? LargeFileUrl { get; set; }

    [JsonProperty("file_url")]
    public string? FileUrl { get; set; }
}

public sealed class DanbooruIqdbMatch {
    [JsonProperty("post_id")]
    public long PostId { get; set; }

    [JsonProperty("score")]
    public double Score { get; set; }

    [JsonProperty("post")]
    public DanbooruIqdbPost? Post { get; set; }

    public static DanbooruIqdbMatch[] FromJson(string json) =>
        JsonConvert.DeserializeObject<DanbooruIqdbMatch[]>(json, Converter.Settings)
        ?? throw new InvalidOperationException("Failed to deserialize Danbooru IQDB JSON.");
}
