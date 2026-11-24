using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace io.github.ykysnk.utils.NonUdon.Reflection
{
    // Member lookup and accessor cache behind Reflect. Each member is resolved once and its accessor is compiled once,
    // so the hot path is a delegate call; when compiling is not available (for example under IL2CPP) the accessor falls
    // back to plain reflection instead of failing.
    internal static class MemberAccessors
    {
        private const BindingFlags DeclaredFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                                   BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        // Failed lookups are cached as null on purpose, so a missing member is not searched again on every access.
        private static readonly ConcurrentDictionary<(Type, string), FieldInfo?> Fields = new();
        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> Properties = new();
        private static readonly ConcurrentDictionary<string, MethodInfo?> Methods = new();

        private static readonly ConcurrentDictionary<FieldInfo, Func<object, object?>?> FieldGetters = new();
        private static readonly ConcurrentDictionary<FieldInfo, Action<object, object?>?> FieldSetters = new();
        private static readonly ConcurrentDictionary<PropertyInfo, Func<object, object?>?> PropertyGetters = new();
        private static readonly ConcurrentDictionary<PropertyInfo, Action<object, object?>?> PropertySetters = new();
        private static readonly ConcurrentDictionary<MethodInfo, Func<object, object?[], object?>?> MethodInvokers = new();

        internal static FieldInfo? FindField(Type type, string name) =>
            Fields.GetOrAdd((type, name), key =>
            {
                foreach (var current in Hierarchy(key.Item1))
                {
                    var field = current.GetField(key.Item2, DeclaredFlags);
                    if (field != null) return field;
                }

                return null;
            });

        internal static PropertyInfo? FindProperty(Type type, string name) =>
            Properties.GetOrAdd((type, name), key =>
            {
                foreach (var current in Hierarchy(key.Item1))
                {
                    var property = current.GetProperty(key.Item2, DeclaredFlags);
                    if (property != null) return property;
                }

                return null;
            });

        internal static MethodInfo? FindMethod(Type type, string name, object?[] arguments, bool staticContext) =>
            Methods.GetOrAdd(MethodKey(type, name, arguments, staticContext),
                _ => ResolveMethod(type, name, arguments, staticContext));

        internal static Func<object, object?>? GetFieldGetter(FieldInfo field) =>
            FieldGetters.GetOrAdd(field, BuildFieldGetter);

        internal static Action<object, object?>? GetFieldSetter(FieldInfo field) =>
            FieldSetters.GetOrAdd(field, BuildFieldSetter);

        internal static Func<object, object?>? GetPropertyGetter(PropertyInfo property) =>
            PropertyGetters.GetOrAdd(property, BuildPropertyGetter);

        internal static Action<object, object?>? GetPropertySetter(PropertyInfo property) =>
            PropertySetters.GetOrAdd(property, BuildPropertySetter);

        internal static Func<object, object?[], object?>? GetMethodInvoker(MethodInfo method) =>
            MethodInvokers.GetOrAdd(method, BuildMethodInvoker);

        internal static MissingFieldException MissingField(Type type, string name) =>
            new($"Field '{name}' was not found on '{type.FullName}'.");

        internal static MissingMemberException MissingProperty(Type type, string name) =>
            new($"Property '{name}' was not found or cannot be read on '{type.FullName}'.");

        internal static MissingMethodException MissingMethod(Type type, string name, object?[] arguments) =>
            new($"No overload of '{name}' on '{type.FullName}' matches " +
                $"({string.Join(", ", Array.ConvertAll(arguments, a => a?.GetType().Name ?? "null"))}).");

        // DeclaredOnly flags stop at each type, so walking the chain is what reaches private members of base types.
        private static IEnumerable<Type> Hierarchy(Type type)
        {
            for (var current = type; current != null; current = current.BaseType)
                yield return current;
        }

        // Overload choice depends on the runtime argument types, and a null argument is recorded as "null", so a null
        // call gets its own key instead of colliding with a typed one.
        private static string MethodKey(Type type, string name, object?[] arguments, bool staticContext)
        {
            var builder = new StringBuilder(64);
            builder.Append(staticContext ? 's' : 'i').Append('|').Append(type.FullName).Append('|').Append(name);

            foreach (var argument in arguments)
                builder.Append('|').Append(argument?.GetType().AssemblyQualifiedName ?? "null");

            return builder.ToString();
        }

        private static MethodInfo? ResolveMethod(Type type, string name, object?[] arguments, bool staticContext)
        {
            MethodInfo? best = null;
            var bestScore = -1;

            foreach (var current in Hierarchy(type))
            foreach (var candidate in current.GetMethods(DeclaredFlags))
            {
                if (candidate.Name != name || candidate.IsGenericMethodDefinition) continue;

                var parameters = candidate.GetParameters();
                if (parameters.Length != arguments.Length) continue;

                var score = Score(parameters, arguments, out var matches);
                if (!matches) continue;

                // Prefer instance methods when there is a target, static ones when there is none.
                if (candidate.IsStatic == staticContext) score += 3;

                if (score <= bestScore) continue;

                best = candidate;
                bestScore = score;
            }

            return best;
        }

        // An exact parameter type scores above an assignable one, so the most specific overload wins.
        private static int Score(ParameterInfo[] parameters, object?[] arguments, out bool matches)
        {
            var score = 0;
            matches = true;

            for (var i = 0; i < parameters.Length; i++)
            {
                var argument = arguments[i];
                var parameterType = parameters[i].ParameterType;

                if (argument == null)
                {
                    if (parameterType.IsValueType && Nullable.GetUnderlyingType(parameterType) == null)
                    {
                        matches = false;
                        return 0;
                    }

                    score += 1;
                    continue;
                }

                var argumentType = argument.GetType();

                if (parameterType == argumentType)
                    score += 4;
                else if (parameterType.IsAssignableFrom(argumentType))
                    score += parameterType.IsValueType ? 1 : 2;
                else
                {
                    matches = false;
                    return 0;
                }
            }

            return score;
        }

        private static Func<object, object?>? BuildFieldGetter(FieldInfo field)
        {
            try
            {
                var instance = Expression.Parameter(typeof(object), "instance");
                Expression access = Expression.Field(
                    field.IsStatic ? null : Expression.Convert(instance, field.DeclaringType!), field);
                if (access.Type.IsValueType) access = Expression.Convert(access, typeof(object));

                return Expression.Lambda<Func<object, object?>>(access, instance).Compile();
            }
            catch (Exception)
            {
                return field.GetValue;
            }
        }

        private static Action<object, object?>? BuildFieldSetter(FieldInfo field)
        {
            try
            {
                var instance = Expression.Parameter(typeof(object), "instance");
                var value = Expression.Parameter(typeof(object), "value");
                var target = field.IsStatic ? null : Expression.Convert(instance, field.DeclaringType!);
                var assign = Expression.Assign(Expression.Field(target, field),
                    Expression.Convert(value, field.FieldType));

                return Expression.Lambda<Action<object, object?>>(assign, instance, value).Compile();
            }
            catch (Exception)
            {
                // Readonly and const fields cannot be assigned, so report no setter instead of failing at call time.
                if (field.IsInitOnly || field.IsLiteral) return null;
                return (instance, value) => field.SetValue(instance, value);
            }
        }

        private static Func<object, object?>? BuildPropertyGetter(PropertyInfo property)
        {
            try
            {
                var getter = property.GetGetMethod(true);
                if (getter == null) return null;

                var instance = Expression.Parameter(typeof(object), "instance");
                Expression access = Expression.Property(
                    getter.IsStatic ? null : Expression.Convert(instance, property.DeclaringType!), property);
                if (access.Type.IsValueType) access = Expression.Convert(access, typeof(object));

                return Expression.Lambda<Func<object, object?>>(access, instance).Compile();
            }
            catch (Exception)
            {
                return property.GetGetMethod(true) == null ? null : property.GetValue;
            }
        }

        private static Action<object, object?>? BuildPropertySetter(PropertyInfo property)
        {
            try
            {
                var setter = property.GetSetMethod(true);
                if (setter == null) return null;

                var instance = Expression.Parameter(typeof(object), "instance");
                var value = Expression.Parameter(typeof(object), "value");
                var target = setter.IsStatic ? null : Expression.Convert(instance, property.DeclaringType!);
                var assign = Expression.Assign(Expression.Property(target, property),
                    Expression.Convert(value, property.PropertyType));

                return Expression.Lambda<Action<object, object?>>(assign, instance, value).Compile();
            }
            catch (Exception)
            {
                return property.GetSetMethod(true) == null
                    ? null
                    : (instance, value) => property.SetValue(instance, value);
            }
        }

        private static Func<object, object?[], object?>? BuildMethodInvoker(MethodInfo method)
        {
            try
            {
                var instance = Expression.Parameter(typeof(object), "instance");
                var arguments = Expression.Parameter(typeof(object?[]), "arguments");
                var parameters = method.GetParameters();
                var callArguments = new Expression[parameters.Length];

                for (var i = 0; i < parameters.Length; i++)
                    callArguments[i] = Expression.Convert(
                        Expression.ArrayIndex(arguments, Expression.Constant(i)), parameters[i].ParameterType);

                var call = method.IsStatic
                    ? Expression.Call(method, callArguments)
                    : Expression.Call(Expression.Convert(instance, method.DeclaringType!), method,
                        (IEnumerable<Expression>)callArguments);

                // A void method still has to yield something for the shared invoker signature.
                Expression body = method.ReturnType == typeof(void)
                    ? Expression.Block(call, Expression.Constant(null, typeof(object)))
                    : Expression.Convert(call, typeof(object));

                return Expression.Lambda<Func<object, object?[], object?>>(body, instance, arguments).Compile();
            }
            catch (Exception)
            {
                return method.Invoke;
            }
        }
    }
}
