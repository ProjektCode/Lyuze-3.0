using Lyuze.Core.Utilities.Serialization;
using Newtonsoft.Json;

namespace Lyuze.Core.Models.API {

    public partial class Data {
        [JsonProperty("images")]
        public Image[]? Images { get; set; }
    }

    public partial class Image {
        [JsonProperty("signature")]
        public string? Signature { get; set; }

        [JsonProperty("extension")]
        public Extension Extension { get; set; }

        [JsonProperty("image_id")]
        public long ImageId { get; set; }

        [JsonProperty("favorites")]
        public long Favorites { get; set; }

        [JsonProperty("dominant_color")]
        public string? DominantColor { get; set; }

        [JsonProperty("source")]
        public Uri? Source { get; set; }

        [JsonProperty("artist")]
        public Artist? Artist { get; set; }

        [JsonProperty("uploaded_at")]
        public DateTimeOffset UploadedAt { get; set; }

        [JsonProperty("liked_at")]
        public object? LikedAt { get; set; }

        [JsonProperty("is_nsfw")]
        public bool IsNsfw { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("byte_size")]
        public long ByteSize { get; set; }

        [JsonProperty("url")]
        public Uri? Url { get; set; }

        [JsonProperty("preview_url")]
        public Uri? PreviewUrl { get; set; }

        [JsonProperty("tags")]
        public Tag[]? Tags { get; set; }
    }

    public partial class Artist {
        [JsonProperty("artist_id")]
        public long ArtistId { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("patreon")]
        public object? Patreon { get; set; }

        [JsonProperty("pixiv")]
        public Uri? Pixiv { get; set; }

        [JsonProperty("twitter")]
        public Uri? Twitter { get; set; }

        [JsonProperty("deviant_art")]
        public object? DeviantArt { get; set; }
    }

    public partial class Tag {
        [JsonProperty("tag_id")]
        public long TagId { get; set; }

        [JsonProperty("name")]
        public Name Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("is_nsfw")]
        public bool IsNsfw { get; set; }
    }

    public enum Extension { Jpeg, Jpg, Png };

    public enum Name { Maid, Oppai, Uniform, Waifu };

    public partial class Data {
        public static Data FromJson(string json) => JsonConvert.DeserializeObject<Data>(json, Converter.Settings);
    }

    public static class Serialize {
        public static string ToJson(this Data self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal class ExtensionConverter : JsonConverter {
        public override bool CanConvert(Type t) => t == typeof(Extension) || t == typeof(Extension?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value) {
                case ".jpeg":
                    return Extension.Jpeg;
                case ".jpg":
                    return Extension.Jpg;
                case ".png":
                    return Extension.Png;
            }
            throw new Exception("Cannot unmarshal type Extension");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer) {
            if (untypedValue == null) {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Extension)untypedValue;
            switch (value) {
                case Extension.Jpeg:
                    serializer.Serialize(writer, ".jpeg");
                    return;
                case Extension.Jpg:
                    serializer.Serialize(writer, ".jpg");
                    return;
                case Extension.Png:
                    serializer.Serialize(writer, ".png");
                    return;
            }
            throw new Exception("Cannot marshal type Extension");
        }

        public static readonly ExtensionConverter Singleton = new ExtensionConverter();
    }

    internal class NameConverter : JsonConverter {
        public override bool CanConvert(Type t) => t == typeof(Name) || t == typeof(Name?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value) {
                case "maid":
                    return Name.Maid;
                case "oppai":
                    return Name.Oppai;
                case "uniform":
                    return Name.Uniform;
                case "waifu":
                    return Name.Waifu;
            }
            throw new Exception("Cannot unmarshal type Name");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer) {
            if (untypedValue == null) {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Name)untypedValue;
            switch (value) {
                case Name.Maid:
                    serializer.Serialize(writer, "maid");
                    return;
                case Name.Oppai:
                    serializer.Serialize(writer, "oppai");
                    return;
                case Name.Uniform:
                    serializer.Serialize(writer, "uniform");
                    return;
                case Name.Waifu:
                    serializer.Serialize(writer, "waifu");
                    return;
            }
            throw new Exception("Cannot marshal type Name");
        }

        public static readonly NameConverter Singleton = new NameConverter();
    }
}
