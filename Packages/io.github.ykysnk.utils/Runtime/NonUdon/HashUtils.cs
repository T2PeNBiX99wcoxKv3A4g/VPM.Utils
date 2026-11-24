using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

/// <summary>
///     Text hashing helpers. Every digest comes back as a lowercase hexadecimal string, so results can be compared
///     or stored as plain text.
/// </summary>
[PublicAPI]
public static class HashUtils
{
    /// <summary>Algorithms accepted by <see cref="ComputeHash(string, HashType)" />.</summary>
    public enum HashType
    {
        /// <summary>128-bit MD5. Fast, but not collision resistant.</summary>
        MD5,
        /// <summary>160-bit SHA-1. Not collision resistant either.</summary>
        SHA1,
        /// <summary>256-bit SHA-256.</summary>
        SHA256,
        /// <summary>512-bit SHA-512.</summary>
        SHA512
    }

    /// <summary>Hashes the UTF-8 bytes of <paramref name="input" /> with the chosen algorithm.</summary>
    /// <param name="input">The text to hash; null is rejected by <see cref="Encoding.UTF8" /> rather than treated as empty.</param>
    /// <param name="type">The algorithm to use; anything outside <see cref="HashType" /> throws.</param>
    /// <returns>The digest as lowercase hexadecimal, two characters per byte (32 characters for MD5, 128 for SHA-512).</returns>
    public static string ComputeHash(string input, HashType type)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes;

        switch (type)
        {
            case HashType.MD5:
                using (var md5 = MD5.Create())
                    hashBytes = md5.ComputeHash(bytes);
                break;

            case HashType.SHA1:
                using (var sha1 = SHA1.Create())
                    hashBytes = sha1.ComputeHash(bytes);
                break;

            case HashType.SHA256:
                using (var sha256 = SHA256.Create())
                    hashBytes = sha256.ComputeHash(bytes);
                break;

            case HashType.SHA512:
                using (var sha512 = SHA512.Create())
                    hashBytes = sha512.ComputeHash(bytes);
                break;

            default:
                throw new ArgumentException("Unsupported hash type", nameof(type));
        }

        var sb = new StringBuilder();
        foreach (var b in hashBytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    /// <summary>
    ///     Joins the items with <c>|</c> and hashes the result. Because the separator is not escaped, two different
    ///     splits of the same text produce the same digest, so this suits cache keys rather than signatures.
    /// </summary>
    /// <param name="input">The items to hash, joined in enumeration order.</param>
    /// <param name="type">The algorithm to use; anything outside <see cref="HashType" /> throws.</param>
    /// <returns>The digest of the joined text as lowercase hexadecimal.</returns>
    public static string ComputeHash<T>(IEnumerable<T> input, HashType type) =>
        ComputeHash(string.Join("|", input), type);

    /// <summary>Joins the items with <c>|</c>, the same text form that <see cref="ComputeHash{T}(IEnumerable{T}, HashType)" /> hashes.</summary>
    /// <param name="enumerable">The items to join, in enumeration order.</param>
    /// <returns>The item texts separated by <c>|</c>, with no escaping.</returns>
    public static string ListString<T>(this IEnumerable<T> enumerable) => string.Join("|", enumerable);
}