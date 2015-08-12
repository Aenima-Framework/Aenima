using System;

namespace Aenima.System.Extensions
{
    public static class ObjectExtensions
    {
        public static Guid ToGuid(this object source)
        {
            return Guid.Parse(source.ToString());
        }
    }
}