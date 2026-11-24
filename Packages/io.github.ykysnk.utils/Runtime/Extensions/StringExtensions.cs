using System;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Extensions
{
    /// <summary>
    ///     Slicing helpers for delimited strings such as "Folder/Name/Extension". Every path helper returns null when its
    ///     separator does not occur, and none of them validate the receiver.
    /// </summary>
    [PublicAPI]
    public static class StringExtensions
    {
        /// <summary>
        ///     Text before the last occurrence of the separator, separator excluded: "a/b/c" with '/' returns "a/b".
        /// </summary>
        /// <param name="str">The string to slice; it is not modified.</param>
        /// <param name="value">Separator character; the cut uses its last occurrence, not the first.</param>
        /// <returns>The prefix, an empty string when the separator only occurs at index 0, or null when it is absent.</returns>
        [CanBeNull]
        public static string FirstPath(this string str, char value)
        {
            var findEnd = str.LastIndexOf(value);
            return findEnd < 0 ? null : str.Substring(0, findEnd);
        }

        /// <summary>
        ///     String separator version of the char overload, matched ordinally as a whole substring: the prefix is cut
        ///     directly before the last occurrence, so "a::b::c" with "::" returns "a::b".
        /// </summary>
        /// <param name="str">The string to slice; it is not modified.</param>
        /// <param name="value">Separator string, compared with ordinal rules; null or empty throws.</param>
        /// <returns>The prefix, or null when the separator does not occur.</returns>
        [CanBeNull]
        public static string FirstPath(this string str, [NotNull] string value)
        {
            var findEnd = str.LastIndexOf(value, StringComparison.Ordinal);
            return findEnd < 0 ? null : str.Substring(0, findEnd);
        }

        /// <summary>
        ///     Text after the first occurrence of the separator, separator excluded: "a/b/c" with '/' returns "b/c".
        /// </summary>
        /// <param name="str">The string to slice; it is not modified.</param>
        /// <param name="value">Separator character; the cut uses its first occurrence, not the last.</param>
        /// <returns>
        ///     The remainder, an empty string when the separator is the last character, or null when it is absent.
        /// </returns>
        [CanBeNull]
        public static string LastPath(this string str, char value)
        {
            var findStart = str.IndexOf(value);
            return findStart < 0 ? null : str.Substring(findStart + 1);
        }

        /// <summary>
        ///     String separator version of the char overload, matched ordinally; the whole separator is skipped.
        /// </summary>
        /// <param name="str">The string to slice; it is not modified.</param>
        /// <param name="value">Separator string; null throws, an empty string matches at index 0 and returns the input.</param>
        /// <returns>The remainder, or null when the separator does not occur.</returns>
        [CanBeNull]
        public static string LastPath(this string str, [NotNull] string value)
        {
            var findStart = str.IndexOf(value, StringComparison.Ordinal);
            return findStart < 0 ? null : str.Substring(findStart + value.Length);
        }

        /// <summary>
        ///     Text between the two separators: the tail is cut at the last occurrence of <paramref name="last" />, then the
        ///     head at the first occurrence of <paramref name="first" /> in what remains. "Folder/Name/Sub" with '/' for
        ///     both returns "Name".
        /// </summary>
        /// <param name="str">The string to slice; it is not modified.</param>
        /// <param name="first">Separator that opens the wanted segment.</param>
        /// <param name="last">Separator that closes the wanted segment.</param>
        /// <returns>The segment without either separator, or null when one of them is missing.</returns>
        [CanBeNull]
        public static string MiddlePath(this string str, char first, char last) => str.FirstPath(last)?.LastPath(first);

        /// <summary>
        ///     String separator version of the char overload for both ends, matched ordinally:
        ///     "a::b::c" with "::" for both separators returns "b".
        /// </summary>
        /// <param name="str">The string to slice; it is not modified.</param>
        /// <param name="first">Separator that opens the wanted segment.</param>
        /// <param name="last">Separator that closes the wanted segment.</param>
        /// <returns>The segment without either separator, or null when one of them is missing.</returns>
        [CanBeNull]
        public static string MiddlePath(this string str, [NotNull] string first, [NotNull] string last) =>
            str.FirstPath(last)?.LastPath(first);

        /// <summary>
        ///     Mixed overload: the segment is closed by the string <paramref name="last" /> and opened by the character
        ///     <paramref name="first" />.
        /// </summary>
        /// <param name="str">The string to slice; it is not modified.</param>
        /// <param name="first">Character separator that opens the wanted segment.</param>
        /// <param name="last">String separator that closes the wanted segment.</param>
        /// <returns>The segment without either separator, or null when one of them is missing.</returns>
        [CanBeNull]
        public static string MiddlePath(this string str, char first, string last) =>
            str.FirstPath(last)?.LastPath(first);

        /// <summary>
        ///     Mixed overload: the segment is closed by the character <paramref name="last" /> and opened by the string
        ///     <paramref name="first" />.
        /// </summary>
        /// <param name="str">The string to slice; it is not modified.</param>
        /// <param name="first">String separator that opens the wanted segment.</param>
        /// <param name="last">Character separator that closes the wanted segment.</param>
        /// <returns>The segment without either separator, or null when one of them is missing.</returns>
        [CanBeNull]
        public static string MiddlePath(this string str, [NotNull] string first, char last) =>
            str.FirstPath(last)?.LastPath(first);

        /// <summary>
        ///     Reads the character at the given index without throwing.
        /// </summary>
        /// <param name="str">The string to read; it is not modified.</param>
        /// <param name="index">Zero-based index; negative or past the end fails the call.</param>
        /// <param name="value">The character, or null when the index is out of range.</param>
        /// <returns>True when the index was valid.</returns>
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

        /// <summary>
        ///     Character at the given index of the string.
        /// </summary>
        /// <param name="str">The string to read; it is not modified.</param>
        /// <param name="index">Zero-based index; out-of-range values yield null instead of an exception.</param>
        /// <returns>The character, or null when the index is out of range.</returns>
        public static char? GetValueOrDefault(this string str, int index) =>
            str.TryGetValue(index, out var value) ? value : null;
    }
}