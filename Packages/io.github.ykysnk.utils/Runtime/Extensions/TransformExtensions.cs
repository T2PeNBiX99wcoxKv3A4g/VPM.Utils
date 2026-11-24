#if UTILS_VRC_SDK3_BASE || COMPILER_UDONSHARP
#define UTILS_UDONSHARP
#endif

using System;
using JetBrains.Annotations;
using UnityEngine;
#if UTILS_UDONSHARP
using VRC.SDKBase;
#endif

namespace io.github.ykysnk.utils.Extensions
{
    /// <summary>
    ///     Transform helpers for hierarchy names, world/local scale conversion, unscaled point conversion and distance
    ///     checks.
    /// </summary>
    [PublicAPI]
    public static class TransformExtensions
    {
        /// <summary>
        ///     Full hierarchy path of the transform, built upwards to the root and joined with '/', for example
        ///     "Root/Child/Name". There is no leading slash.
        /// </summary>
        /// <param name="transform">The transform whose name and parents are read; it is not modified.</param>
        /// <returns>The path, or the transform's own name when it has no parent.</returns>
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
        ///     World scale of the transform, identical to <see cref="Transform.lossyScale" />. Kept for source compatibility
        ///     only.
        /// </summary>
        /// <param name="transform">The transform whose world scale is read.</param>
        /// <returns>The combined scale of the transform and all of its parents.</returns>
        [Obsolete("Use Transform.lossyScale instead.")]
        public static Vector3 GetWorldScale([NotNull] this Transform transform) => transform.lossyScale;

        /// <summary>
        ///     Local scale that makes the transform's world scale equal to (1, 1, 1), i.e. one divided by the parent's lossy
        ///     scale.
        /// </summary>
        /// <param name="transform">The transform whose parent scale is divided into one.</param>
        /// <returns>The local scale for a unit world scale, or (1, 1, 1) when the transform has no parent.</returns>
        public static Vector3 GetLocalScaleFollowWorldScale([NotNull] this Transform transform) =>
            transform.GetLocalScaleFromLossyScale(Vector3.one);

        /// <summary>
        ///     Component-wise division of the other transform's world scale by this transform's world scale, i.e. how many
        ///     times larger the other object is per axis.
        /// </summary>
        /// <param name="transform">The transform supplying the divisor, its own lossy scale.</param>
        /// <param name="other">The transform supplying the dividend; read without a null check.</param>
        /// <returns>The per-axis ratio of the two world scales, which is 1 where both are equally scaled.</returns>
        public static Vector3 GetTargetLocalScale([NotNull] this Transform transform, [NotNull] Transform other)
        {
            var worldScale = transform.lossyScale;
            var objWorldScale = other.lossyScale;
            return objWorldScale.Divide(worldScale);
        }

        /// <summary>
        ///     Converts a wanted world scale into the local scale that produces it, by dividing it by the parent's lossy
        ///     scale. Rotation and shear from a non-uniformly scaled parent are not compensated, so the result is exact only
        ///     for uniformly scaled parents.
        /// </summary>
        /// <param name="transform">The transform whose parent scale is used.</param>
        /// <param name="lossyScale">The wanted world scale; returned unchanged when the transform has no parent.</param>
        /// <returns>The local scale to assign to the transform.</returns>
        public static Vector3 GetLocalScaleFromLossyScale([NotNull] this Transform transform, Vector3 lossyScale)
        {
            if (transform.parent == null) return lossyScale;
            var parentScale = transform.parent.lossyScale;
            return lossyScale.Divide(parentScale);
        }

        /// <summary>
        ///     Sets the local scale so the transform ends up with the wanted world scale, without touching the parent. Same
        ///     limitation as <see cref="GetLocalScaleFromLossyScale" />.
        /// </summary>
        /// <param name="transform">The transform whose local scale is assigned; it is mutated.</param>
        /// <param name="lossyScale">The wanted world scale in Unity units.</param>
        public static void SetLossyScale([NotNull] this Transform transform, Vector3 lossyScale) =>
            transform.localScale = transform.GetLocalScaleFromLossyScale(lossyScale);

        /// <summary>
        ///     Identical to <see cref="SetLossyScale" />: assigns the local scale that yields the wanted world scale and
        ///     leaves the parent transform untouched.
        /// </summary>
        /// <param name="transform">The transform whose local scale is assigned; it is mutated.</param>
        /// <param name="lossyScale">The wanted world scale in Unity units.</param>
        public static void SetLossyScaleInPlace([NotNull] this Transform transform, Vector3 lossyScale) =>
            transform.SetLossyScale(lossyScale);

        /// <summary>
        ///     GameObject overload of the Transform version; reads the other object's Transform.
        /// </summary>
        /// <param name="transform">The transform supplying the divisor, its own lossy scale.</param>
        /// <param name="other">The GameObject supplying the dividend; its Transform is read without a null check.</param>
        /// <returns>The per-axis ratio of the two world scales, which is 1 where both are equally scaled.</returns>
        public static Vector3 GetTargetLocalScale([NotNull] this Transform transform, [NotNull] GameObject other) =>
            transform.GetTargetLocalScale(other.transform);

        // Refs: https://discussions.unity.com/t/version-of-transform-transformpoint-which-is-unaffected-by-scale/172259
        /// <summary>
        ///     Converts a point from the transform's local space to world space while ignoring scale: the point is
        ///     multiplied by a translation and rotation matrix built with a scale of one. Parent scaling still moves the
        ///     result because it moves the transform's position, but the local offset itself is never scaled.
        /// </summary>
        /// <param name="transform">The transform whose position and rotation form the matrix; it is not modified.</param>
        /// <param name="position">The offset in local space, in Unity units.</param>
        /// <returns>The corresponding world position, with the scale factor still 1.</returns>
        public static Vector3 TransformPointUnscaled([NotNull] this Transform transform, Vector3 position)
        {
            var localToWorldMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            return localToWorldMatrix.MultiplyPoint3x4(position);
        }

