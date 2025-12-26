using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

[PublicAPI]
public static class HashUtils
{
    public enum HashType
    {
        MD5,
        SHA1,
        SHA256,
        SHA512
    }

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

    public static string ComputeHash<T>(IEnumerable<T> input, HashType type) =>
        ComputeHash(string.Join("|", input), type);

    public static string ListString<T>(this IEnumerable<T> enumerable) => string.Join("|", enumerable);
}