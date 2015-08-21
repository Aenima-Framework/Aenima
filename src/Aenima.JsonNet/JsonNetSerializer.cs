using System;
using Newtonsoft.Json;

namespace Aenima.JsonNet
{
    public class JsonNetSerializer : ISerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
            TypeNameHandling  = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver  = BetterContractResolver.Instance
        };

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        public object Deserialize(string text, Type declaringType)
        {
            return JsonConvert.DeserializeObject(text, declaringType, Settings);
        }
    }
}