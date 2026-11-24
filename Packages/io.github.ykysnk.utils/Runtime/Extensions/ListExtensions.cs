using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Extensions
{
    /// <summary>
    ///     Bounds-checked index access and in-place editing helpers for lists, arrays and other collections.
    /// </summary>
    [PublicAPI]
    public static class ListExtensions
    {
        /// <summary>
        ///     Reads the element at the given index without throwing.
        /// </summary>
        /// <param name="list">The list to read; it is not modified.</param>
        /// <param name="index">Zero-based index; negative or past the end fails the call.</param>
        /// <param name="value">The element, or default(T) when the index is out of range.</param>
        /// <returns>True when the index was valid.</returns>
        public static bool TryGetValue<T>(this IList<T> list, int index, [CanBeNull] out T value)
        {
            if (index < 0 || index >= list.Count)
            {
                value = default;
                return false;
            }

            value = list[index];
            return true;
        }

        /// <summary>
        ///     Element at the given index of the list.
        /// </summary>
        /// <param name="list">The list to read; it is not modified.</param>
        /// <param name="index">Zero-based index; out-of-range values yield default(T) instead of an exception.</param>
        /// <returns>The element, or default(T) when the index is out of range.</returns>
        [CanBeNull]
        public static T GetValueOrDefault<T>(this IList<T> list, int index) =>
            list.TryGetValue(index, out var value) ? value : default;

        /// <summary>
        ///     Overwrites the element at the given index without throwing.
        /// </summary>
        /// <param name="list">The list to write to; a read-only list still throws from its indexer.</param>
        /// <param name="index">Zero-based index; out-of-range values leave the list untouched.</param>
        /// <param name="value">The value to store, including null for reference types.</param>
        /// <returns>True when the index was valid and the value was written.</returns>
        public static bool TrySetValue<T>(this IList<T> list, int index, T value)
        {
            if (index < 0 || index >= list.Count) return false;
            list[index] = value;
            return true;
        }

        /// <summary>
        ///     Array counterpart of the list overload, using the array length as the bound.
        /// </summary>
        /// <param name="array">The array to read; it is not modified.</param>
        /// <param name="index">Zero-based index; negative or past the end fails the call.</param>
        /// <param name="value">The element, or default(T) when the index is out of range.</param>
        /// <returns>True when the index was valid.</returns>
        public static bool TryGetValue<T>(this T[] array, int index, [CanBeNull] out T value)
        {
            if (index < 0 || index >= array.Length)
            {
                value = default;
                return false;
            }

            value = array[index];
            return true;
        }

        /// <summary>
        ///     Element at the given index of the array.
        /// </summary>
        /// <param name="array">The array to read; it is not modified.</param>
        /// <param name="index">Zero-based index; out-of-range values yield default(T) instead of an exception.</param>
        /// <returns>The element, or default(T) when the index is out of range.</returns>
        [CanBeNull]
        public static T GetValueOrDefault<T>(this T[] array, int index) =>
            array.TryGetValue(index, out var value) ? value : default;

        /// <summary>
        ///     Overwrites the element at the given index of the array without throwing.
        /// </summary>
        /// <param name="array">The array to write to; it is mutated in place.</param>
        /// <param name="index">Zero-based index; out-of-range values leave the array untouched.</param>
        /// <param name="value">The value to store, including null for reference types.</param>
        /// <returns>True when the index was valid and the value was written.</returns>
        public static bool TrySetValue<T>(this T[] array, int index, T value)
        {
            if (index < 0 || index >= array.Length) return false;
            array[index] = value;
            return true;
        }

        /// <summary>
        ///     Removes duplicate entries in place, keeping the first occurrence of each value and the relative order of the
        ///     rest (default equality comparer). The collection is cleared and refilled, so it must support Clear and Add
        ///     and any enumeration started before the call is invalidated.
        /// </summary>
        /// <param name="collection">The collection to compact; arrays and other fixed-size collections throw.</param>
        public static void Rebuild<T>(this ICollection<T> collection)
        {
            var newList = collection.Distinct().ToList();
            collection.Clear();
            collection.AddRange(newList);
        }

        /// <summary>
        ///     Removes the elements that match first, then removes duplicates exactly as the parameterless overload does.
        /// </summary>
        /// <param name="collection">The collection to compact; it is mutated in place.</param>
        /// <param name="removeAll">Return true to drop an element; evaluated once per element before deduplication.</param>
        public static void Rebuild<T>(this ICollection<T> collection, Func<T, bool> removeAll)
        {
            collection.RemoveAll(removeAll);
            collection.Rebuild();
        }

        /// <summary>
        ///     Adds the items one by one; ICollection&lt;T&gt; itself has no bulk add.
        /// </summary>
        /// <param name="collection">The collection to append to; it is mutated in place.</param>
        /// <param name="items">Items to append in enumeration order; an empty sequence is a no-op.</param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items) collection.Add(item);
        }

        /// <summary>
        ///     Removes every element that matches. The matches are collected into a temporary list first, so the collection
        ///     is never enumerated while it is being modified.
        /// </summary>
        /// <param name="collection">The collection to filter; it is mutated in place.</param>
        /// <param name="match">Return true for an element to remove; equality is used, one removal per match.</param>
        public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> match)
        {
            var toRemove = collection.Where(match).ToList();

            foreach (var item in toRemove)
                collection.Remove(item);
        }
    }
}