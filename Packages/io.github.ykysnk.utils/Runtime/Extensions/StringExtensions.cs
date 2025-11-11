using System;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class StringExtensions
    {
        public static string? FirstPath(this string str, char value)
        {
            var findEnd = str.LastIndexOf(value);
            return findEnd < 0 ? null : str[..findEnd];
        }

        public static string? FirstPath(this string str, string value)
        {
            var findEnd = str.LastIndexOf(value, StringComparison.Ordinal);
            return findEnd < 0 ? null : str[..(findEnd - value.Length + 1)];
        }

        public static string? LastPath(this string str, char value)
        {
            var findStart = str.IndexOf(value);
            return findStart < 0 ? null : str[(findStart + 1)..];
        }

        public static string? LastPath(this string str, string value)
        {
            var findStart = str.IndexOf(value, StringComparison.Ordinal);
            return findStart < 0 ? null : str[(findStart + value.Length)..];
        }

        public static string? MiddlePath(this string str, char first, char last) => str.FirstPath(last)?.LastPath(first);

        public static string? MiddlePath(this string str, string first, string last) =>
            str.FirstPath(last)?.LastPath(first);

        public static string? MiddlePath(this string str, char first, string last) => str.FirstPath(last)?.LastPath(first);

        public static string? MiddlePath(this string str, string first, char last) => str.FirstPath(last)?.LastPath(first);

        public static bool TryGetValue(this string str, int index, out char? value)
        {
            if (index < 0 || index >= str.Length)
            {
                value = null;
                return false;
            }

            value = str[index];
            return true;
        }

        public static char? GetValueOrDefault(this string str, int index) =>
            str.TryGetValue(index, out var value) ? value : null;
    }
}