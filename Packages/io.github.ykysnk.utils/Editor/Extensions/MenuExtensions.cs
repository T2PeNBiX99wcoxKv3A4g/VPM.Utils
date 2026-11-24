using System.Collections.Concurrent;
using JetBrains.Annotations;
using UnityEditor;

namespace io.github.ykysnk.utils.Editor.Extensions
{
    /// <summary>
    ///     Keeps menu items that were added at runtime registered while the editor rebuilds its menus.
    ///     <para>
    ///         lilEditorToolbox's <c>MenuItemModifier</c> only picks up methods carrying a <c>MenuItem</c> attribute when
    ///         it rebuilds every menu, so items added through <see cref="AddMenuItem" /> are re-added whenever the
    ///         hierarchy changes. Items registered by that modifier are not touched.
    ///     </para>
    /// </summary>
    [PublicAPI]
    [InitializeOnLoad]
    public static class MenuExtensions
    {
        private static readonly ConcurrentDictionary<string, MenuItemObject> MenuItemObjects = new();

        static MenuExtensions()
        {
            // Hierarchy changes are also what makes lilEditorToolbox rebuild the menus, so re-register there.
            EditorApplication.hierarchyChanged -= UpdateMenuItemEvent;
            EditorApplication.hierarchyChanged += UpdateMenuItemEvent;
        }

        private static void UpdateMenuItemEvent()
        {
            // Unity rebuilds menus on hierarchy changes; adding an item that already exists would register it again.
            foreach (var menuItemObject in MenuItemObjects.Values)
            {
                if (MenuWrap.MenuItemExists(menuItemObject.Name)) continue;

                MenuWrap.AddMenuItem(menuItemObject.Name, menuItemObject.Shortcut, menuItemObject.Checked,
                    menuItemObject.Priority, menuItemObject.Execute, menuItemObject.Validate);
            }

            EditorUtilityWrap.Internal_UpdateAllMenus();
        }

        /// <summary>
        ///     Keeps a menu item registered across menu rebuilds. Returns false when a menu item with the same path is
        ///     already registered.
        /// </summary>
        /// <param name="menuItemObject">The item to keep registered; its <c>Name</c> is the menu path and the key.</param>
        public static bool AddMenuItem(MenuItemObject menuItemObject) =>
            MenuItemObjects.TryAdd(menuItemObject.Name, menuItemObject);

        /// <summary>
        ///     Stops keeping the menu item with the given path registered and returns it. The menu entry itself is not
        ///     removed, this only prevents it from being added again after a rebuild.
        /// </summary>
        /// <param name="name">Menu path that was used as the key in <see cref="AddMenuItem" />.</param>
        public static MenuItemObject? RemoveMenuItem(string name) =>
            MenuItemObjects.TryRemove(name, out var item) ? item : null;
    }
}
