using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Aenima.JsonNet
{
    public class GuidReader : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanConvert(Type objectType)
        {
            return objectType.GetTypeInfo().IsAssignableFrom(typeof(Guid));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Guid convertedValue;
            return Guid.TryParse(reader.Value?.ToString(), out convertedValue)
                ? convertedValue
                : serializer.Deserialize(reader, reader.ValueType); // passthrough
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}