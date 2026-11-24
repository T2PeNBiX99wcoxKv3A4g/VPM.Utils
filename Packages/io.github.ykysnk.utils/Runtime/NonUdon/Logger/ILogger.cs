using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.NonUdon.Logger;

/// <summary>
///     The logging surface this package codes against, so callers can swap the implementation. It mirrors
///     <see cref="Debug" />, with every entry carrying a caller-supplied prefix.
/// </summary>
[PublicAPI]
public interface ILogger
{
    /// <summary>Writes the message at info level.</summary>
    /// <param name="message">The payload; it is converted to text by Unity, so null logs as an empty message.</param>
    void Log(object? message);
    /// <summary>Writes the message at info level, linked to <paramref name="context" /> for console navigation.</summary>
    /// <param name="message">The payload; it is converted to text by Unity.</param>
    /// <param name="context">The object that clicking the entry selects; null is accepted but cannot be selected.</param>
    void Log(object? message, Object context);
    /// <summary>Writes the message as a warning; execution continues.</summary>
    /// <param name="message">The payload; it is converted to text by Unity.</param>
    void LogWarning(object? message);
    /// <summary>Writes the message as a warning, linked to <paramref name="context" /> for console navigation.</summary>
    /// <param name="message">The payload; it is converted to text by Unity.</param>
    /// <param name="context">The object that clicking the entry selects; null is accepted but cannot be selected.</param>
    void LogWarning(object? message, Object context);
    /// <summary>Writes the message as an error; execution continues.</summary>
    /// <param name="message">The payload; it is converted to text by Unity.</param>
    void LogError(object? message);
    /// <summary>Writes the message as an error, linked to <paramref name="context" /> for console navigation.</summary>
    /// <param name="message">The payload; it is converted to text by Unity.</param>
    /// <param name="context">The object that clicking the entry selects; null is accepted but cannot be selected.</param>
    void LogError(object? message, Object context);
    /// <summary>Reports the message when <paramref name="condition" /> is false; nothing is written otherwise.</summary>
    /// <param name="condition">The expectation to check. True means no output.</param>
    /// <param name="message">The payload written when the expectation fails; it is converted to text by Unity.</param>
    void Assert(bool condition, object? message);
    /// <summary>Reports the message when <paramref name="condition" /> is false, linked to <paramref name="context" />.</summary>
    /// <param name="condition">The expectation to check. True means no output.</param>
    /// <param name="message">The payload written when the expectation fails; it is converted to text by Unity.</param>
    /// <param name="context">The object that clicking the entry selects; null is accepted but cannot be selected.</param>
    void Assert(bool condition, object? message, Object context);
}