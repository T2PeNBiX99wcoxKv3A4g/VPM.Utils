using JetBrains.Annotations;
using UnityEngine;
using VRC.SDKBase;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class TransformExtensions
    {
        public static string FullName(this Transform transform)
        {
            var tmpName = transform.name;

            while (Utilities.IsValid(transform.parent))
            {
                transform = transform.parent;
                tmpName = transform.name + "/" + tmpName;
            }

            return tmpName;
        }

        // Refs: https://discussions.unity.com/t/world-scale/374693, https://stackoverflow.com/questions/47669172/unity-scale-a-gameobject-in-world-space
        public static Vector3 GetWorldScale(this Transform transform)
        {
            var worldScale = transform.localScale;
            var parent = transform.parent;

            while (parent)
            {
                worldScale = Vector3.Scale(worldScale, parent.localScale);
                parent = parent.parent;
            }

            return worldScale;
        }

        public static Vector3 GetLocalScaleFromWorldScale(this Transform transform)
        {
            var worldScale = Vector3.one;
            var parent = transform.parent;

            while (parent)
            {
                worldScale = parent.InverseTransformVector(worldScale);
                parent = parent.parent;
            }

            return worldScale;
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
    }
}