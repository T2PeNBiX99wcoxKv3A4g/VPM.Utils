using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.NonUdon.Logger;

[PublicAPI]
public interface ILogger
{
    void Log(object? message);
    void Log(object? message, Object context);
    void LogWarning(object? message);
    void LogWarning(object? message, Object context);
    void LogError(object? message);
    void LogError(object? message, Object context);
    void Assert(bool condition, object? message);
    void Assert(bool condition, object? message, Object context);
}