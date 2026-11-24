using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.NonUdon;

/// <summary>
///     Runs a delegate and turns anything it throws into a <see cref="Result{T}" />, so callers can inspect a
///     failure without a <c>try</c> block. The exception keeps the stack trace of the throw site.
/// </summary>
[PublicAPI]
public static class Try
{
    /// <summary>Calls <paramref name="func" /> and captures its return value or the exception it throws.</summary>
    /// <param name="func">The work to run; it is invoked once, synchronously, on the calling thread.</param>
    /// <returns>A successful result holding the return value, or a failed result holding the exception.</returns>
    [HideInCallstack]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Result<T> Run<T>(Func<T> func)
    {
        try
        {
            return Result<T>.Success(func());
        }
        catch (Exception e)
        {
            return Result<T>.Failure(e);
        }
    }

    /// <summary>Runs <paramref name="action" /> and reports success with <see cref="Unit.Value" /> instead of a value.</summary>
    /// <param name="action">The work to run; it is invoked once, synchronously, on the calling thread.</param>
    /// <returns>A successful result carrying <see cref="Unit.Value" />, or a failed result holding the exception.</returns>
    [HideInCallstack]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Result<Unit> Run(Action action)
    {
        try
        {
            action();
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception e)
        {
            return Result<Unit>.Failure(e);
        }
    }

    /// <summary>
    ///     Awaits <paramref name="func" /> and captures its result or the exception it throws. The continuation runs
    ///     without a captured Unity context, so Unity objects must not be touched after the await.
    /// </summary>
    /// <param name="func">The asynchronous work; it is started once and awaited to completion.</param>
    /// <returns>A task that yields a successful result holding the value, or a failed one holding the exception.</returns>
    [HideInCallstack]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static async Task<Result<T>> Run<T>(Func<Task<T>> func)
    {
        try
        {
            return Result<T>.Success(await func().ConfigureAwait(false));
        }
        catch (Exception e)
        {
            return Result<T>.Failure(e);
        }
    }

    /// <summary>
    ///     Awaits <paramref name="func" /> and reports success with <see cref="Unit.Value" />. The continuation runs
    ///     without a captured Unity context, so Unity objects must not be touched after the await.
    /// </summary>
    /// <param name="func">The asynchronous work; it is started once and awaited to completion.</param>
    /// <returns>A task that yields a successful result carrying <see cref="Unit.Value" />, or a failed one holding the exception.</returns>
    [HideInCallstack]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static async Task<Result<Unit>> Run(Func<Task> func)
    {
        try
        {
            await func().ConfigureAwait(false);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception e)
        {
            return Result<Unit>.Failure(e);
        }
    }
}

/// <summary>
///     Stand-in for <c>void</c> where a type argument is required, so an operation with no return value can still
///     produce a <see cref="Result{T}" />. All instances compare equal.
/// </summary>
[PublicAPI]
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>The instance to return from a successful call that has no value.</summary>
    public static readonly Unit Value = new();

    /// <summary>A completed task carrying <see cref="Value" />, for APIs that want a <c>Task&lt;Unit&gt;</c>.</summary>
    public static readonly Task<Unit> TaskValue = Task.FromResult(Value);

    /// <summary>Always equal, because every instance represents the same empty value.</summary>
    /// <param name="other">The other instance; its contents are irrelevant.</param>
    /// <returns><see langword="true" /> in every case.</returns>
    public bool Equals(Unit other) => true;

    /// <summary>Whether <paramref name="obj" /> is any <see cref="Unit" />.</summary>
    /// <param name="obj">The instance to test; any other type returns <see langword="false" />.</param>
    /// <returns><see langword="true" /> for a <see cref="Unit" />, <see langword="false" /> otherwise.</returns>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>Always 0, because all instances are equal.</summary>
    /// <returns>Zero.</returns>
    public override int GetHashCode() => 0;

    /// <summary>Returns the constant name <c>Unit</c>.</summary>
    /// <returns>The literal <c>"Unit"</c>.</returns>
    public override string ToString() => "Unit";

    /// <summary>Compares two instances, which are always equal.</summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns><see langword="true" /> in every case.</returns>
    public static bool operator ==(Unit left, Unit right) => left.Equals(right);

    /// <summary>Compares two instances, which are never unequal.</summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns><see langword="false" /> in every case.</returns>
    public static bool operator !=(Unit left, Unit right) => !(left == right);
}
