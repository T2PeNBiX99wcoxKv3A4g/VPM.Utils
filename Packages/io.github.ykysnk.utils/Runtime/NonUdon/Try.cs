using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

[PublicAPI]
public static class Try
{
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

    public static async Task<Result<T>> RunAsync<T>(Func<Task<T>> func)
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

    public static async Task<Result<Unit>> RunAsync<T>(Func<Task> func)
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

    public static Result<T, Exception> Run2<T>(Func<T> func)
    {
        try
        {
            return Result<T, Exception>.Ok(func());
        }
        catch (Exception e)
        {
            return Result<T, Exception>.Err(e);
        }
    }

    public static Result<Unit, Exception> Run2(Action action)
    {
        try
        {
            action();
            return Result<Unit, Exception>.Ok(Unit.Value);
        }
        catch (Exception e)
        {
            return Result<Unit, Exception>.Err(e);
        }
    }

    public static async Task<Result<T, Exception>> RunAsync2<T>(Func<Task<T>> func)
    {
        try
        {
            return Result<T, Exception>.Ok(await func().ConfigureAwait(false));
        }
        catch (Exception e)
        {
            return Result<T, Exception>.Err(e);
        }
    }

    public static async Task<Result<Unit, Exception>> RunAsync2(Func<Task> func)
    {
        try
        {
            await func().ConfigureAwait(false);
            return Result<Unit, Exception>.Ok(Unit.Value);
        }
        catch (Exception e)
        {
            return Result<Unit, Exception>.Err(e);
        }
    }
}

public readonly struct Unit : IEquatable<Unit>
{
    public static readonly Unit Value = new();

    public bool Equals(Unit other) => true;
    public override bool Equals(object? obj) => obj is Unit;
    public override int GetHashCode() => 0;
    public override string ToString() => "Unit";
}