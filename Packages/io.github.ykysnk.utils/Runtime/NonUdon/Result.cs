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

    public T? GetOrNull() => IsSuccess ? Value : default;

    public T GetOrElse(T fallback) => IsSuccess ? Value! : fallback;

    public Result<T> OnFailure(Action<Exception> action)
    {
        if (!IsSuccess)
            action(Exception!);
        return this;
    }

    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
            action(Value!);
        return this;
    }

    public void Deconstruct(out bool isSuccess, out T? value, out Exception? exception)
    {
        isSuccess = IsSuccess;
        value = Value;
        exception = Exception;
    }

    public static bool operator true(Result<T> r) => r.IsSuccess;
    public static bool operator false(Result<T> r) => !r.IsSuccess;
}

[PublicAPI]
public readonly struct Result<T, TE>
{
    public T? Value { get; }
    public TE? Error { get; }
    public bool IsSuccess { get; }

    private Result(T value)
    {
        Value = value;
        Error = default;
        IsSuccess = true;
    }

    private Result(TE error)
    {
        Value = default;
        Error = error;
        IsSuccess = false;
    }

    public static Result<T, TE> Ok(T value) => new(value);
    public static Result<T, TE> Err(TE error) => new(error);

    public void Deconstruct(out bool isSuccess, out T? value, out TE? error)
    {
        isSuccess = IsSuccess;
        value = Value;
        error = Error;
    }

    public static bool operator true(Result<T, TE> r) => r.IsSuccess;
    public static bool operator false(Result<T, TE> r) => !r.IsSuccess;

    public override string ToString() =>
        IsSuccess ? $"Ok({Value})" : $"Err({Error})";
}