using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

/// <summary>
///     Reflection helpers for types that cannot be referenced directly, such as compiler-generated closures and
///     state machines. Failures are logged as warnings and produce a default value instead of an exception.
/// </summary>
[PublicAPI]
public static class ReflectionUtils
{
    /// <summary>
    ///     Creates an instance of <paramref name="type" /> through its parameterless constructor and casts it to
    ///     <typeparamref name="T" />. A missing constructor, a throwing constructor or a failing cast is logged and
    ///     yields <c>default</c>.
    /// </summary>
    /// <param name="type">The concrete type to instantiate.</param>
    /// <returns>The new instance, or <c>default</c> when the type could not be created or cast.</returns>
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

    /// <summary>
    ///     Yields every method, public or not, static or not, declared on the nested types of
    ///     <paramref name="type" />. A nested type that the runtime refuses to enumerate is skipped. The result is
    ///     lazy, so reflection happens as the sequence is walked.
    /// </summary>
    /// <param name="type">The enclosing type; <see langword="null" /> yields nothing.</param>
    /// <returns>The methods of the nested types, in no defined order.</returns>
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

    /// <summary>
    ///     Returns the first method found by <see cref="GetNestedMethods" /> whose name contains
    ///     <paramref name="searchName" />.
    /// </summary>
    /// <param name="type">The enclosing type to search.</param>
    /// <param name="name">Name of the member whose generated body is wanted.</param>
    /// <param name="searchName">
    ///     Fragment to match. Defaults to <c>&lt;name&gt;</c>, which is how the compiler names the generated methods
    ///     of a member called <paramref name="name" /> — lambdas, async bodies and local functions.
    /// </param>
    /// <returns>The first matching method, or <see langword="null" /> when nothing matches.</returns>
    public static MethodInfo? GetNestedMethod(Type? type, string name, string? searchName = null)
    {
        if (string.IsNullOrEmpty(searchName))
            searchName = $"<{name}>";

        return GetNestedMethods(type).FirstOrDefault(m => m.Name.Contains(searchName));
    }
    private const BindingFlags AllMembers = BindingFlags.Public | BindingFlags.NonPublic |
                                            BindingFlags.Static | BindingFlags.Instance;

    /// <summary>
    ///     Resolves a type by full name in the already loaded assemblies, so an optional dependency can be probed
    ///     without referencing it. Returns <c>null</c> instead of throwing when the name does not resolve.
    /// </summary>
    /// <param name="typeName">Full or assembly-qualified type name.</param>
    public static Type? FindType(string? typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return null;

        var type = Type.GetType(typeName);
        if (type != null) return type;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName, false);
            if (type != null) return type;
        }

        return null;
    }

    /// <summary>
    ///     Finds a method by name only, public or not, static or instance, declared on the type or inherited.
    ///     Returns <c>null</c> when the type has no such name instead of throwing on a missing member.
    /// </summary>
    /// <param name="type">Type to search; <see langword="null" /> yields <c>null</c>.</param>
    /// <param name="name">Method name; matching is by name only.</param>
    public static MethodInfo? FindMethod(Type? type, string name) =>
        type?.GetMethods(AllMembers).FirstOrDefault(m => m.Name == name);

    /// <summary>Finds a property by name only, public or not; <c>null</c> when the type has no such name.</summary>
    public static PropertyInfo? FindProperty(Type? type, string name) => type?.GetProperty(name, AllMembers);

    /// <summary>
    ///     Every method declared on the type itself, public or not. Unlike <see cref="GetNestedMethods" /> this does
    ///     not look at nested types.
    /// </summary>
    public static IEnumerable<MethodInfo> GetDeclaredMethods(Type? type) =>
        type == null ? Enumerable.Empty<MethodInfo>() : type.GetMethods(AllMembers | BindingFlags.DeclaredOnly);
}
