using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

/// <summary>
///     Dictionary-shaped data stored as two parallel lists, because Unity can serialize lists but not a
///     <see cref="Dictionary{TKey,TValue}" />. Lookups scan <see cref="keys" /> linearly, so it is meant for small
///     collections that have to survive serialization rather than for hot paths.
/// </summary>
[Serializable]
[PublicAPI]
public sealed class DictionaryWrapper<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
{
    /// <summary>Keys in stored order. The value for each key sits at the same index in <see cref="values" />.</summary>
    public List<TKey> keys;
    /// <summary>
    ///     Values aligned with <see cref="keys" />. Serialized and hand-edited data can leave the two lists at
    ///     different lengths; nothing here repairs or validates that.
    /// </summary>
    public List<TValue> values;

    internal DictionaryWrapper(IDictionary<TKey, TValue> dict)
    {
        keys = new(dict.Keys);
        values = new(dict.Values);
    }

    /// <summary>Number of entries, taken from <see cref="keys" />.</summary>
    /// <value>The length of <see cref="keys" />, regardless of how many values were stored.</value>
    public int Count => keys.Count;

    /// <summary>Finds the key by scanning <see cref="keys" />, then reads the value at that index.</summary>
    /// <param name="key">The key to look for; compared with the default equality comparer of <typeparamref name="TKey" />.</param>
    /// <param name="value">The matching value, otherwise the default of <typeparamref name="TValue" />.</param>
    /// <returns>Whether the key is present.</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
        var index = keys.IndexOf(key);

        if (index < 0)
        {
            value = default!;
            return false;
        }

        value = values[index];
        return true;
    }

    /// <summary>The value for <paramref name="key" />.</summary>
    /// <param name="key">The key whose value is wanted; a duplicate key resolves to its first occurrence.</param>
    /// <value>The value stored at the same index as the first matching key.</value>
    /// <exception cref="KeyNotFoundException">The key is not stored in this wrapper.</exception>
    public TValue this[TKey key]
    {
        get
        {
            var index = keys.IndexOf(key);
            if (index < 0)
                throw new KeyNotFoundException($"Key '{key}' was not found.");

            return values[index];
        }
    }
    /// <summary>The keys in stored order.</summary>
    /// <value>The live <see cref="keys" /> list, so changes to it are visible here.</value>
    public IEnumerable<TKey> Keys => keys;
    /// <summary>The values in stored order.</summary>
    /// <value>The live <see cref="values" /> list, so changes to it are visible here.</value>
    public IEnumerable<TValue> Values => values;

    /// <summary>Pairs each key with the value at the same index.</summary>
    /// <returns>An enumerator over freshly created key/value pairs; it reads the lists as it advances.</returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
        keys.Select((t, i) => new KeyValuePair<TKey, TValue>(t, values[i])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Whether <see cref="keys" /> contains <paramref name="key" />.</summary>
    /// <param name="key">The key to look for, using the default equality comparer of <typeparamref name="TKey" />.</param>
    /// <returns><see langword="true" /> when the key is stored.</returns>
    public bool ContainsKey(TKey key) => keys.Contains(key);

    /// <summary>Whether <see cref="values" /> contains <paramref name="value" />.</summary>
    /// <param name="value">The value to look for, using the default equality comparer of <typeparamref name="TValue" />.</param>
    /// <returns><see langword="true" /> when the value is stored, whether or not its key is still valid.</returns>
    public bool ContainsValue(TValue value) => values.Contains(value);

    /// <summary>
    ///     Copies the pairs into a new <see cref="Dictionary{TKey,TValue}" />. Later changes to either side are not
    ///     shared, and a key that repeats in <see cref="keys" /> collapses to its last value.
    /// </summary>
    /// <returns>A new dictionary sized to <see cref="Count" /> and filled in list order.</returns>
    public Dictionary<TKey, TValue> ToDictionary()
    {
        var dict = new Dictionary<TKey, TValue>(keys.Count);
        for (var i = 0; i < keys.Count; i++)
            dict[keys[i]] = values[i];
        return dict;
    }

    /// <summary>Converts by copying, the same as <see cref="ToDictionary" />.</summary>
    /// <param name="wrapper">The wrapper to copy; it is left untouched.</param>
    /// <returns>A new dictionary holding a snapshot of the pairs.</returns>
    public static implicit operator Dictionary<TKey, TValue>(DictionaryWrapper<TKey, TValue> wrapper) =>
        wrapper.ToDictionary();

    /// <summary>Copies the dictionary into a new wrapper; the dictionary is not kept by reference.</summary>
    /// <param name="dictionary">The source pairs; changing it afterwards does not affect the wrapper.</param>
    /// <returns>A new wrapper whose lists hold the keys and values in the dictionary's enumeration order.</returns>
    public static implicit operator DictionaryWrapper<TKey, TValue>(Dictionary<TKey, TValue> dictionary) =>
        new(dictionary);
}