using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    public static IEnumerable<MethodInfo> GetNestedMethods(Type? type)
    {
        if (type == null)
            yield break;

        Type[] nestedTypes;

        try
        {
            nestedTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
        }
        catch
        {
            yield break;
        }

        foreach (var nestedType in nestedTypes)
        {
            MethodInfo[] methods;

            try
            {
                methods = nestedType.GetMethods(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static |
                    BindingFlags.Instance);
            }
            catch
            {
                continue;
            }

            foreach (var method in methods)
                yield return method;
        }
    }

    public static MethodInfo? GetNestedMethod(Type? type, string name, string? searchName = null)
    {
        if (string.IsNullOrEmpty(searchName))
            searchName = $"<{name}>";

        return GetNestedMethods(type)?.FirstOrDefault(m => m.Name.Contains(searchName));
    }
}