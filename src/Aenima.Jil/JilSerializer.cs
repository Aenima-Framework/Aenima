using System;
using Jil;

namespace Aenima.Jil
{
    public class JilSerializer : ISerializer
    {
        public string Serialize(object obj)
        {
            return JSON.Serialize(obj, new Options());
        }

        public object Deserialize(string text, Type declaringType)
        {
            return JSON.Deserialize(text, declaringType);
        }
    }
}