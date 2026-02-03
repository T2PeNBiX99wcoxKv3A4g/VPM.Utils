using System;
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

    public static async Task<Result<T>> OnSuccessAsync<T>(
        this Task<Result<T>> task,
        Func<T, Task> action)
    {
        var result = await task.ConfigureAwait(false);

        if (result.IsSuccess)
            await action(result.Value!).ConfigureAwait(false);

        return result;
    }

    public static async Task<Result<T>> OnFailureAsync<T>(
        this Task<Result<T>> task,
        Func<Exception, Task> action)
    {
        var result = await task.ConfigureAwait(false);

        if (!result.IsSuccess)
            await action(result.Exception!).ConfigureAwait(false);

        return result;
    }

    public static Result<TU> Select<T, TU>(this Result<T> result, Func<T, TU> selector) => result.IsSuccess
        ? Result<TU>.Success(selector(result.Value!))
        : Result<TU>.Failure(result.Exception!);

    public static Result<TU> SelectMany<T, TU>(this Result<T> result, Func<T, Result<TU>> binder) =>
        result.IsSuccess ? binder(result.Value!) : Result<TU>.Failure(result.Exception!);

    public static Result<TV>
        SelectMany<T, TU, TV>(this Result<T> result, Func<T, Result<TU>> binder, Func<T, TU, TV> projector) =>
        result.IsSuccess
            ? binder(result.Value!).Select(u => projector(result.Value!, u))
            : Result<TV>.Failure(result.Exception!);

    public static async Task<Result<TU>> Select<T, TU>(
        this Task<Result<T>> task,
        Func<T, TU> selector)
    {
        var result = await task.ConfigureAwait(false);
        return result.IsSuccess
            ? Result<TU>.Success(selector(result.Value!))
            : Result<TU>.Failure(result.Exception!);
    }

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
}