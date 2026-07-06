using System;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Editor.Patches
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [PublicAPI]
    public sealed class ExportsPatchLoader : Attribute
    {
        public ExportsPatchLoader(Type type) => Type = type;
        public Type Type { get; }
    }
}