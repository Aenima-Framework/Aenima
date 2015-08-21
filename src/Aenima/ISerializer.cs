using System;

namespace Aenima
{
    public interface ISerializer
    {
        string Serialize(object obj);
        object Deserialize(string text, Type type);
    }
}