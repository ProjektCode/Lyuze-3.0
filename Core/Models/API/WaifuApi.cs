using Lyuze.Core.Utilities.Serialization;
using Newtonsoft.Json;

namespace Lyuze.Core.Models.API {

    public partial class Waifu {
        [JsonProperty("images")]
        public Image[]? Images { get; set; }
    }

    public partial class Image {

        [JsonProperty("dominant_color")]
        public string? DominantColor { get; set; }

        [JsonProperty("url")]
        public Uri? Url { get; set; }

        [JsonProperty("preview_url")]
        public Uri? PreviewUrl { get; set; }
    }

    public partial class Waifu {
        public static Waifu FromJson(string json) => JsonConvert.DeserializeObject<Waifu>(json, Converter.Settings) ?? throw new InvalidOperationException("Failed to deserialize Waifu JSON.");

    }

    public static class Serialize {
        public static string ToJson(this Waifu self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

}
