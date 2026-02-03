using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

[Serializable]
[PublicAPI]
public sealed class DictionaryWrapper<TK, TV> : IEnumerable<KeyValuePair<TK, TV>>
{
    public List<TK> keys;
    public List<TV> values;

    internal DictionaryWrapper(IDictionary<TK, TV> dict)
    {
        keys = new(dict.Keys);
        values = new(dict.Values);
    }

    public int Count => keys.Count;

    public TV this[TK key] => values[keys.IndexOf(key)];

    public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator() =>
        keys.Select((t, i) => new KeyValuePair<TK, TV>(t, values[i])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public Dictionary<TK, TV> ToDictionary()
    {
        var dict = new Dictionary<TK, TV>(keys.Count);
        for (var i = 0; i < keys.Count; i++)
            dict[keys[i]] = values[i];
        return dict;
    }

    public bool ContainsKey(TK key) => keys.Contains(key);

    public bool ContainsValue(TV value) => values.Contains(value);

    public static implicit operator Dictionary<TK, TV>(DictionaryWrapper<TK, TV> wrapper) => wrapper.ToDictionary();
    public static implicit operator DictionaryWrapper<TK, TV>(Dictionary<TK, TV> dictionary) => new(dictionary);
}