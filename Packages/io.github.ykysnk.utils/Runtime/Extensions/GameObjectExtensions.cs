#if UTILS_VRC_SDK3_BASE || COMPILER_UDONSHARP
#define UTILS_UDONSHARP
#endif

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Extensions
{
    /// <summary>
    ///     GameObject helpers for hierarchy paths, distance checks and index based component access. Anything that needs a
    ///     position or a scale goes through the GameObject's Transform.
    /// </summary>
    [PublicAPI]
    public static class GameObjectExtensions
    {
        /// <summary>
        ///     Full hierarchy path of the GameObject, for example "Root/Child/Name", without a leading slash.
        /// </summary>
        /// <param name="obj">The GameObject whose Transform is used.</param>
        /// <returns>The path built from the GameObject's Transform up to the root of the hierarchy.</returns>
        public static string FullName([NotNull] this GameObject obj) => obj.transform.FullName();

        /// <summary>
        ///     Compares the two world positions in Unity units, on all three axes.
        /// </summary>
        /// <param name="obj">The GameObject whose pivot is measured from.</param>
        /// <param name="other">The GameObject whose pivot is measured to.</param>
        /// <param name="distance">Threshold in Unity units; the check is strictly less than this value.</param>
        /// <returns>True when the distance is smaller than the threshold, false when it is equal or larger.</returns>
        public static bool IsCloseRange([NotNull] this GameObject obj, [NotNull] GameObject other, float distance) =>
            obj.transform.IsCloseRange(other.transform, distance);

        /// <summary>
        ///     Compares the two world positions on the XZ plane only, so height differences are ignored.
        /// </summary>
        /// <param name="obj">The GameObject whose pivot is measured from.</param>
        /// <param name="other">The GameObject whose pivot is measured to.</param>
        /// <param name="distance">Threshold in Unity units on the XZ plane; the check is strictly less than this value.</param>
        /// <returns>True when the planar distance is smaller than the threshold, false when it is equal or larger.</returns>
        public static bool IsCloseRange2D([NotNull] this GameObject obj, [NotNull] GameObject other, float distance) =>
            obj.transform.IsCloseRange2D(other.transform, distance);

        /// <summary>
        ///     World scale of the GameObject, identical to <see cref="Transform.lossyScale" />. Kept for source
        ///     compatibility only.
        /// </summary>
        /// <param name="obj">The GameObject whose Transform is read.</param>
        /// <returns>The combined scale of the Transform and all of its parents.</returns>
        [Obsolete("Use Transform.lossyScale instead.")]
        public static Vector3 GetWorldScale([NotNull] this GameObject obj) => obj.transform.GetWorldScale();

        /// <summary>
        ///     Local scale that makes the GameObject's world scale equal to (1, 1, 1), i.e. one divided by the parent's
        ///     lossy scale. Returns (1, 1, 1) for a GameObject without a parent.
        /// </summary>
        /// <param name="obj">The GameObject whose parent scale is divided into one.</param>
        /// <returns>The local scale needed to reach a world scale of (1, 1, 1).</returns>
        public static Vector3 GetLocalScaleFollowWorldScale([NotNull] this GameObject obj) =>
            obj.transform.GetLocalScaleFollowWorldScale();

        /// <summary>
        ///     Component-wise division of the other object's world scale by this GameObject's world scale, i.e. how many
        ///     times larger the other object is per axis.
        /// </summary>
        /// <param name="obj">The GameObject supplying the divisor, its own lossy scale.</param>
        /// <param name="other">The GameObject supplying the dividend; its Transform is read without a null check.</param>
        /// <returns>The per-axis ratio of the two world scales, which is 1 where both objects are equally scaled.</returns>
        public static Vector3 GetTargetLocalScale([NotNull] this GameObject obj, [NotNull] GameObject other) =>
            obj.transform.GetTargetLocalScale(other.transform);

        /// <summary>
        ///     Reads the component at the given index of the GameObject's component list.
        /// </summary>
        /// <param name="obj">The GameObject to inspect.</param>
        /// <param name="index">Zero-based index into the component list; negative or past the end fails the call.</param>
        /// <param name="component">The component found, or null when the call returns false.</param>
        /// <returns>True when the index was valid.</returns>
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

        /// <summary>
        ///     Component at the given index of the GameObject.
        /// </summary>
        /// <param name="obj">The GameObject to inspect.</param>
        /// <param name="index">Zero-based index; out-of-range values yield null instead of an exception.</param>
        /// <returns>The component at that index, or null when the index is out of range.</returns>
        [CanBeNull]
        public static Component GetComponentAtIndexOrDefault([NotNull] this GameObject obj, int index) =>
            obj.TryGetComponentAtIndex(index, out var component) ? component : null;

        /// <summary>
        ///     Every component of the GameObject in component order. The result is not filtered by type, so it can hold
        ///     several components of the same type.
        /// </summary>
        /// <param name="obj">The GameObject to read.</param>
        /// <returns>A new array with one entry per component.</returns>
        public static Component[] GetComponents([NotNull] this GameObject obj)
        {
            var count = obj.GetComponentCount();
            var ret = new Component[count];

            for (var i = 0; i < count; i++)
                ret[i] = obj.GetComponentAtIndex(i);

            return ret;
        }

#if !COMPILER_UDONSHARP
        /// <summary>
        ///     Calls <paramref name="componentAction" /> for every component of the GameObject. The component count is read
        ///     once before the loop, so components added or destroyed inside the callback only affect the remaining indices.
        /// </summary>
        /// <param name="obj">The GameObject to iterate.</param>
        /// <param name="componentAction">Invoked with (index, component) for each component, in component order.</param>
        public static void ComponentsForeach([NotNull] this GameObject obj, Action<int, Component> componentAction)
        {
            var count = obj.GetComponentCount();

            for (var i = 0; i < count; i++)
                componentAction(i, obj.GetComponentAtIndex(i));
        }

        /// <summary>
        ///     Projects every component of the GameObject through <paramref name="selector" />, keeping component order. The
        ///     array length is the component count read before the loop.
        /// </summary>
        /// <param name="obj">The GameObject to iterate.</param>
        /// <param name="selector">Invoked with (index, component) for each component and returns its replacement.</param>
        /// <returns>A new array with one projected value per component.</returns>
        public static T[] ComponentsSelect<T>([NotNull] this GameObject obj, Func<int, Component, T> selector)
        {
            var count = obj.GetComponentCount();
            var ret = new T[count];

            for (var i = 0; i < count; i++)
                ret[i] = selector(i, obj.GetComponentAtIndex(i));

            return ret;
        }
#endif

        /// <summary>
        ///     True when the GameObject belongs to a loaded scene and the editor is not in prefab isolation mode. The prefab
        ///     check reads the global editor state rather than this object, and the result is always false in UdonSharp
        ///     builds.
        /// </summary>
        /// <param name="obj">The GameObject whose scene is validated.</param>
        /// <returns>True outside prefab mode for an object that belongs to a valid loaded scene.</returns>
#if !COMPILER_UDONSHARP
        public static bool IsSceneObject([NotNull] this GameObject obj) => obj.scene.IsValid() && !Utils.IsInPrefab;
#else
        public static bool IsSceneObject([NotNull] this GameObject obj) => false;
#endif

#if UTILS_UDONSHARP
        /// <summary>
        ///     VRChat only. Compares the local player's world position with this GameObject's pivot on all three axes.
        /// </summary>
        /// <param name="obj">The GameObject whose pivot is measured from.</param>
        /// <param name="distance">Threshold in Unity units; the check is strictly less than this value.</param>
        /// <returns>True when the player is closer than the threshold, false also when no valid local player exists.</returns>
        public static bool IsPlayerCloseRange([NotNull] this GameObject obj, float distance) =>
            obj.transform.IsPlayerCloseRange(distance);

        /// <summary>
        ///     VRChat only. Compares the local player's world position with this GameObject's pivot on the XZ plane, so
        ///     height differences are ignored.
        /// </summary>
        /// <param name="obj">The GameObject whose pivot is measured from.</param>
        /// <param name="distance">Threshold in Unity units on the XZ plane; the check is strictly less than this value.</param>
        /// <returns>True when the player is closer than the threshold, false also when no valid local player exists.</returns>
        public static bool IsPlayerCloseRange2D([NotNull] this GameObject obj, float distance) =>
            obj.transform.IsPlayerCloseRange2D(distance);
#endif
    }
}