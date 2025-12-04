using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class ComponentExtensions
    {
        [CanBeNull]
        public static string FullName(this Component obj) => obj.transform.FullName();
    }
}