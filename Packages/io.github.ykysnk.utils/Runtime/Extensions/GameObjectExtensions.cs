using JetBrains.Annotations;
using UnityEngine;
using VRC.SDKBase;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class GameObjectExtensions
    {
        public static string? FullName(this GameObject obj) =>
            !Utilities.IsValid(obj.transform) ? null : obj.transform.FullName();
    }
}