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
}