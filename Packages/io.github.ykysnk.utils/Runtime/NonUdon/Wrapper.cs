using System.Collections.Generic;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

[PublicAPI]
public static class Wrapper
{
    public static ListWrapper<T> Create<T>(IEnumerable<T> source) => new(source);
    public static DictionaryWrapper<TK, TV> Create<TK, TV>(IDictionary<TK, TV> source) => new(source);
}