        /// <summary>
        ///     Converts a world position into the transform's local space while ignoring scale; the inverse of
        ///     <see cref="TransformPointUnscaled" />.
        /// </summary>
        /// <param name="transform">The transform whose position and rotation form the inverse matrix; not modified.</param>
        /// <param name="position">The world position to convert, in Unity units.</param>
        /// <returns>The position relative to the transform, without dividing by its scale.</returns>
        public static Vector3 InverseTransformPointUnscaled([NotNull] this Transform transform, Vector3 position)
        {
            var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
            return worldToLocalMatrix.MultiplyPoint3x4(position);
        }

        /// <summary>
        ///     Distance between the two world positions, on all three axes.
        /// </summary>
        /// <param name="transform">The transform whose position is measured from.</param>
        /// <param name="other">The transform whose position is measured to.</param>
        /// <returns>A non-negative distance in Unity units.</returns>
        public static float Distance([NotNull] this Transform transform, [NotNull] Transform other) =>
            transform.position.Distance(other.position);

        /// <summary>
        ///     Distance between the transform's world position and a world point, on all three axes.
        /// </summary>
        /// <param name="transform">The transform whose position is measured from.</param>
        /// <param name="other">The world point measured to, in Unity units.</param>
        /// <returns>A non-negative distance in Unity units.</returns>
        public static float Distance([NotNull] this Transform transform, Vector3 other) =>
            transform.position.Distance(other);

        /// <summary>
        ///     Distance between the two world positions on the XZ plane; the Y difference is ignored, so height does not
        ///     count.
        /// </summary>
        /// <param name="transform">The transform whose position is measured from.</param>
        /// <param name="other">The transform whose position is measured to.</param>
        /// <returns>A non-negative distance in Unity units on the XZ plane.</returns>
        public static float Distance2D([NotNull] this Transform transform, [NotNull] Transform other) =>
            transform.position.Distance2D(other.position);

        /// <summary>
        ///     Distance between the transform's world position and a world point on the XZ plane; the Y difference is
        ///     ignored.
        /// </summary>
        /// <param name="transform">The transform whose position is measured from.</param>
        /// <param name="other">The world point measured to, in Unity units.</param>
        /// <returns>A non-negative distance in Unity units on the XZ plane.</returns>
        public static float Distance2D([NotNull] this Transform transform, Vector3 other) =>
            transform.position.Distance2D(other);

        // Refs: https://qiita.com/tamsco274/items/c7d1edda267326ec0404#%E6%94%B9%E9%80%A0%E3%82%AF%E3%83%A9%E3%82%A4%E3%82%A2%E3%83%B3%E3%83%88%E5%AF%BE%E7%AD%96
        /// <summary>
        ///     Compares the two world positions on all three axes.
        /// </summary>
        /// <param name="transform">The transform whose position is measured from.</param>
        /// <param name="other">The transform whose position is measured to.</param>
        /// <param name="distance">Threshold in Unity units; the check is strictly less than this value.</param>
        /// <returns>True when the distance is smaller than the threshold, false when it is equal or larger.</returns>
        public static bool IsCloseRange([NotNull] this Transform transform, [NotNull] Transform other, float distance) =>
            transform.Distance(other) < distance;

        /// <summary>
        ///     Compares the two world positions on the XZ plane, so height differences are ignored.
        /// </summary>
        /// <param name="transform">The transform whose position is measured from.</param>
        /// <param name="other">The transform whose position is measured to.</param>
        /// <param name="distance">Threshold in Unity units on the XZ plane; the check is strictly less than this value.</param>
        /// <returns>True when the planar distance is smaller than the threshold, false when it is equal or larger.</returns>
        public static bool IsCloseRange2D([NotNull] this Transform transform, [NotNull] Transform other,
            float distance) =>
            transform.Distance2D(other) < distance;

#if UTILS_UDONSHARP
        /// <summary>
        ///     VRChat only. Compares the local player's world position with the transform's position on all three axes.
        /// </summary>
        /// <param name="transform">The transform whose position is measured from.</param>
        /// <param name="distance">Threshold in Unity units; the check is strictly less than this value.</param>
        /// <returns>True when the player is closer than the threshold, false also when no valid local player exists.</returns>
        public static bool IsPlayerCloseRange([NotNull] this Transform transform, float distance)
        {
            var player = Networking.LocalPlayer;
            if (!Utilities.IsValid(player)) return false;
            return player.GetPosition().Distance(transform) < distance;
        }

        /// <summary>
        ///     VRChat only. Compares the local player's world position with the transform's position on the XZ plane, so
        ///     height differences are ignored.
        /// </summary>
        /// <param name="transform">The transform whose position is measured from.</param>
        /// <param name="distance">Threshold in Unity units on the XZ plane; the check is strictly less than this value.</param>
        /// <returns>True when the player is closer than the threshold, false also when no valid local player exists.</returns>
        public static bool IsPlayerCloseRange2D([NotNull] this Transform transform, float distance)
        {
            var player = Networking.LocalPlayer;
            if (!Utilities.IsValid(player)) return false;
            return player.GetPosition().Distance2D(transform) < distance;
        }
#endif
    }
}