using System;
using Lyuze.Core.Utilities.Serialization;
using Newtonsoft.Json;

namespace Lyuze.Core.Models.API {

    public partial class SauceNao {
        [JsonProperty("header")]
        public required SauceHeader Header { get; set; }

        [JsonProperty("results")]
        public required SauceResult[] Results { get; set; }
    }

    public partial class SauceHeader {
        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("account_type")]
        public long AccountType { get; set; }

        [JsonProperty("short_limit")]
        public string? ShortLimit { get; set; }

        [JsonProperty("long_limit")]
        public string? LongLimit { get; set; }

        [JsonProperty("long_remaining")]
        public int LongRemaining { get; set; }

        [JsonProperty("short_remaining")]
        public int ShortRemaining { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }
    }

    public partial class Index {
    }

    public partial class SauceResult {
        [JsonProperty("header")]
        public required ResultHeader Header { get; set; }

        [JsonProperty("data")]
        public required SauceData Data { get; set; }
    }

    public partial class SauceData {
        [JsonProperty("ext_urls")]
        public Uri[]? ExtUrls { get; set; }

        [JsonProperty("danbooru_id", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? DanbooruId { get; set; }

        [JsonProperty("gelbooru_id", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? GelbooruId { get; set; }

        [JsonProperty("creator", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string? Creator { get; set; }

        [JsonProperty("source", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string? Source { get; set; }

        [JsonProperty("author_url", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Uri? AuthorUrl { get; set; }

        [JsonProperty("artist", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string? Artist { get; set; }

        [JsonProperty("author", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string? Author { get; set; }
    }

    public partial class ResultHeader {
        [JsonProperty("similarity")]
        public string? Similarity { get; set; }

        [JsonProperty("thumbnail")]
        public Uri? Thumbnail { get; set; }

    }

    public partial class SauceNao {
        public static SauceNao FromJson(string json) => JsonConvert.DeserializeObject<SauceNao>(json, Converter.Settings) ?? throw new InvalidOperationException("Failed to deserialize  JSON.");
    }
}
