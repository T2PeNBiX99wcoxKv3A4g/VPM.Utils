using System.Collections.Generic;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Extensions
{
    /// <summary>
    ///     Dictionary helpers that treat a missing key as a default value instead of throwing.
    /// </summary>
    [PublicAPI]
    public static class DictionaryExtensions
    {
        /// <summary>
        ///     Value stored under the given key, without throwing when the key is missing.
        /// </summary>
        /// <param name="dict">The dictionary to read; it is not modified.</param>
        /// <param name="key">The key to look up. A null key is not guarded and throws for Dictionary&lt;TKey, TValue&gt;.</param>
        /// <returns>
        ///     The stored value, or default(TValue) when the key is missing: null for reference types, zero or false for
        ///     value types.
        /// </returns>
        [CanBeNull]
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, [NotNull] TKey key) =>
            dict.TryGetValue(key, out var value) ? value : default;
    }
}