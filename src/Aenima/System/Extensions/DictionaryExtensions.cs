using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Aenima.System.Extensions
{
    public static class DictionaryExtensions
    {
        public static TDictionary Merge<TDictionary, TKey, TValue>(this TDictionary master, params IDictionary<TKey, TValue>[] slaves)
            where TDictionary : IDictionary<TKey, TValue>
        {
            Guard.NullOrDefault(() => master);

            slaves
                .Where(slave => slave != null)
                .WithEach(slave => slave.WithEach(p => master[p.Key] = p.Value));

            return master;
        }
    }


    public static class ConcurrentDictionaryExtensions
    {
        public static TValue LazyGetOrAdd<TKey, TValue>(
            this ConcurrentDictionary<TKey, Lazy<TValue>> dictionary,
            TKey key,
            Func<TKey, TValue> valueFactory)
        {
            Guard.NullOrDefault(() => dictionary);
            var result = dictionary.GetOrAdd(key, new Lazy<TValue>(() => valueFactory(key)));
            return result.Value;
        }
    }
}