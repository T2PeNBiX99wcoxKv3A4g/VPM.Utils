using System;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Editor.Patches
{
    /// <summary>
    ///     Assembly attribute declaring one <see cref="IPatchLoader" /> type to run when the editor loads.
    ///     This is how another assembly hands its patch loaders to the framework: every declared type is read,
    ///     instantiated and loaded on editor start-up.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [PublicAPI]
    public sealed class ExportsPatchLoader : Attribute
    {
        /// <summary>Declares <paramref name="type" /> as a patch loader of the assembly carrying this attribute.</summary>
        /// <param name="type">
        ///     The loader type; it must implement <see cref="IPatchLoader" />, normally by deriving from
        ///     <see cref="PatchLoader{T}" />, otherwise the attribute is ignored when the loaders are collected.
        /// </param>
        public ExportsPatchLoader(Type type) => Type = type;
        /// <summary>The loader type this attribute declares.</summary>
        public Type Type { get; }
    }
}
