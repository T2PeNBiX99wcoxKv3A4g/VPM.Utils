using System;
using io.github.ykysnk.utils.NonUdon;
using JetBrains.Annotations;
using UnityEditor;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Reflection wrapper over the internal parts of <c>UnityEditor.Menu</c>, the class behind the editor menu bar.
    ///     Those members are undocumented editor internals and not public API, so Unity can rename or remove them in any
    ///     version; this wrapper is what lets menu items be added, changed and removed at runtime.
    /// </summary>
    [PublicAPI]
    public static class MenuWrap
    {
        private static readonly Type Type = typeof(Menu);

        /// <summary>
        ///     Assigns the shortcut shown next to an existing menu item.
        /// </summary>
        /// <param name="menuPath">Full menu path of the item, e.g. <c>Tools/My Tool/Item</c>.</param>
        /// <param name="hotkey">Shortcut string such as <c>#%g</c>.</param>
        public static void SetHotkey(string menuPath, string hotkey) =>
            ReflectionWrapper.GetAction<string, string>(Type, nameof(SetHotkey))(menuPath, hotkey);

        /// <summary>
        ///     Lists the sub menus that exist below the given path.
        /// </summary>
        /// <param name="menuPath">Menu path to inspect, e.g. <c>Tools</c>.</param>
        /// <returns>Sub menu entries below the path; empty when it has none.</returns>
        public static string[] ExtractSubmenus(string menuPath) =>
            ReflectionWrapper.GetFunc<string, string[]>(Type, nameof(ExtractSubmenus))(menuPath);

        /// <summary>
        ///     Registers a menu item at runtime, without a <c>MenuItem</c> attribute.
        /// </summary>
        /// <param name="name">Menu path of the new item.</param>
        /// <param name="shortcut">Shortcut string such as <c>#%g</c>; an empty string means none.</param>
        /// <param name="checked">Whether the item is drawn with a checkmark.</param>
        /// <param name="priority">Sort order within the parent menu; lower values come first.</param>
        /// <param name="execute">Action run when the item is chosen.</param>
        /// <param name="validate">
        ///     Predicate deciding whether the item is enabled; <c>null</c> means always enabled.
        /// </param>
        public static void AddMenuItem(string name, string shortcut, bool @checked, int priority, Action execute,
            Func<bool>? validate) =>
            ReflectionWrapper.GetAction<string, string, bool, int, Action, Func<bool>?>(Type, nameof(AddMenuItem))(name,
                shortcut, @checked, priority, execute, validate);

        /// <summary>
        ///     Adds a separator entry at the given path and priority.
        /// </summary>
        /// <param name="name">Menu path the separator belongs to.</param>
        /// <param name="priority">Sort order within the parent menu; lower values come first.</param>
        public static void AddSeparator(string name, int priority) =>
            ReflectionWrapper.GetAction<string, int>(Type, nameof(AddSeparator))(name, priority);

        /// <summary>
        ///     Removes the menu item registered under the given path.
        /// </summary>
        /// <param name="name">Menu path of the item to remove.</param>
        public static void RemoveMenuItem(string name) =>
            ReflectionWrapper.GetAction<string>(Type, nameof(RemoveMenuItem))(name);

        /// <summary>
        ///     Rebuilds the whole menu bar from the registered items.
        /// </summary>
        public static void RebuildAllMenus() => ReflectionWrapper.GetAction(Type, nameof(RebuildAllMenus))();

        /// <summary>
        ///     Checks whether a menu item is currently registered under the given path.
        /// </summary>
        /// <param name="menuPath">Menu path to look for.</param>
        /// <returns><c>true</c> when an item with that path exists.</returns>
        public static bool MenuItemExists(string menuPath) =>
            ReflectionWrapper.GetFunc<string, bool>(Type, nameof(MenuItemExists))(menuPath);
    }
}