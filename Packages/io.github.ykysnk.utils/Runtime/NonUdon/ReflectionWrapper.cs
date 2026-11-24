using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using io.github.ykysnk.utils.NonUdon.ReflectionDelegate;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

/// <summary>
///     Cached reflection lookups for fields, properties and methods, and compiled delegates built from them.
///     Lookups cover public and non-public instance and static members of the given type; private members that exist
///     only on a base type are not reached. A missing member throws instead of returning null.
/// </summary>
[PublicAPI]
public static class ReflectionWrapper
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                       BindingFlags.NonPublic;

    private static readonly ConcurrentDictionary<string, Type> TypeCache = new();
    private static readonly ConcurrentDictionary<(Type, string), FieldInfo> FieldCache = new();
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo> PropertyCache = new();
    private static readonly ConcurrentDictionary<string, MethodInfo> MethodCache = new();
    private static readonly ConcurrentDictionary<string, Delegate> DelegateCache = new();

    /// <summary>Resolves a type by name and caches it under that name.</summary>
    /// <param name="typeName">
    ///     Name accepted by <see cref="Type.GetType(string, bool)" />, for example a full or assembly-qualified name.
    /// </param>
    /// <returns>The resolved type; later calls with the same name return the cached instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeName" /> is null.</exception>
    /// <exception cref="TypeLoadException">Thrown when the name cannot be resolved.</exception>
    public static Type GetType(string typeName)
    {
        return typeName == null
            ? throw new ArgumentNullException(nameof(typeName))
            : TypeCache.GetOrAdd(typeName, name => Type.GetType(name, true));
    }

    /// <summary>Caches a type by its full name, so a later call with that name returns the cached type.</summary>
    /// <param name="type">Type to cache; null is rejected.</param>
    /// <returns>
    ///     The type already cached under <paramref name="type" />'s full name, otherwise <paramref name="type" /> itself.
    ///     A type without a full name (a generic type parameter) is returned unchanged and never cached.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
    public static Type GetType(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (type.FullName == null) return type;
        return TypeCache.GetOrAdd(type.FullName, _ => type);
    }

    /// <summary>Finds a field by name and caches it by type and name.</summary>
    /// <param name="type">Type to search; must not be null.</param>
    /// <param name="fieldName">Field name; must not be null or empty.</param>
    /// <returns>
    ///     The field, public or non-public, instance or static. A private field declared only on a base type is not
    ///     found.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName" /> is null or empty.</exception>
    /// <exception cref="MissingFieldException">Thrown when the type has no field with that name.</exception>
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

    /// <summary>Finds a property by name and caches it by type and name.</summary>
    /// <param name="type">Type to search; must not be null.</param>
    /// <param name="propertyName">Property name; must not be null or empty.</param>
    /// <returns>
    ///     The property, public or non-public, instance or static. A private property declared only on a base type is
    ///     not found.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName" /> is null or empty.</exception>
    /// <exception cref="MissingMemberException">Thrown when the type has no property with that name.</exception>
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

    /// <summary>Finds a method by name and exact parameter types and caches it.</summary>
    /// <param name="type">Type to search; must not be null.</param>
    /// <param name="methodName">Method name; must not be null or empty.</param>
    /// <param name="parameterTypes">
    ///     Exact parameter types, empty for a parameterless method. No conversion of arguments is applied.
    /// </param>
    /// <returns>The matching method, public or non-public, instance or static.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="methodName" /> is null or empty.</exception>
    /// <exception cref="MissingMethodException">Thrown when no overload matches the parameter types.</exception>
    public static MethodInfo GetMethod(Type type, string methodName, params Type[] parameterTypes)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(methodName))
            throw new ArgumentException("Method name cannot be empty.", nameof(methodName));
        return MethodCache.GetOrAdd(MethodKey(type, methodName, parameterTypes), _ =>
            type.GetMethod(methodName, Flags, null, parameterTypes, null) ??
            throw new MissingMethodException(type.FullName, methodName));
    }

    // The parameter types take part in the cache key structurally; keying on the Type[] instance would make every
    // call with a fresh array (which params creates) miss the cache.
    private static string MethodKey(Type type, string methodName, Type[] parameterTypes)
    {
        var builder = new StringBuilder(64);
        builder.Append(type.AssemblyQualifiedName).Append('|').Append(methodName).Append('|')
            .Append(parameterTypes.Length);

        foreach (var parameterType in parameterTypes)
            builder.Append('|').Append(parameterType?.AssemblyQualifiedName ?? "null");

        return builder.ToString();
    }

    /// <summary>
    ///     Compiles a delegate for a method with an expression tree and caches it. The cache key is the delegate type,
    ///     the declaring type, the method name and the parameter type names, so one method used with two delegate types
    ///     is compiled twice.
    /// </summary>
    /// <param name="type">Type declaring the method; must not be null.</param>
    /// <param name="methodName">Method name; must not be null or empty.</param>
    /// <param name="parameterTypes">Exact parameter types, empty for a parameterless method.</param>
    /// <typeparam name="TDelegate">
    ///     Delegate to build; its signature must match the method. For an instance method the first parameter receives
    ///     the instance and the remaining ones match the method's own parameters.
    /// </typeparam>
    /// <returns>The cached delegate instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="methodName" /> is null or empty.</exception>
    /// <exception cref="MissingMethodException">Thrown when no overload matches the parameter types.</exception>
    public static TDelegate GetDelegate<TDelegate>(Type type, string methodName, params Type[] parameterTypes)
        where TDelegate : Delegate
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(methodName))
            throw new ArgumentException("Method name cannot be empty.", nameof(methodName));
        var key = $"{typeof(TDelegate).AssemblyQualifiedName}|{MethodKey(type, methodName, parameterTypes)}";
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

    /// <summary>Builds and caches a getter delegate for a field.</summary>
    /// <param name="type">Type declaring the field; must not be null.</param>
    /// <param name="fieldName">Field name; must not be null or empty.</param>
    /// <returns>
    ///     <see cref="FieldGetter{TField}" /> for a static field, <see cref="FieldGetter{TInstance, TField}" /> for an
    ///     instance field; the generic overloads cast this result.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName" /> is null or empty.</exception>
    /// <exception cref="MissingFieldException">Thrown when the type has no field with that name.</exception>
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

    /// <summary>Returns the cached getter of a static field as <see cref="FieldGetter{TField}" />.</summary>
    /// <typeparam name="TField">Field type; must match the declared field type.</typeparam>
    /// <param name="type">Type declaring the static field; must not be null.</param>
    /// <param name="fieldName">Field name; must not be null or empty.</param>
    /// <returns>The getter delegate, which takes no arguments.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName" /> is null or empty.</exception>
    /// <exception cref="InvalidCastException">Thrown when the field is an instance field.</exception>
    /// <exception cref="MissingFieldException">Thrown when the type has no field with that name.</exception>
    public static FieldGetter<TField> GetFieldGetter<TField>(Type type, string fieldName) =>
        (FieldGetter<TField>)GetFieldGetter(type, fieldName);

    /// <summary>Returns the cached getter of an instance field as <see cref="FieldGetter{TInstance, TField}" />.</summary>
    /// <param name="fieldName">Field name; must not be null or empty.</param>
    /// <typeparam name="TInstance">Type searched for the field, normally the instance type.</typeparam>
    /// <typeparam name="TField">Field type; must match the declared field type.</typeparam>
    /// <returns>The getter delegate, which takes the instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName" /> is null or empty.</exception>
    /// <exception cref="InvalidCastException">Thrown when the field is a static field.</exception>
    /// <exception cref="MissingFieldException">Thrown when the type has no field with that name.</exception>
    public static FieldGetter<TInstance, TField> GetFieldGetter<TInstance, TField>(string fieldName) =>
        (FieldGetter<TInstance, TField>)GetFieldGetter(typeof(TInstance), fieldName);

    /// <summary>Builds and caches a setter delegate for a field.</summary>
    /// <param name="type">Type declaring the field; must not be null.</param>
    /// <param name="fieldName">Field name; must not be null or empty.</param>
    /// <returns>
    ///     <see cref="FieldSetter{TField}" /> for a static field, <see cref="FieldSetter{TInstance, TField}" /> for an
    ///     instance field; the generic overloads cast this result.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName" /> is null or empty.</exception>
    /// <exception cref="MissingFieldException">Thrown when the type has no field with that name.</exception>
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

    /// <summary>Returns the cached setter of a static field as <see cref="FieldSetter{TField}" />.</summary>
    /// <param name="type">Type declaring the static field; must not be null.</param>
    /// <param name="fieldName">Field name; must not be null or empty.</param>
    /// <typeparam name="TField">Field type; must match the declared field type.</typeparam>
    /// <returns>The setter delegate, which takes the new value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName" /> is null or empty.</exception>
    /// <exception cref="InvalidCastException">Thrown when the field is an instance field.</exception>
    /// <exception cref="MissingFieldException">Thrown when the type has no field with that name.</exception>
    public static FieldSetter<TField> GetFieldSetter<TField>(Type type, string fieldName) =>
        (FieldSetter<TField>)GetFieldSetter(type, fieldName);

    /// <summary>Returns the cached setter of an instance field as <see cref="FieldSetter{TInstance, TField}" />.</summary>
    /// <param name="fieldName">Field name; must not be null or empty.</param>
    /// <typeparam name="TInstance">Type searched for the field, normally the instance type.</typeparam>
    /// <typeparam name="TField">Field type; must match the declared field type.</typeparam>
    /// <returns>The setter delegate, which takes the instance and the new value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName" /> is null or empty.</exception>
    /// <exception cref="InvalidCastException">Thrown when the field is a static field.</exception>
    /// <exception cref="MissingFieldException">Thrown when the type has no field with that name.</exception>
    public static FieldSetter<TInstance, TField> GetFieldSetter<TInstance, TField>(string fieldName) =>
        (FieldSetter<TInstance, TField>)GetFieldSetter(typeof(TInstance), fieldName);

    /// <summary>Builds and caches a getter delegate for a property.</summary>
    /// <param name="type">Type declaring the property; must not be null.</param>
    /// <param name="propertyName">Property name; must not be null or empty.</param>
    /// <returns>
    ///     <see cref="PropertyGetter{TProperty}" /> for a static property,
    ///     <see cref="PropertyGetter{TInstance, TProperty}" /> for an instance property; the generic overloads cast this
    ///     result.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName" /> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the property has no getter.</exception>
    /// <exception cref="MissingMemberException">Thrown when the type has no property with that name.</exception>
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

    /// <summary>Returns the cached getter of a static property as <see cref="PropertyGetter{TProperty}" />.</summary>
    /// <param name="type">Type declaring the static property; must not be null.</param>
    /// <param name="propertyName">Property name; must not be null or empty.</param>
    /// <typeparam name="TProperty">Property type; must match the declared property type.</typeparam>
    /// <returns>The getter delegate, which takes no arguments.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName" /> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the property has no getter.</exception>
    /// <exception cref="InvalidCastException">Thrown when the property is an instance property.</exception>
    /// <exception cref="MissingMemberException">Thrown when the type has no property with that name.</exception>
    public static PropertyGetter<TProperty> GetPropertyGetter<TProperty>(Type type, string propertyName) =>
        (PropertyGetter<TProperty>)GetPropertyGetter(type, propertyName);

    /// <summary>Returns the cached getter of an instance property as <see cref="PropertyGetter{TInstance, TProperty}" />.</summary>
    /// <param name="propertyName">Property name; must not be null or empty.</param>
    /// <typeparam name="TInstance">Type searched for the property, normally the instance type.</typeparam>
    /// <typeparam name="TProperty">Property type; must match the declared property type.</typeparam>
    /// <returns>The getter delegate, which takes the instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName" /> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the property has no getter.</exception>
    /// <exception cref="InvalidCastException">Thrown when the property is a static property.</exception>
    /// <exception cref="MissingMemberException">Thrown when the type has no property with that name.</exception>
    public static PropertyGetter<TInstance, TProperty> GetPropertyGetter<TInstance, TProperty>(string propertyName) =>
        (PropertyGetter<TInstance, TProperty>)GetPropertyGetter(typeof(TInstance), propertyName);

    /// <summary>Builds and caches a setter delegate for a property.</summary>
    /// <param name="type">Type declaring the property; must not be null.</param>
    /// <param name="propertyName">Property name; must not be null or empty.</param>
    /// <returns>
    ///     <see cref="PropertySetter{TProperty}" /> for a static property,
    ///     <see cref="PropertySetter{TInstance, TProperty}" /> for an instance property; the generic overloads cast this
    ///     result.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName" /> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the property has no setter.</exception>
    /// <exception cref="MissingMemberException">Thrown when the type has no property with that name.</exception>
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

    /// <summary>Returns the cached setter of a static property as <see cref="PropertySetter{TProperty}" />.</summary>
    /// <typeparam name="TProperty">Property type; must match the declared property type.</typeparam>
    /// <param name="type">Type declaring the static property; must not be null.</param>
    /// <param name="propertyName">Property name; must not be null or empty.</param>
    /// <returns>The setter delegate, which takes the new value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName" /> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the property has no setter.</exception>
    /// <exception cref="InvalidCastException">Thrown when the property is an instance property.</exception>
    /// <exception cref="MissingMemberException">Thrown when the type has no property with that name.</exception>
    public static PropertySetter<TProperty> GetPropertySetter<TProperty>(Type type, string propertyName) =>
        (PropertySetter<TProperty>)GetPropertySetter(type, propertyName);

    /// <summary>Returns the cached setter of an instance property as <see cref="PropertySetter{TInstance, TProperty}" />.</summary>
    /// <param name="propertyName">Property name; must not be null or empty.</param>
    /// <typeparam name="TInstance">Type searched for the property, normally the instance type.</typeparam>
    /// <typeparam name="TProperty">Property type; must match the declared property type.</typeparam>
    /// <returns>The setter delegate, which takes the instance and the new value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName" /> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the property has no setter.</exception>
    /// <exception cref="InvalidCastException">Thrown when the property is a static property.</exception>
    /// <exception cref="MissingMemberException">Thrown when the type has no property with that name.</exception>
    public static PropertySetter<TInstance, TProperty> GetPropertySetter<TInstance, TProperty>(string propertyName) =>
        (PropertySetter<TInstance, TProperty>)GetPropertySetter(typeof(TInstance), propertyName);

    // Typed wrappers over GetDelegate for the common arities, for static methods only: every type argument is a method
    // parameter type and GetFunc takes the return type last. For an instance method call GetDelegate directly and pass
    // the declaring type as the delegate's first parameter. All of them share the delegate cache.
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