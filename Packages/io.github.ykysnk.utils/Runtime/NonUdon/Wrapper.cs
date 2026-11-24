using System.Collections.Generic;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

/// <summary>
///     Entry point for the serializable wrappers. <see cref="ListWrapper{T}" /> and
///     <see cref="DictionaryWrapper{TKey,TValue}" /> exist because Unity cannot serialize a dictionary or a member
///     typed as an interface directly. Their constructors are internal, so calling code gets instances from here or
///     from the implicit conversions on those types.
/// </summary>
[PublicAPI]
public static class Wrapper
{
    /// <summary>Copies <paramref name="source" /> into a <see cref="ListWrapper{T}" />.</summary>
    /// <param name="source">The items to copy, in enumeration order; null throws while the backing list is created.</param>
    /// <returns>A new wrapper that owns its own list.</returns>
    public static ListWrapper<T> Create<T>(IEnumerable<T> source) => new(source);
    /// <summary>Copies the pairs of <paramref name="source" /> into a <see cref="DictionaryWrapper{TKey,TValue}" />.</summary>
    /// <param name="source">The pairs to copy; keys and values are captured in enumeration order.</param>
    /// <returns>A new wrapper whose two lists stay aligned with each other.</returns>
    public static DictionaryWrapper<TK, TV> Create<TK, TV>(IDictionary<TK, TV> source) => new(source);
}