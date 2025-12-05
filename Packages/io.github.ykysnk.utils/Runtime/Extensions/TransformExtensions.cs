using JetBrains.Annotations;
using UnityEngine;
#if UTILS_VRC_SDK3_BASE
using VRC.SDKBase;
#endif

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class TransformExtensions
    {
        public static string FullName(this Transform transform)
        {
            var tmpName = transform.name;

            while (transform.parent != null)
            {
                transform = transform.parent;
                tmpName = transform.name + "/" + tmpName;
            }

            return tmpName;
        }

        public static Vector3 GetWorldScale(this Transform transform)
        {
            var worldScale = Vector3.one;
            if (transform.parent != null) worldScale = transform.parent.TransformVector(Vector3.one);
            return worldScale;
        }

        public static Vector3 GetLocalScaleFromWorldScale(this Transform transform)
        {
            var localScale = Vector3.one;
            if (transform.parent != null) localScale = transform.parent.InverseTransformVector(Vector3.one);
            return localScale;
        }

        public static float Distance(this Transform transform, [NotNull] Transform other) =>
            transform.position.Distance(other.position);

        public static float Distance(this Transform transform, Vector3 other) => transform.position.Distance(other);

        public static float Distance2D(this Transform transform, [NotNull] Transform other) =>
            transform.position.Distance2D(other.position);

        public static float Distance2D(this Transform transform, Vector3 other) => transform.position.Distance2D(other);

        // Refs: https://qiita.com/tamsco274/items/c7d1edda267326ec0404#%E6%94%B9%E9%80%A0%E3%82%AF%E3%83%A9%E3%82%A4%E3%82%A2%E3%83%B3%E3%83%88%E5%AF%BE%E7%AD%96
        public static bool IsCloseRange(this Transform transform, [NotNull] Transform other, float distance) =>
            transform.Distance(other) < distance;

        public static bool IsCloseRange2D(this Transform transform, [NotNull] Transform other, float distance) =>
            transform.Distance2D(other) < distance;

#if UTILS_VRC_SDK3_BASE
        public static bool IsPlayerCloseRange(this Transform transform, float distance)
        {
            var player = Networking.LocalPlayer;
            if (!Utilities.IsValid(player)) return false;
            return player.GetPosition().Distance(transform) < distance;
        }

        public static bool IsPlayerCloseRange2D(this Transform transform, float distance)
        {
            var player = Networking.LocalPlayer;
            if (!Utilities.IsValid(player)) return false;
            return player.GetPosition().Distance2D(transform) < distance;
        }
#endif
    }
}