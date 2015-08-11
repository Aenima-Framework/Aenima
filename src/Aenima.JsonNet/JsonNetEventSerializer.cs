using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using static System.String;

namespace Aenima.JsonNet
{
    public class JsonNetEventSerializer : IEventSerializer
    {
        public static readonly GuidReader GuidReader = new GuidReader();
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
            TypeNameHandling  = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver  = EventContractResolver.Instance,
        };

        public JsonNetEventSerializer()
        {
            Settings.Converters.Add(GuidReader);
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        public object Deserialize(string text, Type declaringType)
        {
            return JsonConvert.DeserializeObject(
                value   : text,
                type    : declaringType,
                settings: Settings);
        }
    }
}
