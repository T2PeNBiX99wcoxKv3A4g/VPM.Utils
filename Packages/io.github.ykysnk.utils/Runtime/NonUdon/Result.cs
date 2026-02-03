using System;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

[PublicAPI]
public readonly struct Result<T>
{
    public T? Value { get; }
    public Exception? Exception { get; }
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

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Exception exception) => new(exception);

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Err(Exception exception) => new(exception);

    public void Deconstruct(out bool isSuccess, out T? value, out Exception? exception)
    {
        isSuccess = IsSuccess;
        value = Value;
        exception = Exception;
    }

    public static bool operator true(Result<T> r) => r.IsSuccess;
    public static bool operator false(Result<T> r) => !r.IsSuccess;

    public override string ToString() => IsSuccess ? $"Ok({Value})" : $"Err({Exception})";
}