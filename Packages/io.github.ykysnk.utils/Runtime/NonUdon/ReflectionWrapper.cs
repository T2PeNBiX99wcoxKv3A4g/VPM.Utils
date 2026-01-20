using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

[PublicAPI]
public static class ReflectionWrapper
{
    public delegate TField FieldGetter<out TField>();

    public delegate TField FieldGetter<in TInstance, out TField>(TInstance instance);

    public delegate void FieldSetter<in TField>(TField value);

    public delegate void FieldSetter<in TInstance, in TField>(TInstance instance, TField value);

    public delegate TProperty PropertyGetter<out TProperty>();

    public delegate TProperty PropertyGetter<in TInstance, out TProperty>(TInstance instance);

    public delegate void PropertySetter<in TProperty>(TProperty value);

    public delegate void PropertySetter<in TInstance, in TProperty>(TInstance instance, TProperty value);

    public delegate void WrapAction();

    public delegate void WrapAction<in T1>(T1 arg1);

    public delegate void WrapAction<in T1, in T2>(T1 arg1, T2 arg2);

    public delegate void WrapAction<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);

    public delegate void WrapAction<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

    public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4,
        T5 arg5, T6 arg6);

    public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7>(T1 arg1, T2 arg2, T3 arg3,
        T4 arg4, T5 arg5, T6 arg6, T7 arg7);

    public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8>(T1 arg1, T2 arg2, T3 arg3,
        T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

    public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9>(T1 arg1, T2 arg2,
        T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

    public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10>(T1 arg1,
        T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);

    public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11>(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);

    public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11,
        in T12>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
        T12 arg12);

    public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11,
        in T12, in T13>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
        T11 arg11, T12 arg12, T13 arg13);

    public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11,
        in T12, in T13, in T14>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9,
        T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14);

    public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11,
        in T12, in T13, in T14, in T15>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8,
        T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15);

    public delegate void WrapAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11,
        in T12, in T13, in T14, in T15, in T16>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7,
        T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);

    public delegate TR WrapFunc<out TR>();

    public delegate TR WrapFunc<in T1, out TR>(T1 arg1);

    public delegate TR WrapFunc<in T1, in T2, out TR>(T1 arg1, T2 arg2);

    public delegate TR WrapFunc<in T1, in T2, in T3, out TR>(T1 arg1, T2 arg2, T3 arg3);

    public delegate TR WrapFunc<in T1, in T2, in T3, in T4, out TR>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, out TR>(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

    public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, out TR>(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

    public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, out TR>(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

    public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, out TR>(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

    public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, out TR>(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

    public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, out TR>(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);

    public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11,
        out TR>(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);

    public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12,
        out TR>(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
        T12 arg12);

    public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12,
        in T13, out TR>(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
        T12 arg12, T13 arg13);

    public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12,
        in T13, in T14, out TR>(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
        T12 arg12, T13 arg13, T14 arg14);

    public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12,
        in T13, in T14, in T15, out TR>(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
        T12 arg12, T13 arg13, T14 arg14, T15 arg15);

    public delegate TR WrapFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12,
        in T13, in T14, in T15, in T16, out TR>(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
        T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);

    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                       BindingFlags.NonPublic;

    private static readonly ConcurrentDictionary<string, Type> TypeCache = new();
    private static readonly ConcurrentDictionary<(Type, string), FieldInfo> FieldCache = new();
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo> PropertyCache = new();
    private static readonly ConcurrentDictionary<(Type, string, Type[]), MethodInfo> MethodCache = new();
    private static readonly ConcurrentDictionary<string, Delegate> DelegateCache = new();

    /// <summary>
    ///     Retrieves a Type by its name and caches the result for later requests.
    /// </summary>
    /// <param name="typeName">The fully qualified name of the Type to retrieve.</param>
    /// <returns>The cached Type instance matching the specified name.</returns>
    public static Type GetType(string typeName)
    {
        return typeName == null
            ? throw new ArgumentNullException(nameof(typeName))
            : TypeCache.GetOrAdd(typeName, name => Type.GetType(name, true));
    }

    /// <summary>
    ///     Retrieves a cached Type instance or stores the provided type in the cache.
    /// </summary>
    /// <param name="type">The Type instance to cache or retrieve.</param>
    /// <returns>The cached Type instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided Type is null.</exception>
    public static Type GetType(Type type) => type == null
        ? throw new ArgumentNullException(nameof(type))
        : TypeCache.GetOrAdd(type.FullName, _ => type);

    /// <summary>
    ///     Retrieves a FieldInfo object representing the specified field of a given type,
    ///     and caches the result for optimized future access.
    /// </summary>
    /// <param name="type">The Type containing the field to retrieve.</param>
    /// <param name="fieldName">The name of the field to retrieve.</param>
    /// <returns>A FieldInfo object representing the specified field of the given type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the type or fieldName parameter is null.</exception>
    /// <exception cref="MissingFieldException">Thrown when the specified field does not exist in the given type.</exception>
    public static FieldInfo GetField(Type type, string fieldName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (fieldName == null)
            throw new ArgumentNullException(nameof(fieldName));
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentException("Field name cannot be empty.", nameof(fieldName));
        return FieldCache.GetOrAdd((type, fieldName), key =>
        {
            var (t, f) = key;
            return t.GetField(f, Flags) ?? throw new MissingFieldException(t.FullName, f);
        });
    }

    /// <summary>
    ///     Retrieves a PropertyInfo object representing a specified property from a given type,
    ///     and caches the result for future use.
    /// </summary>
    /// <param name="type">The type from which to retrieve the property information.</param>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <returns>The PropertyInfo object representing the specified property.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when either <paramref name="type" /> or <paramref name="propertyName" /> is null.
    /// </exception>
    /// <exception cref="MissingMemberException">
    ///     Thrown when the specified property cannot be found in the given type.
    /// </exception>
    public static PropertyInfo GetProperty(Type type, string propertyName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (propertyName == null)
            throw new ArgumentNullException(nameof(propertyName));
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException("Property name cannot be empty.", nameof(propertyName));
        return PropertyCache.GetOrAdd((type, propertyName), key =>
        {
            var (t, p) = key;
            return t.GetProperty(p, Flags) ?? throw new MissingMemberException(t.FullName, p);
        });
    }

    /// <summary>
    ///     Retrieves a MethodInfo by its name, parameter types, and declaring type, caching the result for later requests.
    /// </summary>
    /// <param name="type">The Type that declares the method.</param>
    /// <param name="methodName">The name of the method to retrieve.</param>
    /// <param name="parameterTypes">An array of Types representing the method's parameters.</param>
    /// <returns>The cached MethodInfo instance matching the specified criteria.</returns>
    public static MethodInfo GetMethod(Type type, string methodName, params Type[] parameterTypes)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (methodName == null)
            throw new ArgumentNullException(nameof(methodName));
        if (string.IsNullOrEmpty(methodName))
            throw new ArgumentException("Method name cannot be empty.", nameof(methodName));
        return MethodCache.GetOrAdd((type, methodName, parameterTypes), key =>
        {
            var (t, m, p) = key;
            return t.GetMethod(m, Flags, null, p, null) ?? throw new MissingMethodException(t.FullName, m);
        });
    }

    /// <summary>
    ///     Retrieves a delegate of the specified type for a method, identified by its declaring type, name,
    ///     and parameter types, caching the result for later requests.
    /// </summary>
    /// <param name="type">The Type that declares the method.</param>
    /// <param name="methodName">The name of the method to retrieve the delegate for.</param>
    /// <param name="parameterTypes">An array of Types representing the method's parameters.</param>
    /// <typeparam name="TDelegate">The type of the delegate to generate and return.</typeparam>
    /// <returns>The cached delegate of the specified type for the given method.</returns>
    public static TDelegate GetDelegate<TDelegate>(Type type, string methodName, params Type[] parameterTypes)
        where TDelegate : Delegate
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (methodName == null)
            throw new ArgumentNullException(nameof(methodName));
        if (string.IsNullOrEmpty(methodName))
            throw new ArgumentException("Method name cannot be empty.", nameof(methodName));
        var key = $"{type.FullName}.method.{methodName}.{string.Join(",", parameterTypes.Select(t => t.FullName))}";
        return (TDelegate)DelegateCache.GetOrAdd(key, _ =>
        {
            var method = GetMethod(type, methodName, parameterTypes);
            var parameters = method.GetParameters();
            var exprParams = new ParameterExpression[parameters.Length + (method.IsStatic ? 0 : 1)];

            var offset = 0;
            if (!method.IsStatic)
            {
                exprParams[0] = Expression.Parameter(type, "instance");
                offset = 1;
            }

            for (var i = 0; i < parameters.Length; i++)
                exprParams[i + offset] =
                    Expression.Parameter(parameters[i].ParameterType, parameters[i].Name ?? $"arg{i}");

            var call = method.IsStatic
                ? Expression.Call(method, exprParams.Cast<Expression>())
                : Expression.Call(exprParams[0], method, exprParams[1..].Cast<Expression>());

            var lambda = Expression.Lambda<TDelegate>(call, exprParams);

            return lambda.Compile();
        });
    }

    /// <summary>
    ///     Creates a delegate that retrieves the value of a static field of a specified type.
    /// </summary>
    /// <typeparam name="TField">The type of the field to retrieve.</typeparam>
    /// <param name="type">The type declaring the static field.</param>
    /// <param name="fieldName">The name of the static field.</param>
    /// <returns>A delegate that retrieves the value of the specified static field.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> or <paramref name="fieldName" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName" /> is empty or the field does not exist.</exception>
    public static FieldGetter<TField> GetFieldGetter<TField>(Type type, string fieldName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (fieldName == null)
            throw new ArgumentNullException(nameof(fieldName));
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentException("Field name cannot be empty.", nameof(fieldName));
        var key = $"{type.FullName}.field.set.{fieldName}";
        return (FieldGetter<TField>)DelegateCache.GetOrAdd(key, _ =>
        {
            var field = GetField(type, fieldName);
            var fieldAccess = Expression.Field(null, field);
            var lambda = Expression.Lambda<FieldGetter<TField>>(fieldAccess);
            return lambda.Compile();
        });
    }

    /// <summary>
    ///     Retrieves a delegate that provides a getter function for a specified field in a given type.
    /// </summary>
    /// <param name="type">The type containing the field to be accessed.</param>
    /// <param name="fieldName">The name of the field for which the getter will be created.</param>
    /// <typeparam name="TInstance">The type of the object instance from which the field value will be retrieved.</typeparam>
    /// <typeparam name="TField">The type of the field whose value will be retrieved.</typeparam>
    /// <returns>A delegate capable of accessing the field value from a given instance of the specified type.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when the <paramref name="type" /> or <paramref name="fieldName" /> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the <paramref name="fieldName" /> is an empty string.
    /// </exception>
    public static FieldGetter<TInstance, TField> GetFieldGetter<TInstance, TField>(Type type, string fieldName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (fieldName == null)
            throw new ArgumentNullException(nameof(fieldName));
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentException("Field name cannot be empty.", nameof(fieldName));
        var key = $"{type.FullName}.field.get.{fieldName}";
        return (FieldGetter<TInstance, TField>)DelegateCache.GetOrAdd(key, _ =>
        {
            var field = GetField(type, fieldName);
            var instance = Expression.Parameter(typeof(TInstance), "instance");
            var fieldAccess = Expression.Field(instance, field);
            var lambda = Expression.Lambda<FieldGetter<TInstance, TField>>(fieldAccess, instance);
            return lambda.Compile();
        });
    }

    /// <summary>
    ///     Retrieves a delegate capable of setting the value of a specified static field on a given type.
    /// </summary>
    /// <param name="type">The type that contains the static field.</param>
    /// <param name="fieldName">The name of the static field to set.</param>
    /// <typeparam name="TField">The type of the field value to set.</typeparam>
    /// <returns>A delegate allowing the setting of the static field's value.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when the <paramref name="type" /> or <paramref name="fieldName" /> is
    ///     null.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="fieldName" /> is empty or invalid.</exception>
    public static FieldSetter<TField> GetFieldSetter<TField>(Type type, string fieldName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (fieldName == null)
            throw new ArgumentNullException(nameof(fieldName));
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentException("Field name cannot be empty.", nameof(fieldName));
        var key = $"{type.FullName}.field.set.{fieldName}";
        return (FieldSetter<TField>)DelegateCache.GetOrAdd(key, _ =>
        {
            var field = GetField(type, fieldName);
            var value = Expression.Parameter(typeof(TField), "value");
            var assign = Expression.Assign(Expression.Field(null, field), value);
            var lambda = Expression.Lambda<FieldSetter<TField>>(assign, value);
            return lambda.Compile();
        });
    }

    /// <summary>
    ///     Creates a delegate that sets the value of a specified field on an instance of a given type.
    /// </summary>
    /// <param name="type">The Type containing the field to be set.</param>
    /// <param name="fieldName">The name of the field to be set.</param>
    /// <typeparam name="TInstance">The type of the instance containing the field.</typeparam>
    /// <typeparam name="TField">The type of the field to be set.</typeparam>
    /// <returns>A compiled delegate that sets the value of the specified field on the given instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided type or field name is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the provided field name is empty.</exception>
    public static FieldSetter<TInstance, TField> GetFieldSetter<TInstance, TField>(Type type, string fieldName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (fieldName == null)
            throw new ArgumentNullException(nameof(fieldName));
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentException("Field name cannot be empty.", nameof(fieldName));
        var key = $"{type.FullName}.field.set.{fieldName}";
        return (FieldSetter<TInstance, TField>)DelegateCache.GetOrAdd(key, _ =>
        {
            var field = GetField(type, fieldName);
            var instance = Expression.Parameter(typeof(TInstance), "instance");
            var value = Expression.Parameter(typeof(TField), "value");
            var assign = Expression.Assign(Expression.Field(instance, field), value);
            var lambda = Expression.Lambda<FieldSetter<TInstance, TField>>(assign, instance, value);
            return lambda.Compile();
        });
    }

    /// <summary>
    ///     Creates a delegate that retrieves the value of a static property from a specified type.
    /// </summary>
    /// <param name="type">The Type containing the static property.</param>
    /// <param name="propertyName">The name of the static property to retrieve.</param>
    /// <typeparam name="TProperty">The type of the property to retrieve.</typeparam>
    /// <returns>A delegate that retrieves the value of the specified static property.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type" /> or <paramref name="propertyName" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="propertyName" /> is an empty string.</exception>
    public static PropertyGetter<TProperty> GetPropertyGetter<TProperty>(Type type, string propertyName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (propertyName == null)
            throw new ArgumentNullException(nameof(propertyName));
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException("Property name cannot be empty.", nameof(propertyName));
        var key = $"{type.FullName}.property.get.{propertyName}";
        return (PropertyGetter<TProperty>)DelegateCache.GetOrAdd(key, _ =>
        {
            var property = GetProperty(type, propertyName);
            var propertyAccess = Expression.Property(null, property);
            var lambda = Expression.Lambda<PropertyGetter<TProperty>>(propertyAccess);
            return lambda.Compile();
        });
    }

    /// <summary>
    ///     Retrieves a delegate for accessing a property getter on the specified type and caches the result.
    /// </summary>
    /// <param name="type">The Type containing the property to retrieve the getter for.</param>
    /// <param name="propertyName">The name of the property to retrieve the getter for.</param>
    /// <typeparam name="TInstance">The type of the instance that contains the property.</typeparam>
    /// <typeparam name="TProperty">The type of the property to be accessed.</typeparam>
    /// <returns>A delegate that represents the property getter for the specified property.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when the provided <paramref name="type" /> or
    ///     <paramref name="propertyName" /> is null.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName" /> is an empty string.</exception>
    public static PropertyGetter<TInstance, TProperty> GetPropertyGetter<TInstance, TProperty>(Type type,
        string propertyName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (propertyName == null)
            throw new ArgumentNullException(nameof(propertyName));
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException("Property name cannot be empty.", nameof(propertyName));
        var key = $"{type.FullName}.property.get.{propertyName}";
        return (PropertyGetter<TInstance, TProperty>)DelegateCache.GetOrAdd(key, _ =>
        {
            var property = GetProperty(type, propertyName);
            var instance = Expression.Parameter(typeof(TInstance), "instance");
            var propertyAccess = Expression.Property(instance, property);
            var lambda = Expression.Lambda<PropertyGetter<TInstance, TProperty>>(propertyAccess, instance);
            return lambda.Compile();
        });
    }

    /// <summary>
    ///     Retrieves a delegate for setting the value of a static property of the specified type.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property to be set.</typeparam>
    /// <param name="type">The type that defines the static property.</param>
    /// <param name="propertyName">The name of the static property.</param>
    /// <returns>A delegate that sets the value of the specified static property.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="type" /> or <paramref name="propertyName" /> is
    ///     null.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName" /> is an empty string.</exception>
    public static PropertySetter<TProperty> GetPropertySetter<TProperty>(Type type, string propertyName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (propertyName == null)
            throw new ArgumentNullException(nameof(propertyName));
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException("Property name cannot be empty.", nameof(propertyName));
        var key = $"{type.FullName}.property.set.{propertyName}";
        return (PropertySetter<TProperty>)DelegateCache.GetOrAdd(key, _ =>
        {
            var property = GetProperty(type, propertyName);
            var value = Expression.Parameter(typeof(TProperty), "value");
            var assign = Expression.Assign(Expression.Property(null, property), value);
            var lambda = Expression.Lambda<PropertySetter<TProperty>>(assign, value);
            return lambda.Compile();
        });
    }

    /// <summary>
    ///     Retrieves a delegate that sets a property value on an instance of the specified type.
    /// </summary>
    /// <param name="type">The type that declares the property.</param>
    /// <param name="propertyName">The name of the property to set.</param>
    /// <typeparam name="TInstance">The type of the instance on which the property will be set.</typeparam>
    /// <typeparam name="TProperty">The type of the property to be set.</typeparam>
    /// <returns>A delegate that sets the value of the specified property on the given instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type" /> or <paramref name="propertyName" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="propertyName" /> is an empty string.</exception>
    public static PropertySetter<TInstance, TProperty> GetPropertySetter<TInstance, TProperty>(Type type,
        string propertyName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (propertyName == null)
            throw new ArgumentNullException(nameof(propertyName));
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException("Property name cannot be empty.", nameof(propertyName));
        var key = $"{type.FullName}.property.set.{propertyName}";
        return (PropertySetter<TInstance, TProperty>)DelegateCache.GetOrAdd(key, _ =>
        {
            var property = GetProperty(type, propertyName);
            var instance = Expression.Parameter(typeof(TInstance), "instance");
            var value = Expression.Parameter(typeof(TProperty), "value");
            var assign = Expression.Assign(Expression.Property(instance, property), value);
            var lambda = Expression.Lambda<PropertySetter<TInstance, TProperty>>(assign, instance, value);
            return lambda.Compile();
        });
    }

    public static WrapAction GetAction(Type type, string methodName)
        => GetDelegate<WrapAction>(type, methodName);

    public static WrapAction<T1> GetAction<T1>(Type type, string methodName)
        => GetDelegate<WrapAction<T1>>(type, methodName, typeof(T1));

    public static WrapAction<T1, T2> GetAction<T1, T2>(Type type, string methodName)
        => GetDelegate<WrapAction<T1, T2>>(type, methodName, typeof(T1), typeof(T2));

    public static WrapAction<T1, T2, T3> GetAction<T1, T2, T3>(Type type, string methodName)
        => GetDelegate<WrapAction<T1, T2, T3>>(type, methodName, typeof(T1), typeof(T2), typeof(T3));

    public static WrapAction<T1, T2, T3, T4> GetAction<T1, T2, T3, T4>(Type type, string methodName)
        => GetDelegate<WrapAction<T1, T2, T3, T4>>(type, methodName, typeof(T1), typeof(T2), typeof(T3), typeof(T4));

    public static WrapAction<T1, T2, T3, T4, T5> GetAction<T1, T2, T3, T4, T5>(Type type, string methodName)
        => GetDelegate<WrapAction<T1, T2, T3, T4, T5>>(type, methodName, typeof(T1), typeof(T2), typeof(T3),
            typeof(T4), typeof(T5));

    public static WrapAction<T1, T2, T3, T4, T5, T6> GetAction<T1, T2, T3, T4, T5, T6>(Type type, string methodName)
        => GetDelegate<WrapAction<T1, T2, T3, T4, T5, T6>>(type, methodName, typeof(T1), typeof(T2), typeof(T3),
            typeof(T4), typeof(T5), typeof(T6));

    public static WrapAction<T1, T2, T3, T4, T5, T6, T7> GetAction<T1, T2, T3, T4, T5, T6, T7>(Type type,
        string methodName)
        => GetDelegate<WrapAction<T1, T2, T3, T4, T5, T6, T7>>(type, methodName, typeof(T1), typeof(T2), typeof(T3),
            typeof(T4), typeof(T5), typeof(T6), typeof(T7));

    public static WrapAction<T1, T2, T3, T4, T5, T6, T7, T8> GetAction<T1, T2, T3, T4, T5, T6, T7, T8>(Type type,
        string methodName)
        => GetDelegate<WrapAction<T1, T2, T3, T4, T5, T6, T7, T8>>(type, methodName,
            typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));

    public static WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> GetAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Type type,
        string methodName)
        => GetDelegate<WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(type, methodName,
            typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
            typeof(T9));

    public static WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> GetAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        Type type, string methodName)
        => GetDelegate<WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>(type, methodName,
            typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
            typeof(T9), typeof(T10));

    public static WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> GetAction<T1, T2, T3, T4, T5, T6, T7, T8, T9,
        T10, T11>(Type type, string methodName)
        => GetDelegate<WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>(type, methodName,
            typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
            typeof(T9), typeof(T10), typeof(T11));

    public static WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> GetAction<T1, T2, T3, T4, T5, T6, T7, T8,
        T9, T10, T11, T12>(Type type, string methodName)
        => GetDelegate<WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>(type, methodName,
            typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
            typeof(T9), typeof(T10), typeof(T11), typeof(T12));

    public static WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> GetAction<T1, T2, T3, T4, T5, T6, T7,
        T8, T9, T10, T11, T12, T13>(Type type, string methodName)
        => GetDelegate<WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>(type, methodName,
            typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
            typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13));

    public static WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> GetAction<T1, T2, T3, T4, T5,
        T6, T7, T8, T9, T10, T11, T12, T13, T14>(Type type, string methodName)
        => GetDelegate<WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>(type, methodName,
            typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
            typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14));

    public static WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> GetAction<T1, T2, T3, T4,
        T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Type type, string methodName)
        => GetDelegate<WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>(type, methodName,
            typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
            typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15));

    public static WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> GetAction<T1, T2, T3,
        T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Type type, string methodName)
        => GetDelegate<WrapAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>(
            type, methodName,
            typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
            typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16));

    public static WrapFunc<TR> GetFunc<TR>(Type type, string methodName)
        => GetDelegate<WrapFunc<TR>>(type, methodName);

    public static WrapFunc<T1, TR> GetFunc<T1, TR>(Type type, string methodName)
        => GetDelegate<WrapFunc<T1, TR>>(type, methodName, typeof(T1));

    public static WrapFunc<T1, T2, TR> GetFunc<T1, T2, TR>(Type type, string methodName)
        => GetDelegate<WrapFunc<T1, T2, TR>>(type, methodName, typeof(T1), typeof(T2));

    public static WrapFunc<T1, T2, T3, TR> GetFunc<T1, T2, T3, TR>(Type type, string methodName)
        => GetDelegate<WrapFunc<T1, T2, T3, TR>>(type, methodName, typeof(T1), typeof(T2), typeof(T3));

    public static WrapFunc<T1, T2, T3, T4, TR> GetFunc<T1, T2, T3, T4, TR>(Type type, string methodName)
        => GetDelegate<WrapFunc<T1, T2, T3, T4, TR>>(type, methodName, typeof(T1), typeof(T2), typeof(T3),
            typeof(T4));

    public static WrapFunc<T1, T2, T3, T4, T5, TR> GetFunc<T1, T2, T3, T4, T5, TR>(Type type, string methodName)
        => GetDelegate<WrapFunc<T1, T2, T3, T4, T5, TR>>(type, methodName, typeof(T1), typeof(T2), typeof(T3),
            typeof(T4), typeof(T5));

    public static WrapFunc<T1, T2, T3, T4, T5, T6, TR> GetFunc<T1, T2, T3, T4, T5, T6, TR>(Type type,
        string methodName)
        => GetDelegate<WrapFunc<T1, T2, T3, T4, T5, T6, TR>>(type, methodName, typeof(T1), typeof(T2), typeof(T3),
            typeof(T4), typeof(T5), typeof(T6));

    public static WrapFunc<T1, T2, T3, T4, T5, T6, T7, TR> GetFunc<T1, T2, T3, T4, T5, T6, T7, TR>(Type type,
        string methodName)
        => GetDelegate<WrapFunc<T1, T2, T3, T4, T5, T6, T7, TR>>(type, methodName, typeof(T1), typeof(T2), typeof(T3),
            typeof(T4), typeof(T5), typeof(T6), typeof(T7));

    public static WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, TR> GetFunc<T1, T2, T3, T4, T5, T6, T7, T8, TR>(Type type,
        string methodName)
        => GetDelegate<WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, TR>>(type, methodName, typeof(T1), typeof(T2),
            typeof(T3),
            typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));

    public static WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TR> GetFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TR>(
        Type type,
        string methodName)
        => GetDelegate<WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TR>>(type, methodName, typeof(T1), typeof(T2),
            typeof(T3),
            typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));

    public static WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TR> GetFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9,
        T10, TR>(Type type,
        string methodName)
        => GetDelegate<WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TR>>(type, methodName, typeof(T1),
            typeof(T2),
            typeof(T3),
            typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));

    public static WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TR> GetFunc<T1, T2, T3, T4, T5, T6, T7, T8,
        T9, T10, T11, TR>(Type type,
        string methodName)
        => GetDelegate<WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TR>>(type, methodName, typeof(T1),
            typeof(T2),
            typeof(T3),
            typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11));

    public static WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TR> GetFunc<T1, T2, T3, T4, T5, T6, T7,
        T8, T9, T10, T11, T12, TR>(Type type,
        string methodName)
        => GetDelegate<WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TR>>(type, methodName, typeof(T1),
            typeof(T2),
            typeof(T3),
            typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11),
            typeof(T12));

    public static WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TR> GetFunc<T1, T2, T3, T4, T5, T6,
        T7, T8, T9, T10, T11, T12, T13, TR>(Type type,
        string methodName)
        => GetDelegate<WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TR>>(type, methodName,
            typeof(T1), typeof(T2),
            typeof(T3),
            typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11),
            typeof(T12), typeof(T13));

    public static WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TR> GetFunc<T1, T2, T3, T4,
        T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TR>(Type type, string methodName)
        => GetDelegate<WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TR>>(type, methodName,
            typeof(T1), typeof(T2),
            typeof(T3),
            typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11),
            typeof(T12), typeof(T13), typeof(T14));

    public static WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TR> GetFunc<T1, T2, T3,
        T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TR>(Type type, string methodName)
        => GetDelegate<WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TR>>(type,
            methodName,
            typeof(T1), typeof(T2),
            typeof(T3),
            typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11),
            typeof(T12), typeof(T13), typeof(T14), typeof(T15));

    public static WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TR> GetFunc<T1, T2,
        T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TR>(Type type,
        string methodName)
        => GetDelegate<WrapFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TR>>(type,
            methodName,
            typeof(T1), typeof(T2),
            typeof(T3),
            typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11),
            typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16));
}