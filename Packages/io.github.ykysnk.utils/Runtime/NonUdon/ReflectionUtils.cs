using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

[PublicAPI]
public static class ReflectionUtils
{
    public static T? Instantiate<T>(Type type)
    {
        try
        {
            var loader = (T?)Activator.CreateInstance(type);
            if (loader == null)
                Utils.LogWarning(nameof(ReflectionUtils), $"Failed to instantiate class of type {type}");
            return loader;
        }
        catch (Exception e)
        {
            Utils.LogWarning(nameof(ReflectionUtils),
                $"Failed to instantiate class of type {type}: {e.Message}\n{e.StackTrace}");
            return default;
        }
    }

    public static IEnumerable<MethodInfo>? GetNestedMethods(Type? type) =>
        type?.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
            .SelectMany(t =>
                t.GetMethods(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static |
                    BindingFlags.Instance)).Where(m => !m.IsDefined(typeof(CompilerGeneratedAttribute), false));

    public static MethodInfo? GetNestedMethod(Type? type, string name, string? searchName = null)
    {
        if (string.IsNullOrEmpty(searchName))
            searchName = $"<{name}>";

        return GetNestedMethods(type)?.FirstOrDefault(m => m.Name.Contains(searchName));
    }
}