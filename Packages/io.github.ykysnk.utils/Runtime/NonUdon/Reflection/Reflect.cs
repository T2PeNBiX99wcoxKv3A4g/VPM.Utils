using System;
using System.Reflection;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon.Reflection
{
    /// <summary>
    ///     Cached reflection access modelled after HarmonyLib's <c>Traverse</c> but without Harmony and without
    ///     resolving members on every access.
    ///     <para>
    ///         A member is resolved once when the accessor is created and its accessor is compiled once, so a
    ///         reused accessor is a delegate call instead of <c>FieldInfo.GetValue</c>.
    ///     </para>
    ///     <code>
    /// var health = Reflect.For(component).Field("health");
    /// var value = health.Get&lt;int&gt;();      // delegate call, safe to reuse in a loop
    /// health.Set(3);
    ///
    /// var name = Reflect.For(type).Method("BuildName", 1, "x").Invoke&lt;string&gt;();
    /// </code>
    /// </summary>
    [PublicAPI]
    public static class Reflect
    {
        /// <summary>Creates an accessor for an instance. Members are looked up on its runtime type.</summary>
        public static ReflectionAccess For([NotNull] object target) => new(target, target.GetType());

        /// <summary>Creates an accessor for static members of a type.</summary>
        public static ReflectionAccess For([NotNull] Type type) => new(null, type);
    }

    /// <summary>An instance (or type, for static members) that members can be read from.</summary>
    [PublicAPI]
    public readonly struct ReflectionAccess
    {
        internal ReflectionAccess(object? target, Type type)
        {
            Target = target;
            Type = type;
        }

        /// <summary>The instance the accessor reads from, or <c>null</c> for static access.</summary>
        [CanBeNull]
        public object? Target { get; }

        /// <summary>The type members are looked up on.</summary>
        [NotNull]
        public Type Type { get; }

        public FieldAccess Field([NotNull] string name) => new(Target, Type, name);

        public PropertyAccess Property([NotNull] string name) => new(Target, Type, name);

        public MethodAccess Method([NotNull] string name, [CanBeNull] params object?[] arguments) =>
            new(Target, Type, name, arguments ?? EmptyArguments);

        private static readonly object?[] EmptyArguments = Array.Empty<object?>();
    }

    /// <summary>A field of the target type.</summary>
    [PublicAPI]
    public readonly struct FieldAccess
    {
        private readonly object? _target;
        private readonly Type _type;
        private readonly string _name;
        private readonly FieldInfo? _field;
        private readonly Func<object, object?>? _getter;
        private readonly Action<object, object?>? _setter;

        internal FieldAccess(object? target, Type type, string name)
        {
            _target = target;
            _type = type;
            _name = name;
            _field = MemberAccessors.FindField(type, name);
            _getter = _field == null ? null : MemberAccessors.GetFieldGetter(_field);
            _setter = _field == null ? null : MemberAccessors.GetFieldSetter(_field);
        }

        public bool Exists => _field != null;

        /// <summary>Whether the field can be assigned (false for readonly and const fields).</summary>
        public bool CanWrite => _setter != null;

        [CanBeNull]
        public Type? MemberType => _field?.FieldType;

        /// <summary>
        ///     Reads the field. Throws <see cref="MissingFieldException" /> when it does not exist and
        ///     <see cref="MemberAccessException" /> when it cannot be read.
        /// </summary>
        public T? Get<T>()
        {
            if (_getter == null) throw MemberAccessors.MissingField(_type, _name);
            var value = _getter(_target!);
            return value is null ? default : (T)value;
        }

        /// <summary>Reads the field into <paramref name="value" /> when it exists.</summary>
        public bool TryGet<T>([CanBeNull] out T? value)
        {
            if (_getter == null)
            {
                value = default;
                return false;
            }

            var result = _getter(_target!);
            value = result is null ? default : (T)result;
            return true;
        }

        /// <summary>Writes the field. Returns false when it does not exist or cannot be written.</summary>
        public bool Set([CanBeNull] object? value)
        {
            if (_setter == null) return false;
            _setter(_target!, value);
            return true;
        }
    }

    /// <summary>A property of the target type.</summary>
    [PublicAPI]
    public readonly struct PropertyAccess
    {
        private readonly object? _target;
        private readonly Type _type;
        private readonly string _name;
        private readonly PropertyInfo? _property;
        private readonly Func<object, object?>? _getter;
        private readonly Action<object, object?>? _setter;

        internal PropertyAccess(object? target, Type type, string name)
        {
            _target = target;
            _type = type;
            _name = name;
            _property = MemberAccessors.FindProperty(type, name);
            _getter = _property == null ? null : MemberAccessors.GetPropertyGetter(_property);
            _setter = _property == null ? null : MemberAccessors.GetPropertySetter(_property);
        }

        public bool Exists => _property != null;

        /// <summary>Whether the property can be assigned (false when it has no setter).</summary>
        public bool CanWrite => _setter != null;

        [CanBeNull]
        public Type? MemberType => _property?.PropertyType;

        /// <summary>
        ///     Reads the property. Throws <see cref="MissingMemberException" /> when it does not exist and
        ///     <see cref="MemberAccessException" /> when it has no getter.
        /// </summary>
        public T? Get<T>()
        {
            if (_getter == null) throw MemberAccessors.MissingProperty(_type, _name);
            var value = _getter(_target!);
            return value is null ? default : (T)value;
        }

        /// <summary>Reads the property into <paramref name="value" /> when it exists and can be read.</summary>
        public bool TryGet<T>([CanBeNull] out T? value)
        {
            if (_getter == null)
            {
                value = default;
                return false;
            }

            var result = _getter(_target!);
            value = result is null ? default : (T)result;
            return true;
        }

        /// <summary>Writes the property. Returns false when it does not exist or cannot be written.</summary>
        public bool Set([CanBeNull] object? value)
        {
            if (_setter == null) return false;
            _setter(_target!, value);
            return true;
        }
    }

    /// <summary>A method of the target type, with the arguments it will be called with.</summary>
    [PublicAPI]
    public readonly struct MethodAccess
    {
        private readonly object? _target;
        private readonly Type _type;
        private readonly string _name;
        private readonly object?[] _arguments;
        private readonly MethodInfo? _method;
        private readonly Func<object, object?[], object?>? _invoker;

        internal MethodAccess(object? target, Type type, string name, object?[] arguments)
        {
            _target = target;
            _type = type;
            _name = name;
            _arguments = arguments;
            _method = MemberAccessors.FindMethod(type, name, arguments, target == null);
            _invoker = _method == null ? null : MemberAccessors.GetMethodInvoker(_method);
        }

        public bool Exists => _method != null;

        [CanBeNull]
        public Type? ReturnType => _method?.ReturnType;

        public bool TryInvoke([CanBeNull] out object? result)
        {
            if (_invoker == null)
            {
                result = null;
                return false;
            }

            result = _invoker(_target, _arguments);
            return true;
        }

        public bool TryInvoke<T>([CanBeNull] out T? result)
        {
            if (!TryInvoke(out var value))
            {
                result = default;
                return false;
            }

            result = value is null ? default : (T)value;
            return true;
        }

        /// <summary>
        ///     Calls the method. Throws <see cref="MissingMethodException" /> when no overload matches the arguments.
        /// </summary>
        public object? Invoke()
        {
            if (_invoker == null)
                throw MemberAccessors.MissingMethod(_type, _name, _arguments);
            return _invoker(_target, _arguments);
        }

        /// <inheritdoc cref="Invoke" />
        public T? Invoke<T>()
        {
            var value = Invoke();
            return value is null ? default : (T)value;
        }
    }
}
