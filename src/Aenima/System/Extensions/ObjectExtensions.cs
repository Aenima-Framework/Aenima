using System;

namespace Aenima.System.Extensions
{
    public static class ObjectExtensions
    {
        public static Guid? ToGuid(this object source)
        {
            if(source == null) return null;

            Guid guid;
            if(Guid.TryParse(source.ToString(), out guid))
                return guid;

            return null;
        }

        public static Guid ToGuidOrDefault(this object source, Guid defaultValue)
        {
            return source.ToGuid().GetValueOrDefault(defaultValue);
        }
    }
}