using System;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

[PublicAPI]
public readonly struct Option<T>
{
    public T? Value { get; }
    public bool HasValue { get; }

    private Option(T value)
    {
        Value = value;
        HasValue = true;
    }

    private Option(bool _)
    {
        Value = default;
        HasValue = false;
    }

    public static Option<T> Some(T value) => new(value);
    public static Option<T> None() => new(false);

    public TResult Match<TResult>(
        Func<T, TResult> onSome,
        Func<TResult> onNone) =>
        HasValue ? onSome(Value!) : onNone();

    public void Deconstruct(out bool hasValue, out T? value)
    {
        hasValue = HasValue;
        value = Value;
    }

    public override string ToString() => HasValue ? $"Some({Value})" : "None";
}