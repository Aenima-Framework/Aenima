using System.Collections.Generic;

namespace Aenima.System.Extensions
{
    public static class GenericExtensions
    {
        /// <summary>
        ///     A T extension method that query if 'source' equals it's default value.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="source">The object to act on.</param>
        /// <returns>true if default, false if not.</returns>
        /// <remarks>
        ///     To avoid boxing, the best way to compare generics for equality is with EqualityComparer&lt;T&gt;.Default.<br/>
        ///     This respects IEquatable&lt;T&gt; (without boxing) as well as object.Equals, and handles all the Nullable&lt;T&gt; "lifted" nuances.<br/>
        ///     This will match:<br/>
        ///         - null for classes<br/>
        ///         - null (empty) for Nullable&lt;T&gt;<br/>
        ///         - zero/false/etc for other structs<br/>
        ///     By Marc Gravell.<br/>
        ///     Source: http://stackoverflow.com/a/864860/503085
        /// </remarks>
        public static bool IsDefault<T>(this T source)
        {
            return EqualityComparer<T>.Default.Equals(source, default(T));
        }

        /// <summary>
        ///     A T extension method that query if 'source' does not equals it's default value.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="source">The object to act on.</param>
        /// <returns>true if not default, false if not.</returns>
        public static bool IsNotDefault<T>(this T source)
        {
            return !IsDefault(source);
        }
    }
}