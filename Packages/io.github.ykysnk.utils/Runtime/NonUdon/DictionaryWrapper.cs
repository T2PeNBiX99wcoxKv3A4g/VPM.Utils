using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

[Serializable]
[PublicAPI]
public sealed class DictionaryWrapper<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
{
    public List<TKey> keys;
    public List<TValue> values;

    internal DictionaryWrapper(IDictionary<TKey, TValue> dict)
    {
        keys = new(dict.Keys);
        values = new(dict.Values);
    }

    public int Count => keys.Count;

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

    public TValue this[TKey key] => values[keys.IndexOf(key)];
    public IEnumerable<TKey> Keys => keys;
    public IEnumerable<TValue> Values => values;

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
        keys.Select((t, i) => new KeyValuePair<TKey, TValue>(t, values[i])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool ContainsKey(TKey key) => keys.Contains(key);

    public bool ContainsValue(TValue value) => values.Contains(value);

    public Dictionary<TKey, TValue> ToDictionary()
    {
        var dict = new Dictionary<TKey, TValue>(keys.Count);
        for (var i = 0; i < keys.Count; i++)
            dict[keys[i]] = values[i];
        return dict;
    }

    public static implicit operator Dictionary<TKey, TValue>(DictionaryWrapper<TKey, TValue> wrapper) =>
        wrapper.ToDictionary();

    public static implicit operator DictionaryWrapper<TKey, TValue>(Dictionary<TKey, TValue> dictionary) =>
        new(dictionary);
}