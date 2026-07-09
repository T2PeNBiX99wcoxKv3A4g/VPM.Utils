using System;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

[PublicAPI]
public static class ReflectionUtils
{
    public static T? Instantiate<T>(Type type)
    {
        var loader = (T?)Activator.CreateInstance(type);
        if (loader == null)
            Utils.LogWarning(nameof(ReflectionUtils), $"Failed to instantiate class of type {type}");
        return loader;
    }
}