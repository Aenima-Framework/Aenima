using System;
using System.Globalization;

namespace Aenima.System.Extensions
{
    public static class StringExtensions
    {
        public static string FormatWith(this string source, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, source, args);
        }

        public static string FormatWith(this string source, CultureInfo ci, params object[] args)
        {
            return string.Format(ci, source, args);
        }

        public static bool IsNotNullOrWhiteSpace(this string source)
        {
            return !string.IsNullOrWhiteSpace(source);
        }

        public static bool IsNullOrWhiteSpace(this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }
    }

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