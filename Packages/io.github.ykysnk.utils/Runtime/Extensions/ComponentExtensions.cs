using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class ComponentExtensions
    {
        [CanBeNull]
        public static string FullName([NotNull] this Component component) => component.transform.FullName();

        public static bool
            TryGetComponentAtIndex([NotNull] this Component component, int index, [CanBeNull] out Component component2) =>
            component.gameObject.TryGetComponentAtIndex(index, out component2);

        public static Component GetComponentAtIndexOrDefault([NotNull] this Component component, int index) =>
            component.TryGetComponentAtIndex(index, out var component2) ? component2 : null;

        public static Component[] GetComponents([NotNull] this Component component) =>
            component.gameObject.GetComponents();

#if !COMPILER_UDONSHARP
        public static void ComponentsForeach([NotNull] this Component component,
            GameObjectExtensions.ComponentAction componentAction) =>
            component.gameObject.ComponentsForeach(componentAction);

        public static T[] ComponentsSelect<T>([NotNull] this Component component, GameObjectExtensions.ComponentSelect<T> selector) =>
            component.gameObject.ComponentsSelect(selector);
#endif
    }
}