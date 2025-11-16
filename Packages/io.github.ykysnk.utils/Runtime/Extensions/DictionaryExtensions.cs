using System.Collections.Generic;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class DictionaryExtensions
    {
        public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) =>
            dict.TryGetValue(key, out var value) ? value : default;
    }
}