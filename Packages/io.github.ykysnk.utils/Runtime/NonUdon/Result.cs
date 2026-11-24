using System;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

/// <summary>
///     Outcome of an operation that can fail: either a value or the exception that stopped it. The success flag is
///     explicit rather than inferred from the value, so <c>Result&lt;int&gt;</c> can carry a genuine 0 and still
///     report success.
/// </summary>
[PublicAPI]
public readonly struct Result<T>
{
    /// <summary>The value produced by the call when it succeeded.</summary>
    /// <value>
    ///     Whatever the delegate returned; on failure this is the default of <typeparamref name="T" />. A null value
    ///     on a successful result is allowed and is not an error.
    /// </value>
    public T? Value { get; }
    /// <summary>The exception that stopped the call.</summary>
    /// <value>The captured exception with its original stack trace, or <see langword="null" /> on success.</value>
    public Exception? Exception { get; }
    /// <summary>Whether the call completed without throwing.</summary>
    /// <value><see langword="true" /> when <see cref="Exception" /> is null.</value>
    public bool IsSuccess => Exception is null;

    private Result(T value)
    {
        Value = value;
        Exception = null;
    }

    private Result(Exception exception)
    {
        Value = default;
        Exception = exception;
    }

    /// <summary>Builds a successful result.</summary>
    /// <param name="value">The value to carry; null is allowed and still counts as success.</param>
    /// <returns>A result with <see cref="IsSuccess" /> set and <see cref="Exception" /> cleared.</returns>
    public static Result<T> Success(T value) => new(value);
    /// <summary>Builds a failed result.</summary>
    /// <param name="exception">The failure to carry; it is stored as-is, not unwrapped or rethrown.</param>
    /// <returns>A result with <see cref="IsSuccess" /> cleared and <see cref="Value" /> set to the default of <typeparamref name="T" />.</returns>
    public static Result<T> Failure(Exception exception) => new(exception);

    /// <summary>Splits the result into its three readable parts, which is what <c>var (success, value, error) = …</c> calls.</summary>
    /// <param name="isSuccess">Receives <see cref="IsSuccess" />.</param>
    /// <param name="value">Receives <see cref="Value" />, which is the default of <typeparamref name="T" /> on failure.</param>
    /// <param name="exception">Receives <see cref="Exception" />, which is <see langword="null" /> on success.</param>
    public void Deconstruct(out bool isSuccess, out T? value, out Exception? exception)
    {
        isSuccess = IsSuccess;
        value = Value;
        exception = Exception;
    }

    /// <summary>Reports success, so a result can be used directly in a conditional.</summary>
    /// <param name="r">The result to test.</param>
    /// <returns><see langword="true" /> when the call succeeded.</returns>
    public static bool operator true(Result<T> r) => r.IsSuccess;
    /// <summary>Reports failure. The compiler requires this alongside the <c>true</c> operator for conditional use.</summary>
    /// <param name="r">The result to test.</param>
    /// <returns><see langword="true" /> when the call failed.</returns>
    public static bool operator false(Result<T> r) => !r.IsSuccess;

    /// <summary>Renders the outcome as <c>Success(value)</c> or <c>Failure(exception)</c>.</summary>
    /// <returns>Text that includes the value or the exception's own <c>ToString()</c>.</returns>
    public override string ToString() => IsSuccess ? $"Success({Value})" : $"Failure({Exception})";
}