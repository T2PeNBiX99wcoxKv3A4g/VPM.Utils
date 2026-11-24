using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon.Extensions;

[PublicAPI]
public static class ResultExtensions
{
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
            action(result.Value!);
        return result;
    }

    public static Result<T> OnFailure<T>(this Result<T> result, Action<Exception> action)
    {
        if (!result.IsSuccess)
            action(result.Exception!);
        return result;
    }

    public static Result<T> OnFinally<T>(this Result<T> result, Action action)
    {
        action();
        return result;
    }

    public static async Task<Result<T>> OnSuccess<T>(
        this Task<Result<T>> task,
        Func<T, Task> action)
    {
        var result = await task.ConfigureAwait(false);

        if (result.IsSuccess)
            await action(result.Value!).ConfigureAwait(false);

        return result;
    }

    public static async Task<Result<T>> OnFailure<T>(
        this Task<Result<T>> task,
        Func<Exception, Task> action)
    {
        var result = await task.ConfigureAwait(false);

        if (!result.IsSuccess)
            await action(result.Exception!).ConfigureAwait(false);

        return result;
    }

    public static async Task<Result<T>> OnFinally<T>(this Task<Result<T>> task, Func<Task> action)
    {
        var result = await task.ConfigureAwait(false);
        await action().ConfigureAwait(false);
        return result;
    }

    public static TResult Match<T, TResult>(this Result<T> result, Func<T, TResult> onSuccess,
        Func<Exception, TResult> onFailure) => result.IsSuccess ? onSuccess(result.Value!) : onFailure(result.Exception!);

    public static async Task<TResult> Match<T, TResult>(
        this Task<Result<T>> task,
        Func<T, Task<TResult>> onSuccess,
        Func<Exception, Task<TResult>> onFailure)
    {
        var result = await task.ConfigureAwait(false);

        return result.IsSuccess
            ? await onSuccess(result.Value!).ConfigureAwait(false)
            : await onFailure(result.Exception!).ConfigureAwait(false);
    }

    public static T? GetOrNull<T>(this Result<T> result) => result.IsSuccess ? result.Value : default;

    /// <summary>
    ///     The value, and whether the call succeeded. <see cref="GetOrNull{T}" /> cannot answer this for a
    ///     value type — <c>T?</c> on an unconstrained parameter stays <c>T</c>, so a failed
    ///     <c>Result&lt;int&gt;</c> would come back as <c>0</c> and look like a real value.
    /// </summary>
    public static bool TryGet<T>(this Result<T> result, [MaybeNullWhen(false)] out T value)
    {
        value = result.IsSuccess ? result.Value! : default!;
        return result.IsSuccess;
    }

    public static async Task<T?> GetOrNull<T>(this Task<Result<T>> task)
    {
        var result = await task.ConfigureAwait(false);
        return result.IsSuccess ? result.Value : default;
    }

    /// <summary>
    ///     Whether the call succeeded, and its value — the async form of <see cref="TryGet{T}" />, which
    ///     cannot use an <c>out</c> parameter across an <c>await</c>.
    /// </summary>
    public static async Task<(bool Success, T? Value)> TryGet<T>(this Task<Result<T>> task)
    {
        var result = await task.ConfigureAwait(false);
        return (result.IsSuccess, result.IsSuccess ? result.Value : default);
    }

    public static T GetOrElse<T>(this Result<T> result, Func<Exception, T> onFailure) =>
        result.IsSuccess ? result.Value! : onFailure(result.Exception!);

    public static T GetOrThrow<T>(this Result<T> result)
    {
        if (result.IsSuccess) return result.Value!;
        result.ThrowOnFailure();
        return default!;
    }

    private static void ThrowOnFailure<T>(this Result<T> result)
    {
        if (result.Exception is { } exception)
            ExceptionDispatchInfo.Capture(exception).Throw();
    }

    public static async Task<T> GetOrElse<T>(
        this Task<Result<T>> task,
        Func<Exception, Task<T>> onFailure)
    {
        var result = await task.ConfigureAwait(false);
        return result.IsSuccess ? result.Value! : await onFailure(result.Exception!).ConfigureAwait(false);
    }

    public static async Task<T> GetOrThrow<T>(this Task<Result<T>> task)
    {
        var result = await task.ConfigureAwait(false);
        if (result.IsSuccess) return result.Value!;
        result.ThrowOnFailure();
        return default!;
    }

    public static Result<TU> Select<T, TU>(this Result<T> result, Func<T, TU> selector) => result.IsSuccess
        ? Result<TU>.Success(selector(result.Value!))
        : Result<TU>.Failure(result.Exception!);

    public static async Task<Result<TU>> Select<T, TU>(
        this Task<Result<T>> task,
        Func<T, TU> selector)
    {
        var result = await task.ConfigureAwait(false);
        return result.IsSuccess
            ? Result<TU>.Success(selector(result.Value!))
            : Result<TU>.Failure(result.Exception!);
    }

    public static Result<TU> SelectMany<T, TU>(this Result<T> result, Func<T, Result<TU>> binder) =>
        result.IsSuccess ? binder(result.Value!) : Result<TU>.Failure(result.Exception!);

    public static Result<TV>
        SelectMany<T, TU, TV>(this Result<T> result, Func<T, Result<TU>> binder, Func<T, TU, TV> projector) =>
        result.IsSuccess
            ? binder(result.Value!).Select(u => projector(result.Value!, u))
            : Result<TV>.Failure(result.Exception!);

    public static async Task<Result<TU>> SelectMany<T, TU>(
        this Task<Result<T>> task,
        Func<T, Task<Result<TU>>> binder)
    {
        var result = await task.ConfigureAwait(false);
        return result.IsSuccess
            ? await binder(result.Value!).ConfigureAwait(false)
            : Result<TU>.Failure(result.Exception!);
    }

    public static async Task<Result<TV>> SelectMany<T, TU, TV>(
        this Task<Result<T>> task,
        Func<T, Task<Result<TU>>> binder,
        Func<T, TU, TV> projector)
    {
        var result = await task.ConfigureAwait(false);

        if (!result.IsSuccess)
            return Result<TV>.Failure(result.Exception!);

        var inner = await binder(result.Value!).ConfigureAwait(false);

        return inner.IsSuccess
            ? Result<TV>.Success(projector(result.Value!, inner.Value!))
            : Result<TV>.Failure(inner.Exception!);
    }

    public static async Task<Result<TV>> SelectMany<T, TU, TV>(
        this Task<Result<T>> task,
        Func<T, Task<Result<TU>>> binder,
        Func<T, TU, Task<TV>> projector)
    {
        var result = await task.ConfigureAwait(false);

        if (!result.IsSuccess)
            return Result<TV>.Failure(result.Exception!);

        var inner = await binder(result.Value!).ConfigureAwait(false);

        return inner.IsSuccess
            ? Result<TV>.Success(await projector(result.Value!, inner.Value!).ConfigureAwait(false))
            : Result<TV>.Failure(inner.Exception!);
    }
}