using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Plovhest.Shared
{
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
