using System.Collections.Generic;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Extensions;

[PublicAPI]
public static class ListExtensions
{
    public static bool TryGetValue<T>(this IList<T> list, int index, out T? value)
    {
        if (index < 0 || index >= list.Count)
        {
            value = default;
            return false;
        }

        value = list[index];
        return true;
    }

    public static T? GetValueOrDefault<T>(this IList<T> list, int index) =>
        list.TryGetValue(index, out var value) ? value : default;

    public static bool TrySetValue<T>(this IList<T> list, int index, T value)
    {
        if (index < 0 || index >= list.Count) return false;
        list[index] = value;
        return true;
    }

    public static bool TryGetValue<T>(this T[] array, int index, out T? value)
    {
        if (index < 0 || index >= array.Length)
        {
            value = default;
            return false;
        }

        value = array[index];
        return true;
    }

    public static T? GetValueOrDefault<T>(this T[] array, int index) =>
        array.TryGetValue(index, out var value) ? value : default;

    public static bool TrySetValue<T>(this T[] array, int index, T value)
    {
        if (index < 0 || index >= array.Length) return false;
        array[index] = value;
        return true;
    }
}