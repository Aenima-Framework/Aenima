using System;

namespace Aenima
{
    public static class SerializerExtensions
    {
        public static T DeserializeAs<T>(this ISerializer serializer, string text, Type declaringType)
        {
            return (T)serializer.Deserialize(text, declaringType);
        }

        public static T Deserialize<T>(this ISerializer serializer, string text)
        {
            return (T)serializer.Deserialize(text, typeof(T));
        }
    }
}