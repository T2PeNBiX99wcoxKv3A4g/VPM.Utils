using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.NonUdon;

[PublicAPI]
public static class Try
{
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

public readonly struct Unit : IEquatable<Unit>
{
    public static readonly Unit Value = new();
    public static readonly Task<Unit> TaskValue = Task.FromResult(Value);

    public bool Equals(Unit other) => true;
    public override bool Equals(object? obj) => obj is Unit;
    public override int GetHashCode() => 0;
    public override string ToString() => "Unit";

    public static bool operator ==(Unit left, Unit right) => left.Equals(right);
    public static bool operator !=(Unit left, Unit right) => !(left == right);
}