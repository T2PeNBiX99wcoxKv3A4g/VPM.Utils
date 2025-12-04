using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class GameObjectExtensions
    {
        [CanBeNull]
        public static string FullName(this GameObject obj) => obj.transform.FullName();

        public static bool IsCloseRange(this GameObject obj, [NotNull] GameObject other, float distance) =>
            obj.transform.IsCloseRange(other.transform, distance);

        public static bool IsCloseRange2D(this GameObject obj, [NotNull] GameObject other, float distance) =>
            obj.transform.IsCloseRange2D(other.transform, distance);

#if UTILS_VRC_SDK3_BASE
        public static bool IsPlayerCloseRange(this GameObject obj, float distance) =>
            obj.transform.IsPlayerCloseRange(distance);

        public static bool IsPlayerCloseRange2D(this GameObject obj, float distance) =>
            obj.transform.IsPlayerCloseRange2D(distance);
#endif
    }
}