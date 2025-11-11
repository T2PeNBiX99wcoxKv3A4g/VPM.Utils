using JetBrains.Annotations;
using UnityEngine;
using VRC.SDKBase;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class ComponentExtensions
    {
        public static string? FullName(this Component obj) => !Utilities.IsValid(obj) ? null : obj.transform.FullName();
    }
}