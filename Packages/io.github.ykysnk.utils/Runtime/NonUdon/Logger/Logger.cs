using System;
using System.Threading;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.utils.NonUdon.Logger;

/// <summary>
///     Logger that prefixes every entry with the name of <typeparamref name="T" />, so a console line names the type
///     it came from without the prefix being written at each call site. One instance is shared per
///     <typeparamref name="T" />.
/// </summary>
[PublicAPI]
public class Logger<T> : ILogger
{
    private static readonly Lazy<Logger<T>> InstanceInternal =
        new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly string LogPrefix = typeof(T).Name;

    /// <summary>The shared logger for <typeparamref name="T" />, created on first use and reused afterwards.</summary>
    /// <value>A thread-safe singleton; logging from several threads reaches the same instance.</value>
    public static Logger<T> Instance => InstanceInternal.Value;

    /// <summary>Writes an info-level message prefixed with the type name.</summary>
    /// <param name="message">The payload; null logs as an empty message.</param>
    public void Log(object? message) => Utils.Log(LogPrefix, message);
    /// <summary>Writes an info-level message prefixed with the type name, linked to <paramref name="context" />.</summary>
    /// <param name="message">The payload; null logs as an empty message.</param>
    /// <param name="context">The object that clicking the entry selects.</param>
    public void Log(object? message, Object context) => Utils.Log(LogPrefix, message, context);

    /// <summary>Writes a warning prefixed with the type name; execution continues.</summary>
    /// <param name="message">The payload; null logs as an empty message.</param>
    public void LogWarning(object? message) => Utils.LogWarning(LogPrefix, message);
    /// <summary>Writes a warning prefixed with the type name, linked to <paramref name="context" />.</summary>
    /// <param name="message">The payload; null logs as an empty message.</param>
    /// <param name="context">The object that clicking the entry selects.</param>
    public void LogWarning(object? message, Object context) => Utils.LogWarning(LogPrefix, message, context);

    /// <summary>Writes an error prefixed with the type name; execution continues.</summary>
    /// <param name="message">The payload; null logs as an empty message.</param>
    public void LogError(object? message) => Utils.LogError(LogPrefix, message);
    /// <summary>Writes an error prefixed with the type name, linked to <paramref name="context" />.</summary>
    /// <param name="message">The payload; null logs as an empty message.</param>
    /// <param name="context">The object that clicking the entry selects.</param>
    public void LogError(object? message, Object context) => Utils.LogError(LogPrefix, message, context);

    /// <summary>Reports the message when <paramref name="condition" /> is false; nothing is written otherwise.</summary>
    /// <param name="condition">The expectation to check. True means no output.</param>
    /// <param name="message">The payload written when the expectation fails.</param>
    public void Assert(bool condition, object? message) => Utils.Assert(condition, LogPrefix, message);

    /// <summary>Reports the message when <paramref name="condition" /> is false, linked to <paramref name="context" />.</summary>
    /// <param name="condition">The expectation to check. True means no output.</param>
    /// <param name="message">The payload written when the expectation fails.</param>
    /// <param name="context">The object that clicking the entry selects.</param>
    public void Assert(bool condition, object? message, Object context) =>
        Utils.Assert(condition, LogPrefix, message, context);
}