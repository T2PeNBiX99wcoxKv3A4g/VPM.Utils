using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using io.github.ykysnk.utils.NonUdon.ReflectionDelegate;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

[PublicAPI]
public static class ReflectionWrapper
{
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
    ///     Creates a delegate for retrieving the value of a field from a specified type.
    /// </summary>
    /// <param name="type">The type that contains the field whose value will be retrieved.</param>
    /// <param name="fieldName">The name of the field for which the getter delegate is created.</param>
    /// <returns>A delegate for retrieving the field value, either static or instance-based, depending on the field.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the specified type is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the field name is null or an empty string.</exception>
    public static Delegate GetFieldGetter(Type type, string fieldName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentException("Field name cannot be empty.", nameof(fieldName));
        var key = $"{type.FullName}.field.get.{fieldName}";
        return DelegateCache.GetOrAdd(key, _ =>
        {
            var field = GetField(type, fieldName);

            // static getter → FieldGetter<TField>
            if (field.IsStatic)
            {
                var body = Expression.Field(null, field);
                var delegateType = typeof(FieldGetter<>).MakeGenericType(field.FieldType);
                var lambda = Expression.Lambda(delegateType, body);
                return lambda.Compile();
            }

            // instance getter → FieldGetter<TInstance, TField>
            {
                var instanceParam = Expression.Parameter(type, "instance");
                var body = Expression.Field(instanceParam, field);
                var delegateType = typeof(FieldGetter<,>).MakeGenericType(type, field.FieldType);
                var lambda = Expression.Lambda(delegateType, body, instanceParam);
                return lambda.Compile();
            }
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
    public static FieldGetter<TField> GetFieldGetter<TField>(Type type, string fieldName) =>
        (FieldGetter<TField>)GetFieldGetter(type, fieldName);

    /// <summary>
    ///     Retrieves a delegate that provides a getter function for a specified field in a given type.
    /// </summary>
    /// <param name="fieldName">The name of the field for which the getter will be created.</param>
    /// <typeparam name="TInstance">The type of the object instance from which the field value will be retrieved.</typeparam>
    /// <typeparam name="TField">The type of the field whose value will be retrieved.</typeparam>
    /// <returns>A delegate capable of accessing the field value from a given instance of the specified type.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when the <paramref name="fieldName" /> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the <paramref name="fieldName" /> is an empty string.
    /// </exception>
    public static FieldGetter<TInstance, TField> GetFieldGetter<TInstance, TField>(string fieldName) =>
        (FieldGetter<TInstance, TField>)GetFieldGetter(typeof(TInstance), fieldName);

    public static Delegate GetFieldSetter(Type type, string fieldName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentException("Field name cannot be empty.", nameof(fieldName));
        var key = $"{type.FullName}.field.set.{fieldName}";
        return DelegateCache.GetOrAdd(key, _ =>
        {
            var field = GetField(type, fieldName);

            // static setter → FieldSetter<TField>
            if (field.IsStatic)
            {
                var valueParam = Expression.Parameter(field.FieldType, "value");
                var body = Expression.Assign(Expression.Field(null, field), valueParam);
                var delegateType = typeof(FieldSetter<>).MakeGenericType(field.FieldType);
                var lambda = Expression.Lambda(delegateType, body, valueParam);
                return lambda.Compile();
            }

            // instance setter → FieldSetter<TInstance, TField>
            {
                var instanceParam = Expression.Parameter(type, "instance");
                var valueParam = Expression.Parameter(field.FieldType, "value");
                var body = Expression.Assign(Expression.Field(instanceParam, field), valueParam);
                var delegateType = typeof(FieldSetter<,>).MakeGenericType(type, field.FieldType);
                var lambda = Expression.Lambda(delegateType, body, instanceParam, valueParam);
                return lambda.Compile();
            }
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
    public static FieldSetter<TField> GetFieldSetter<TField>(Type type, string fieldName) =>
        (FieldSetter<TField>)GetFieldSetter(type, fieldName);

    /// <summary>
    ///     Creates a delegate that sets the value of a specified field on an instance of a given type.
    /// </summary>
    /// <param name="fieldName">The name of the field to be set.</param>
    /// <typeparam name="TInstance">The type of the instance containing the field.</typeparam>
    /// <typeparam name="TField">The type of the field to be set.</typeparam>
    /// <returns>A compiled delegate that sets the value of the specified field on the given instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided type or field name is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the provided field name is empty.</exception>
    public static FieldSetter<TInstance, TField> GetFieldSetter<TInstance, TField>(string fieldName) =>
        (FieldSetter<TInstance, TField>)GetFieldSetter(typeof(TInstance), fieldName);

    /// <summary>
    ///     Creates a delegate to retrieve the value of a specified property from a given type.
    /// </summary>
    /// <param name="type">The type that contains the property.</param>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <returns>A delegate that, when executed, retrieves the value of the specified property.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided type is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the provided property name is null or empty.</exception>
    public static Delegate GetPropertyGetter(Type type, string propertyName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException("Property name cannot be empty.", nameof(propertyName));
        var key = $"{type.FullName}.property.get.{propertyName}";
        return DelegateCache.GetOrAdd(key, _ =>
        {
            var property = GetProperty(type, propertyName);
            var getter = property.GetGetMethod(true);

            if (getter == null)
                throw new InvalidOperationException($"Property '{propertyName}' has no getter.");

            // static getter → PropertyGetter<TProperty>
            if (getter.IsStatic)
            {
                var body = Expression.Property(null, property);
                var delegateType = typeof(PropertyGetter<>).MakeGenericType(property.PropertyType);
                var lambda = Expression.Lambda(delegateType, body);
                return lambda.Compile();
            }

            // instance getter → PropertyGetter<TInstance, TProperty>
            {
                var instanceParam = Expression.Parameter(type, "instance");
                var body = Expression.Property(instanceParam, property);
                var delegateType = typeof(PropertyGetter<,>).MakeGenericType(type, property.PropertyType);
                var lambda = Expression.Lambda(delegateType, body, instanceParam);
                return lambda.Compile();
            }
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
    public static PropertyGetter<TProperty> GetPropertyGetter<TProperty>(Type type, string propertyName) =>
        (PropertyGetter<TProperty>)GetPropertyGetter(type, propertyName);

    /// <summary>
    ///     Retrieves a delegate for accessing a property getter on the specified type and caches the result.
    /// </summary>
    /// <param name="propertyName">The name of the property to retrieve the getter for.</param>
    /// <typeparam name="TInstance">The type of the instance that contains the property.</typeparam>
    /// <typeparam name="TProperty">The type of the property to be accessed.</typeparam>
    /// <returns>A delegate that represents the property getter for the specified property.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when the provided <paramref name="propertyName" /> is null.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName" /> is an empty string.</exception>
    public static PropertyGetter<TInstance, TProperty> GetPropertyGetter<TInstance, TProperty>(string propertyName) =>
        (PropertyGetter<TInstance, TProperty>)GetPropertyGetter(typeof(TInstance), propertyName);

    /// <summary>
    ///     Retrieves a delegate for the setter of a specified property on a given type.
    /// </summary>
    /// <param name="type">The type to which the property belongs.</param>
    /// <param name="propertyName">The name of the property whose setter is being retrieved.</param>
    /// <returns>A delegate representing the setter for the specified property.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided type is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the property name is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the specified property does not have a setter.</exception>
    public static Delegate GetPropertySetter(Type type, string propertyName)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException("Property name cannot be empty.", nameof(propertyName));
        var key = $"{type.FullName}.property.set.{propertyName}";
        return DelegateCache.GetOrAdd(key, _ =>
        {
            var property = GetProperty(type, propertyName);
            var setter = property.GetSetMethod(true);

            if (setter == null)
                throw new InvalidOperationException($"Property '{propertyName}' has no setter.");

            // static setter → PropertySetter<TProperty>
            if (setter.IsStatic)
            {
                var valueParam = Expression.Parameter(property.PropertyType, "value");
                var body = Expression.Assign(Expression.Property(null, property), valueParam);
                var delegateType = typeof(PropertySetter<>).MakeGenericType(property.PropertyType);
                var lambda = Expression.Lambda(delegateType, body, valueParam);
                return lambda.Compile();
            }

            // instance setter → PropertySetter<TInstance, TProperty>
            {
                var instanceParam = Expression.Parameter(type, "instance");
                var valueParam = Expression.Parameter(property.PropertyType, "value");
                var body = Expression.Assign(Expression.Property(instanceParam, property), valueParam);
                var delegateType = typeof(PropertySetter<,>).MakeGenericType(type, property.PropertyType);
                var lambda = Expression.Lambda(delegateType, body, instanceParam, valueParam);
                return lambda.Compile();
            }
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
    public static PropertySetter<TProperty> GetPropertySetter<TProperty>(Type type, string propertyName) =>
        (PropertySetter<TProperty>)GetPropertySetter(type, propertyName);

    /// <summary>
    ///     Retrieves a delegate that sets a property value on an instance of the specified type.
    /// </summary>
    /// <param name="propertyName">The name of the property to set.</param>
    /// <typeparam name="TInstance">The type of the instance on which the property will be set.</typeparam>
    /// <typeparam name="TProperty">The type of the property to be set.</typeparam>
    /// <returns>A delegate that sets the value of the specified property on the given instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="propertyName" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="propertyName" /> is an empty string.</exception>
    public static PropertySetter<TInstance, TProperty> GetPropertySetter<TInstance, TProperty>(string propertyName) =>
        (PropertySetter<TInstance, TProperty>)GetPropertySetter(typeof(TInstance), propertyName);

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