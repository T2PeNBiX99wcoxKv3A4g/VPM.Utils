using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon.Extensions;

/// <summary>
///     Chainable operations on <see cref="Result{T}" />, plus the same set again for a <see cref="Task{TResult}" />
///     of a result. When a call short-circuits, it carries the original exception through rather than replacing it.
/// </summary>
[PublicAPI]
public static class ResultExtensions
{
    /// <summary>Runs <paramref name="action" /> with the value when the call succeeded.</summary>
    /// <param name="result">The result to inspect; a failure leaves <paramref name="action" /> unexecuted.</param>
    /// <param name="action">The callback that receives the value; exceptions it throws are not captured.</param>
    /// <returns>The same result, so further calls can be chained.</returns>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
            action(result.Value!);
        return result;
    }

    /// <summary>Runs <paramref name="action" /> with the exception when the call failed.</summary>
    /// <param name="result">The result to inspect; a success leaves <paramref name="action" /> unexecuted.</param>
    /// <param name="action">The callback that receives the exception; it does not consume or clear it.</param>
    /// <returns>The same result.</returns>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<Exception> action)
    {
        if (!result.IsSuccess)
            action(result.Exception!);
        return result;
    }

    /// <summary>
    ///     Runs <paramref name="action" /> whatever the outcome, the way a <c>finally</c> block would. It is not tied
    ///     to the failure path; use <see cref="OnFailure{T}(Result{T}, Action{Exception})" /> for that.
    /// </summary>
    /// <param name="result">The result being chained, which is passed through unchanged.</param>
    /// <param name="action">The cleanup callback; it receives no value or exception.</param>
    /// <returns>The same result.</returns>
    public static Result<T> OnFinally<T>(this Result<T> result, Action action)
    {
        action();
        return result;
    }

    /// <summary>Awaits the result, then runs <paramref name="action" /> with the value when the call succeeded.</summary>
    /// <param name="task">The pending operation; it is awaited once.</param>
    /// <param name="action">The asynchronous callback that receives the value; it is awaited before returning.</param>
    /// <returns>A task that yields the same result, after the callback has completed.</returns>
    public static async Task<Result<T>> OnSuccess<T>(
        this Task<Result<T>> task,
        Func<T, Task> action)
    {
        var result = await task.ConfigureAwait(false);

        if (result.IsSuccess)
            await action(result.Value!).ConfigureAwait(false);

        return result;
    }

    /// <summary>Awaits the result, then runs <paramref name="action" /> with the exception when the call failed.</summary>
    /// <param name="task">The pending operation; it is awaited once.</param>
    /// <param name="action">The asynchronous callback that receives the exception; it is awaited before returning.</param>
    /// <returns>A task that yields the same result, after the callback has completed.</returns>
    public static async Task<Result<T>> OnFailure<T>(
        this Task<Result<T>> task,
        Func<Exception, Task> action)
    {
        var result = await task.ConfigureAwait(false);

        if (!result.IsSuccess)
            await action(result.Exception!).ConfigureAwait(false);

        return result;
    }

    /// <summary>Awaits the result, then runs <paramref name="action" /> whatever the outcome.</summary>
    /// <param name="task">The pending operation; it is awaited once.</param>
    /// <param name="action">The asynchronous cleanup callback; it receives no value or exception.</param>
    /// <returns>A task that yields the same result, after the callback has completed.</returns>
    public static async Task<Result<T>> OnFinally<T>(this Task<Result<T>> task, Func<Task> action)
    {
        var result = await task.ConfigureAwait(false);
        await action().ConfigureAwait(false);
        return result;
    }

    /// <summary>Collapses the outcome into one value by running exactly one of the two callbacks.</summary>
    /// <param name="result">The result to inspect.</param>
    /// <param name="onSuccess">Produces the value from the call's value.</param>
    /// <param name="onFailure">Produces the value from the call's exception.</param>
    /// <returns>Whatever the chosen callback returned.</returns>
    public static TResult Match<T, TResult>(this Result<T> result, Func<T, TResult> onSuccess,
        Func<Exception, TResult> onFailure) => result.IsSuccess ? onSuccess(result.Value!) : onFailure(result.Exception!);

    /// <summary>Awaits the result and collapses the outcome by awaiting exactly one of the two callbacks.</summary>
    /// <param name="task">The pending operation; it is awaited once.</param>
    /// <param name="onSuccess">Produces the value from the call's value.</param>
    /// <param name="onFailure">Produces the value from the call's exception.</param>
    /// <returns>A task that yields whatever the chosen callback produced.</returns>
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

    /// <summary>The value on success, otherwise the default of <typeparamref name="T" />.</summary>
    /// <param name="result">The result to read; no exception is thrown or logged.</param>
    /// <returns>
    ///     The value, or <c>default</c>. For a value type <c>T</c> that default is indistinguishable from a real
    ///     value, so use <see cref="TryGet{T}(Result{T}, out T)" /> when 0 or <c>false</c> are plausible results.
    /// </returns>
    public static T? GetOrNull<T>(this Result<T> result) => result.IsSuccess ? result.Value : default;

    /// <summary>
    ///     Whether the call succeeded, and its value. Unlike <see cref="GetOrNull{T}(Result{T})" /> this can report
    ///     failure for a value type: <c>T?</c> on an unconstrained parameter stays <c>T</c>, so a failed
    ///     <c>Result&lt;int&gt;</c> would otherwise come back as <c>0</c> and look like a real value.
    /// </summary>
    /// <param name="result">The result to read.</param>
    /// <param name="value">The value on success, otherwise the default of <typeparamref name="T" />.</param>
    /// <returns>Whether the result succeeded.</returns>
    public static bool TryGet<T>(this Result<T> result, [MaybeNullWhen(false)] out T value)
    {
        value = result.IsSuccess ? result.Value! : default!;
        return result.IsSuccess;
    }

    /// <summary>Awaits the result, then returns its value or the default of <typeparamref name="T" />.</summary>
    /// <param name="task">The pending operation; it is awaited once.</param>
    /// <returns>The value on success, otherwise <c>default</c>.</returns>
    public static async Task<T?> GetOrNull<T>(this Task<Result<T>> task)
    {
        var result = await task.ConfigureAwait(false);
        return result.IsSuccess ? result.Value : default;
    }

    /// <summary>
    ///     Whether the call succeeded, and its value — the async form of
    ///     <see cref="TryGet{T}(Result{T}, out T)" />, which cannot use an <c>out</c> parameter across an
    ///     <c>await</c>.
    /// </summary>
    /// <param name="task">The pending operation; it is awaited once.</param>
    /// <returns>Whether the result succeeded, together with its value or the default of <typeparamref name="T" />.</returns>
    public static async Task<(bool Success, T? Value)> TryGet<T>(this Task<Result<T>> task)
    {
        var result = await task.ConfigureAwait(false);
        return (result.IsSuccess, result.IsSuccess ? result.Value : default);
    }

    /// <summary>The value on success, otherwise a value derived from the exception.</summary>
    /// <param name="result">The result to read.</param>
    /// <param name="onFailure">Builds the fallback from the exception; it runs only on failure.</param>
    /// <returns>The value, or whatever <paramref name="onFailure" /> returned.</returns>
    public static T GetOrElse<T>(this Result<T> result, Func<Exception, T> onFailure) =>
        result.IsSuccess ? result.Value! : onFailure(result.Exception!);

    /// <summary>The value on success; on failure the stored exception is rethrown with its original stack trace.</summary>
    /// <param name="result">The result to read; a success makes this call a plain value read.</param>
    /// <returns>The value produced by the call.</returns>
    public static T GetOrThrow<T>(this Result<T> result)
    {
        if (result.IsSuccess) return result.Value!;
        result.ThrowOnFailure();
        return default!;
    }

    // Rethrows with the original stack trace; a plain `throw exception;` would reset it to this line.
    private static void ThrowOnFailure<T>(this Result<T> result)
    {
        if (result.Exception is { } exception)
            ExceptionDispatchInfo.Capture(exception).Throw();
    }

    /// <summary>Awaits the result, then returns its value or one derived from the exception.</summary>
    /// <param name="task">The pending operation; it is awaited once.</param>
    /// <param name="onFailure">Builds the fallback asynchronously from the exception; it runs only on failure.</param>
    /// <returns>A task that yields the value or whatever <paramref name="onFailure" /> produced.</returns>
    public static async Task<T> GetOrElse<T>(
        this Task<Result<T>> task,
        Func<Exception, Task<T>> onFailure)
    {
        var result = await task.ConfigureAwait(false);
        return result.IsSuccess ? result.Value! : await onFailure(result.Exception!).ConfigureAwait(false);
    }

    /// <summary>Awaits the result, then returns its value; on failure the stored exception is rethrown.</summary>
    /// <param name="task">The pending operation; it is awaited once.</param>
    /// <returns>A task that yields the value produced by the call.</returns>
    public static async Task<T> GetOrThrow<T>(this Task<Result<T>> task)
    {
        var result = await task.ConfigureAwait(false);
        if (result.IsSuccess) return result.Value!;
        result.ThrowOnFailure();
        return default!;
    }

    /// <summary>Maps the value to <typeparamref name="TU" /> when the call succeeded; a failure is passed on unchanged.</summary>
    /// <param name="result">The result to map.</param>
    /// <param name="selector">Transforms the value; it runs only on success.</param>
    /// <returns>A new result holding the mapped value, or the original exception.</returns>
    public static Result<TU> Select<T, TU>(this Result<T> result, Func<T, TU> selector) => result.IsSuccess
        ? Result<TU>.Success(selector(result.Value!))
        : Result<TU>.Failure(result.Exception!);

    /// <summary>
    ///     Awaits the result, then maps its value to <typeparamref name="TU" />. The selector is synchronous; chain
    ///     <see cref="SelectMany{T,TU}(Task{Result{T}}, Func{T,Task{Result{TU}}})" /> when the mapping itself is async.
    /// </summary>
    /// <param name="task">The pending operation; it is awaited once.</param>
    /// <param name="selector">Transforms the value; it runs only on success.</param>
    /// <returns>A task that yields the mapped result, or the original exception.</returns>
    public static async Task<Result<TU>> Select<T, TU>(
        this Task<Result<T>> task,
        Func<T, TU> selector)
    {
        var result = await task.ConfigureAwait(false);
        return result.IsSuccess
            ? Result<TU>.Success(selector(result.Value!))
            : Result<TU>.Failure(result.Exception!);
    }

    /// <summary>
    ///     Feeds the value into <paramref name="binder" /> and returns its result, which is the bind step that query
    ///     syntax calls. A failure short-circuits without running <paramref name="binder" />.
    /// </summary>
    /// <param name="result">The result to bind.</param>
    /// <param name="binder">Produces the next result from the value; it runs only on success.</param>
    /// <returns>The bound result, or the original exception when the first result failed.</returns>
    public static Result<TU> SelectMany<T, TU>(this Result<T> result, Func<T, Result<TU>> binder) =>
        result.IsSuccess ? binder(result.Value!) : Result<TU>.Failure(result.Exception!);

    /// <summary>
    ///     Query-syntax bind plus projection: <paramref name="binder" /> runs on the value, then
    ///     <paramref name="projector" /> combines the original value with the bound one.
    /// </summary>
    /// <param name="result">The result to bind.</param>
    /// <param name="binder">Produces the intermediate result; it runs only on success.</param>
    /// <param name="projector">Combines the original and intermediate values into the final one.</param>
    /// <returns>The projected result, or the first exception encountered.</returns>
    public static Result<TV>
        SelectMany<T, TU, TV>(this Result<T> result, Func<T, Result<TU>> binder, Func<T, TU, TV> projector) =>
        result.IsSuccess
            ? binder(result.Value!).Select(u => projector(result.Value!, u))
            : Result<TV>.Failure(result.Exception!);

    /// <summary>Awaits the result, then feeds its value into an asynchronous <paramref name="binder" />.</summary>
    /// <param name="task">The pending operation; it is awaited once.</param>
    /// <param name="binder">Produces the next result from the value; it runs only on success.</param>
    /// <returns>A task that yields the bound result, or the original exception.</returns>
    public static async Task<Result<TU>> SelectMany<T, TU>(
        this Task<Result<T>> task,
        Func<T, Task<Result<TU>>> binder)
    {
        var result = await task.ConfigureAwait(false);
        return result.IsSuccess
            ? await binder(result.Value!).ConfigureAwait(false)
            : Result<TU>.Failure(result.Exception!);
    }

    /// <summary>Awaits the result, binds it asynchronously, then projects the two values into one.</summary>
    /// <param name="task">The pending operation; it is awaited once.</param>
    /// <param name="binder">Produces the intermediate result; it runs only on success.</param>
    /// <param name="projector">Combines the original and intermediate values into the final one.</param>
    /// <returns>A task that yields the projected result, or the first exception encountered.</returns>
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

    /// <summary>Awaits the result, binds it asynchronously, then projects through another asynchronous step.</summary>
    /// <param name="task">The pending operation; it is awaited once.</param>
    /// <param name="binder">Produces the intermediate result; it runs only on success.</param>
    /// <param name="projector">Combines the original and intermediate values into the final one, asynchronously.</param>
    /// <returns>A task that yields the projected result, or the first exception encountered.</returns>
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