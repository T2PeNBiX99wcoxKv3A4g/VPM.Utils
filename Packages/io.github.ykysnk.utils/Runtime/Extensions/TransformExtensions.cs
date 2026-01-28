using System;
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
        public static string FullName([NotNull] this Transform transform)
        {
            var tmpName = transform.name;

            while (transform.parent != null)
            {
                transform = transform.parent;
                tmpName = transform.name + "/" + tmpName;
            }

            return tmpName;
        }

        /// <summary>
        ///     Calculates and returns the world scale of the specified transform.
        ///     Same as <see cref="Transform.lossyScale" />
        /// </summary>
        /// <param name="transform">The transform for which the world scale is to be calculated.</param>
        /// <returns>The scale in world space</returns>
        [Obsolete("Use Transform.lossyScale instead.")]
        public static Vector3 GetWorldScale([NotNull] this Transform transform) => transform.lossyScale;

        /// <summary>
        ///     Calculates and returns the local scale follow a world scale.
        /// </summary>
        /// <param name="transform">The transform for which the local scale is to be calculated.</param>
        /// <returns>The scale in local space equals world space</returns>
        public static Vector3 GetLocalScaleFollowWorldScale([NotNull] this Transform transform) =>
            transform.GetLocalScaleFromLossyScale(Vector3.one);

        /// <summary>
        ///     Calculates and returns the local scale of the target transform relative to another transform.
        /// </summary>
        /// <param name="transform">The transform for which the local scale is to be calculated.</param>
        /// <param name="other">The transform relative to which the local scale is determined.</param>
        /// <returns>The local scale of the target transform in relation to the other transform.</returns>
        public static Vector3 GetTargetLocalScale([NotNull] this Transform transform, [NotNull] Transform other)
        {
            var worldScale = transform.lossyScale;
            var objWorldScale = other.lossyScale;
            return objWorldScale.Divide(worldScale);
        }

        /// <summary>
        ///     Calculates and returns the local scale of a transform based on a given lossy scale,
        ///     taking into account the scale of its parent.
        /// </summary>
        /// <param name="transform">The transform whose local scale is to be calculated.</param>
        /// <param name="lossyScale">The lossy scale to use for the calculation.</param>
        /// <returns>The calculated local scale of the transform.</returns>
        public static Vector3 GetLocalScaleFromLossyScale([NotNull] this Transform transform, Vector3 lossyScale)
        {
            if (transform.parent == null) return transform.localScale = lossyScale;
            var parentScale = transform.parent.lossyScale;
            return lossyScale.Divide(parentScale);
        }

        /// <summary>
        ///     Sets the local scale of the specified transform such that its world scale matches the provided target lossy scale.
        /// </summary>
        /// <param name="transform">The transform whose local scale is to be adjusted.</param>
        /// <param name="lossyScale">The desired world scale to be achieved for the transform.</param>
        public static void SetLossyScale([NotNull] this Transform transform, Vector3 lossyScale) =>
            transform.localScale = transform.GetLocalScaleFromLossyScale(lossyScale);

        /// <summary>
        ///     Calculates and returns the local scale of the target transform relative to another transform.
        /// </summary>
        /// <param name="transform">The transform for which the local scale is to be calculated.</param>
        /// <param name="other">The transform relative to which the local scale is determined.</param>
        /// <returns>The local scale of the target transform in relation to the other transform.</returns>
        public static Vector3 GetTargetLocalScale([NotNull] this Transform transform, GameObject other) =>
            transform.GetTargetLocalScale(other.transform);

        // Refs: https://discussions.unity.com/t/version-of-transform-transformpoint-which-is-unaffected-by-scale/172259
        /// <summary>
        ///     Transforms a position from local space to world space without being affected by the scale of the transform.
        /// </summary>
        /// <param name="transform">The transform to use for the calculation.</param>
        /// <param name="position">The position in local space to be transformed.</param>
        /// <returns>The transformed position in world space.</returns>
        public static Vector3 TransformPointUnscaled([NotNull] this Transform transform, Vector3 position)
        {
            var localToWorldMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            return localToWorldMatrix.MultiplyPoint3x4(position);
        }

        /// <summary>
        ///     Transforms a position from world space to local space using the transform's position and rotation, but without
        ///     scaling.
        /// </summary>
        /// <param name="transform">The transform relative to which the position will be transformed.</param>
        /// <param name="position">The world-space position to be transformed into local space.</param>
        /// <returns>The position in the local space of the transform, without considering scaling.</returns>
        public static Vector3 InverseTransformPointUnscaled([NotNull] this Transform transform, Vector3 position)
        {
            var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
            return worldToLocalMatrix.MultiplyPoint3x4(position);
        }

        public static float Distance([NotNull] this Transform transform, [NotNull] Transform other) =>
            transform.position.Distance(other.position);

        public static float Distance([NotNull] this Transform transform, Vector3 other) =>
            transform.position.Distance(other);

        public static float Distance2D([NotNull] this Transform transform, [NotNull] Transform other) =>
            transform.position.Distance2D(other.position);

        public static float Distance2D([NotNull] this Transform transform, Vector3 other) =>
            transform.position.Distance2D(other);

        // Refs: https://qiita.com/tamsco274/items/c7d1edda267326ec0404#%E6%94%B9%E9%80%A0%E3%82%AF%E3%83%A9%E3%82%A4%E3%82%A2%E3%83%B3%E3%83%88%E5%AF%BE%E7%AD%96
        public static bool IsCloseRange([NotNull] this Transform transform, [NotNull] Transform other, float distance) =>
            transform.Distance(other) < distance;

        public static bool IsCloseRange2D([NotNull] this Transform transform, [NotNull] Transform other,
            float distance) =>
            transform.Distance2D(other) < distance;

#if UTILS_VRC_SDK3_BASE
        public static bool IsPlayerCloseRange([NotNull] this Transform transform, float distance)
        {
            var player = Networking.LocalPlayer;
            if (!Utilities.IsValid(player)) return false;
            return player.GetPosition().Distance(transform) < distance;
        }

        public static bool IsPlayerCloseRange2D([NotNull] this Transform transform, float distance)
        {
            var player = Networking.LocalPlayer;
            if (!Utilities.IsValid(player)) return false;
            return player.GetPosition().Distance2D(transform) < distance;
        }
#endif
    }
}