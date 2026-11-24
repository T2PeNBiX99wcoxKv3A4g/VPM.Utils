using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

/// <summary>
///     A list behind a single serialized field, so a member typed as <see cref="IReadOnlyList{T}" /> or another
///     interface still has something Unity can store and show in the inspector.
/// </summary>
[Serializable]
[PublicAPI]
public sealed class ListWrapper<T> : IReadOnlyList<T>
{
    /// <summary>The stored items, in order.</summary>
    public List<T> items;

    internal ListWrapper(IEnumerable<T> source) => items = new(source);

    /// <summary>Number of stored items.</summary>
    /// <value>The length of <see cref="items" />.</value>
    public int Count => items.Count;

    /// <summary>The item at <paramref name="index" />; an invalid index throws exactly like the backing list.</summary>
    /// <param name="index">Zero-based position into <see cref="items" />.</param>
    /// <value>The item stored at <paramref name="index" />.</value>
    public T this[int index] => items[index];

    /// <summary>Enumerates <see cref="items" /> directly, without copying it.</summary>
    /// <returns>An enumerator over the backing list.</returns>
    public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Whether <see cref="items" /> contains <paramref name="item" />.</summary>
    /// <param name="item">The item to look for, using the default equality comparer of <typeparamref name="T" />.</param>
    /// <returns><see langword="true" /> when the item is stored.</returns>
    public bool Contains(T item) => items.Contains(item);

    /// <summary>Hands out the backing list itself, not a copy.</summary>
    /// <param name="wrapper">The wrapper to unwrap.</param>
    /// <returns>The live <see cref="items" /> list, so changes made through it affect the wrapper.</returns>
    public static implicit operator List<T>(ListWrapper<T> wrapper) => wrapper.items;
    /// <summary>Copies the list into a new wrapper, so later changes to <paramref name="list" /> are not visible here.</summary>
    /// <param name="list">The items to copy; null throws while the backing list is created.</param>
    /// <returns>A new wrapper holding a snapshot of the list.</returns>
    public static implicit operator ListWrapper<T>(List<T> list) => new(list);
}