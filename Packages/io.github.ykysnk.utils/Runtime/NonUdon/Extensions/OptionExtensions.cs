using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon.Extensions;

[PublicAPI]
public static class OptionExtensions
{
    public static TResult Match<T, TResult>(this Option<T> option, Func<T, TResult> onSome, Func<TResult> onNone) =>
        option.HasValue ? onSome(option.Value!) : onNone();

    public static async Task<TResult> Match<T, TResult>(this Task<Option<T>> task, Func<T, Task<TResult>> onSome,
        Func<Task<TResult>> onNone)
    {
        var option = await task.ConfigureAwait(false);
        return option.HasValue ? await onSome(option.Value!).ConfigureAwait(false) : await onNone().ConfigureAwait(false);
    }

    public static Option<TU> Select<T, TU>(this Option<T> option, Func<T, TU> selector) =>
        option.HasValue ? Option<TU>.Some(selector(option.Value!)) : Option<TU>.None();

    public static async Task<Option<TU>> Select<T, TU>(
        this Task<Option<T>> task,
        Func<T, TU> selector)
    {
        var option = await task.ConfigureAwait(false);
        return option.HasValue
            ? Option<TU>.Some(selector(option.Value!))
            : Option<TU>.None();
    }

    public static Option<TU> SelectMany<T, TU>(this Option<T> option, Func<T, Option<TU>> binder) =>
        option.HasValue ? binder(option.Value!) : Option<TU>.None();

    public static Option<TV>
        SelectMany<T, TU, TV>(this Option<T> option, Func<T, Option<TU>> binder, Func<T, TU, TV> projector) =>
        option.HasValue
            ? binder(option.Value!).Select(u => projector(option.Value!, u))
            : Option<TV>.None();

    public static async Task<Option<TU>> SelectMany<T, TU>(
        this Task<Option<T>> task,
        Func<T, Task<Option<TU>>> binder)
    {
        var option = await task.ConfigureAwait(false);
        return option.HasValue
            ? await binder(option.Value!).ConfigureAwait(false)
            : Option<TU>.None();
    }

    public static async Task<Option<TV>> SelectMany<T, TU, TV>(
        this Task<Option<T>> task,
        Func<T, Task<Option<TU>>> binder,
        Func<T, TU, TV> projector)
    {
        var option = await task.ConfigureAwait(false);

        if (!option.HasValue)
            return Option<TV>.None();

        var inner = await binder(option.Value!).ConfigureAwait(false);

        return inner.HasValue
            ? Option<TV>.Some(projector(option.Value!, inner.Value!))
            : Option<TV>.None();
    }

    public static Result<T> ToResult<T>(this Option<T> option, Exception errorMessage) =>
        option.HasValue ? Result<T>.Success(option.Value!) : Result<T>.Failure(errorMessage);

    public static Result<T> ToResult<T>(this Option<T> option, string errorMessage) =>
        option.HasValue ? Result<T>.Success(option.Value!) : Result<T>.Failure(new(errorMessage));
}