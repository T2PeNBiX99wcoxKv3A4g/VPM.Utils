using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon.Extensions;

[PublicAPI]
public static class OptionExtensions
{
    public static Option<TU> Select<T, TU>(this Option<T> option, Func<T, TU> selector) =>
        option.HasValue ? Option<TU>.Some(selector(option.Value!)) : Option<TU>.None();

    public static Option<TU> SelectMany<T, TU>(this Option<T> option, Func<T, Option<TU>> binder) =>
        option.HasValue ? binder(option.Value!) : Option<TU>.None();

    public static Option<TV>
        SelectMany<T, TU, TV>(this Option<T> option, Func<T, Option<TU>> binder, Func<T, TU, TV> projector) =>
        option.HasValue
            ? binder(option.Value!).Select(u => projector(option.Value!, u))
            : Option<TV>.None();

    public static async Task<Option<TU>> Select<T, TU>(
        this Task<Option<T>> task,
        Func<T, TU> selector)
    {
        var option = await task.ConfigureAwait(false);
        return option.HasValue
            ? Option<TU>.Some(selector(option.Value!))
            : Option<TU>.None();
    }

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
}