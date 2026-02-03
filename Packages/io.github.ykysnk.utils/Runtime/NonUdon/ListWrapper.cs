using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

[Serializable]
[PublicAPI]
public sealed class ListWrapper<T> : IEnumerable<T>
{
    public List<T> items;

    internal ListWrapper(IEnumerable<T> source) => items = new(source);

    public int Count => items.Count;

    public T this[int index] => items[index];

    public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Contains(T item) => items.Contains(item);

    public static implicit operator List<T>(ListWrapper<T> wrapper) => wrapper.items;
    public static implicit operator ListWrapper<T>(List<T> list) => new(list);
}