using Lyuze.Core.Utilities.Serialization;
using Newtonsoft.Json;

namespace Lyuze.Core.Models.API {

    public partial class AnimeRandomQuote {
        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("data")]
        public Data? Data { get; set; }
    }

    public partial class Data {
        [JsonProperty("content")]
        public string? Content { get; set; }

        [JsonProperty("anime")]
        public Anime? Anime { get; set; }

        [JsonProperty("character")]
        public Character? Character { get; set; }
    }

    public partial class Anime {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("altName")]
        public string? AltName { get; set; }
    }

    public partial class Character {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }

    public partial class AnimeRandomQuote {
        public static AnimeRandomQuote FromJson(string json) => JsonConvert.DeserializeObject<AnimeRandomQuote>(json, Converter.Settings) ?? throw new InvalidOperationException("Failed to deserialize AnimeRandomQuote JSON.");
    }

}
