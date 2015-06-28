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
}