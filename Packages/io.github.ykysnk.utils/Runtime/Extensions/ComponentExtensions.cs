using System;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Extensions
{
    /// <summary>
    ///     Component versions of the GameObject and Transform helpers, forwarding everything to the GameObject that owns
    ///     the component. The exact behaviour is documented on <see cref="GameObjectExtensions" /> and
    ///     <see cref="TransformExtensions" />.
    /// </summary>
    [PublicAPI]
    public static class ComponentExtensions
    {
        /// <summary>
        ///     Full hierarchy path of the GameObject that owns the component, for example "Root/Child/Name", without a
        ///     leading slash.
        /// </summary>
        /// <param name="component">The component whose GameObject is used; the component type is irrelevant.</param>
        /// <returns>The path built from the GameObject's Transform up to the root of the hierarchy.</returns>
        public static string FullName([NotNull] this Component component) => component.transform.FullName();

        /// <summary>
        ///     Reads the component stored at the given index of the owning GameObject's component list, in the same order
        ///     that <see cref="GameObjectExtensions.GetComponents(GameObject)" /> returns.
        /// </summary>
        /// <param name="component">The component whose GameObject is searched.</param>
        /// <param name="index">Zero-based index into the component list; negative or past the end fails the call.</param>
        /// <param name="component2">The component found, or null when the call returns false.</param>
        /// <returns>True when the index was valid.</returns>
        public static bool
            TryGetComponentAtIndex([NotNull] this Component component, int index, [CanBeNull] out Component component2) =>
            component.gameObject.TryGetComponentAtIndex(index, out component2);

        /// <summary>
        ///     Component at the given index of the owning GameObject.
        /// </summary>
        /// <param name="component">The component whose GameObject is searched.</param>
        /// <param name="index">Zero-based index; out-of-range values yield null instead of an exception.</param>
        /// <returns>The component at that index, or null when the index is out of range.</returns>
        public static Component GetComponentAtIndexOrDefault([NotNull] this Component component, int index) =>
            component.TryGetComponentAtIndex(index, out var component2) ? component2 : null;

        /// <summary>
        ///     Every component of the owning GameObject, in the GameObject's component order. The result is not filtered by
        ///     type, so it can hold several components of the same type.
        /// </summary>
        /// <param name="component">The component whose GameObject is read.</param>
        /// <returns>A new array with one entry per component of the GameObject.</returns>
        public static Component[] GetComponents([NotNull] this Component component) =>
            component.gameObject.GetComponents();

#if !COMPILER_UDONSHARP
        /// <summary>
        ///     Calls <paramref name="componentAction" /> with the index and component for every component of the owning
        ///     GameObject. The component count is read once before the loop, so components added or destroyed inside the
        ///     callback only affect the remaining indices.
        /// </summary>
        /// <param name="component">The component whose GameObject is iterated.</param>
        /// <param name="componentAction">Invoked with (index, component) for each component, in component order.</param>
        public static void ComponentsForeach([NotNull] this Component component,
            Action<int, Component> componentAction) =>
            component.gameObject.ComponentsForeach(componentAction);

        /// <summary>
        ///     Projects every component of the owning GameObject through <paramref name="selector" />, keeping component
        ///     order. The array length is the component count read before the loop.
        /// </summary>
        /// <param name="component">The component whose GameObject is iterated.</param>
        /// <param name="selector">Invoked with (index, component) for each component and returns its replacement.</param>
        /// <returns>A new array with one projected value per component.</returns>
        public static T[] ComponentsSelect<T>([NotNull] this Component component, Func<int, Component, T> selector) =>
            component.gameObject.ComponentsSelect(selector);
#endif
    }
}