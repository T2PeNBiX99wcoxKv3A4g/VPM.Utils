using System;
using System.Threading;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace io.github.ykysnk.utils.NonUdon.Logger;

[PublicAPI]
public class Logger<T> : ILogger
{
    private static readonly Lazy<Logger<T>> InstanceInternal =
        new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly string LogPrefix = typeof(T).Name;

    public static Logger<T> Instance => InstanceInternal.Value;

    public void Log(object? message) => Utils.Log(LogPrefix, message);
    public void Log(object? message, Object context) => Utils.Log(LogPrefix, message, context);

    public void LogWarning(object? message) => Utils.LogWarning(LogPrefix, message);
    public void LogWarning(object? message, Object context) => Utils.LogWarning(LogPrefix, message, context);

    public void LogError(object? message) => Utils.LogError(LogPrefix, message);
    public void LogError(object? message, Object context) => Utils.LogError(LogPrefix, message, context);

    public void Assert(bool condition, object? message) => Utils.Assert(condition, LogPrefix, message);

    public void Assert(bool condition, object? message, Object context) =>
        Utils.Assert(condition, LogPrefix, message, context);
}