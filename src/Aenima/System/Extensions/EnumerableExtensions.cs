using System;
using System.Collections.Generic;
using System.Linq;

namespace Aenima.System.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Func<T,T> func)
        {
            return source.Select(func);
        }

        public static void WithEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach(var item in source) action(item);
        }
    }
}