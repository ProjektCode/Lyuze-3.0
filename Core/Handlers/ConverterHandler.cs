using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lyuze.Core.Handlers {

    internal static class Converter {
        public readonly static JsonSerializerSettings Settings = new() {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            Converters = {
                new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            }
        };
    }

}
