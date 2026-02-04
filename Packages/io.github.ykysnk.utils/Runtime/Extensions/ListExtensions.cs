using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class ListExtensions
    {
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

        [CanBeNull]
        public static T GetValueOrDefault<T>(this IList<T> list, int index) =>
            list.TryGetValue(index, out var value) ? value : default;

        public static bool TrySetValue<T>(this IList<T> list, int index, T value)
        {
            if (index < 0 || index >= list.Count) return false;
            list[index] = value;
            return true;
        }

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

        [CanBeNull]
        public static T GetValueOrDefault<T>(this T[] array, int index) =>
            array.TryGetValue(index, out var value) ? value : default;

        public static bool TrySetValue<T>(this T[] array, int index, T value)
        {
            if (index < 0 || index >= array.Length) return false;
            array[index] = value;
            return true;
        }

        public static void Rebuild<T>(this ICollection<T> collection)
        {
            var newList = collection.Distinct().ToList();
            collection.Clear();
            collection.AddRange(newList);
        }

        public static void Rebuild<T>(this ICollection<T> collection, Func<T, bool> removeAll)
        {
            collection.RemoveAll(removeAll);
            collection.Rebuild();
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items) collection.Add(item);
        }

        public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> match)
        {
            var toRemove = collection.Where(match).ToList();

            foreach (var item in toRemove)
                collection.Remove(item);
        }
    }
}