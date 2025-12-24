using Lyuze.Core.Utilities.Serialization;
using Newtonsoft.Json;

namespace Lyuze.Core.Models.API {
    public partial class TraceMoe {
        [JsonProperty("frameCount")]
        public long FrameCount { get; set; }
        [JsonProperty("error")]
        public string? Error { get; set; }
        [JsonProperty("result")]
        public TraceResult[]? Result { get; set; }

    }

    public partial class TraceResult {
        [JsonProperty("anilist")]
        public long AnilistId { get; set; }
        [JsonProperty("filename")]
        public string? Filename { get; set; }
        [JsonProperty("episode")]
        public long? Episode { get; set; }
        [JsonProperty("from")]
        public double From { get; set; }
        [JsonProperty("to")]
        public double To { get; set; }
        [JsonProperty("similarity")]
        public double Similarity { get; set; }
        [JsonProperty("video")]
        public string? Video { get; set; }
        [JsonProperty("image")]
        public string? Image { get; set; }
    }

    public partial class TraceMoe { 
        
        public static TraceMoe FromJson(string json) => JsonConvert.DeserializeObject<TraceMoe>(json, Converter.Settings) ?? throw new InvalidOperationException("Failed to deserialize TraceMoe JSON.");

    }

}
