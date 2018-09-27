namespace Plovhest.Shared
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    internal static class JsonHelper
    {
        internal static JsonSerializerSettings JsonSerializerSettings =>
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter(),
                    new IsoDateTimeConverter()
                }
            };
    }
}
