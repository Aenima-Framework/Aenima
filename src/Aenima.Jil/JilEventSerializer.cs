using System;
using Jil;

namespace Aenima.Jil
{
    public class JilEventSerializer : IEventSerializer
    {
        private static readonly Options SerializationOptions = new Options(
            excludeNulls: true,
            unspecifiedDateTimeKindBehavior: UnspecifiedDateTimeKindBehavior.IsUTC, 
            includeInherited: true);

        public string Serialize(object obj)
        {
            return JSON.SerializeDynamic(obj, SerializationOptions);
        }

        public object Deserialize(string serialized, Type type)
        {
            return JSON.Deserialize(serialized, type, SerializationOptions);
        }

        public string Serialize<T>(T obj)
        {
            throw new NotImplementedException();
        }

        public T Deserialize<T>(string text)
        {
            throw new NotImplementedException();
        }
    }
}