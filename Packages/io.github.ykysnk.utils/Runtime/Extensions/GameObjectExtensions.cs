using System;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Extensions
{
#if !COMPILER_UDONSHARP
    public delegate void ComponentAction(int index, Component component);

    public delegate T ComponentSelect<out T>(int index, Component component);
#endif
    [PublicAPI]
    public static class GameObjectExtensions
    {
        [CanBeNull]
        public static string FullName([NotNull] this GameObject obj) => obj.transform.FullName();

        public static bool IsCloseRange([NotNull] this GameObject obj, [NotNull] GameObject other, float distance) =>
            obj.transform.IsCloseRange(other.transform, distance);

        public static bool IsCloseRange2D([NotNull] this GameObject obj, [NotNull] GameObject other, float distance) =>
            obj.transform.IsCloseRange2D(other.transform, distance);

        /// <summary>
        ///     Calculates and returns the world scale of the specified transform.
        ///     Same as <see cref="Transform.lossyScale" />
        /// </summary>
        /// <param name="obj">The transform for which the world scale is to be calculated.</param>
        /// <returns>The scale in world space</returns>
        [Obsolete("Use Transform.lossyScale instead.")]
        public static Vector3 GetWorldScale([NotNull] this GameObject obj) => obj.transform.GetWorldScale();

        /// <summary>
        ///     Calculates and returns the local scale same as a world scale.
        /// </summary>
        /// <param name="obj">The transform for which the local scale is to be calculated.</param>
        /// <returns>The scale in local space equals world space</returns>
        public static Vector3 GetLocalScaleFollowWorldScale([NotNull] this GameObject obj) =>
            obj.transform.GetLocalScaleFollowWorldScale();

        /// <summary>
        ///     Calculates and returns the local scale of the target transform relative to another transform.
        /// </summary>
        /// <param name="obj">The transform for which the local scale is to be calculated.</param>
        /// <param name="other">The transform relative to which the local scale is determined.</param>
        /// <returns>The local scale of the target transform in relation to the other transform.</returns>
        public static Vector3 GetTargetLocalScale([NotNull] this GameObject obj, [NotNull] GameObject other) =>
            obj.transform.GetTargetLocalScale(other.transform);

        public static bool TryGetComponentAtIndex([NotNull] this GameObject obj, int index,
            [CanBeNull] out Component component)
        {
            if (index < 0 || index >= obj.GetComponentCount())
            {
                component = null;
                return false;
            }

            component = obj.GetComponentAtIndex(index);
            return true;
        }

        [CanBeNull]
        public static Component GetComponentAtIndexOrDefault([NotNull] this GameObject obj, int index) =>
            obj.TryGetComponentAtIndex(index, out var component) ? component : null;

        public static Component[] GetComponents([NotNull] this GameObject obj)
        {
            var count = obj.GetComponentCount();
            var ret = new Component[count];

            for (var i = 0; i < count; i++)
                ret[i] = obj.GetComponentAtIndex(i);

            return ret;
        }

#if !COMPILER_UDONSHARP
        public static void ComponentsForeach([NotNull] this GameObject obj, ComponentAction componentAction)
        {
            var count = obj.GetComponentCount();

            for (var i = 0; i < count; i++)
                componentAction(i, obj.GetComponentAtIndex(i));
        }

        public static T[] ComponentsSelect<T>([NotNull] this GameObject obj, ComponentSelect<T> selector)
        {
            var count = obj.GetComponentCount();
            var ret = new T[count];

            for (var i = 0; i < count; i++)
                ret[i] = selector(i, obj.GetComponentAtIndex(i));

            return ret;
        }
#endif

#if !COMPILER_UDONSHARP
        public static bool IsSceneObject([NotNull] this GameObject obj) => obj.scene.IsValid() && !Utils.IsInPrefab;
#else
        public static bool IsSceneObject([NotNull] this GameObject obj) => false;
#endif

#if UTILS_VRC_SDK3_BASE
        public static bool IsPlayerCloseRange([NotNull] this GameObject obj, float distance) =>
            obj.transform.IsPlayerCloseRange(distance);

        public static bool IsPlayerCloseRange2D([NotNull] this GameObject obj, float distance) =>
            obj.transform.IsPlayerCloseRange2D(distance);
#endif
    }
}