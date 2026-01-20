using System;
using io.github.ykysnk.utils.NonUdon;
using JetBrains.Annotations;
using UnityEditor;

namespace io.github.ykysnk.utils.Editor
{
    [PublicAPI]
    public static class MenuWrap
    {
        private static readonly Type Type = typeof(Menu);

        public static void SetHotkey(string menuPath, string hotkey) =>
            ReflectionWrapper.GetAction<string, string>(Type, nameof(SetHotkey))(menuPath, hotkey);

        public static string[] ExtractSubmenus(string menuPath) =>
            ReflectionWrapper.GetFunc<string, string[]>(Type, nameof(ExtractSubmenus))(menuPath);

        public static void AddMenuItem(string name, string shortcut, bool @checked, int priority, Action execute,
            Func<bool>? validate) =>
            ReflectionWrapper.GetAction<string, string, bool, int, Action, Func<bool>?>(Type, nameof(AddMenuItem))(name,
                shortcut, @checked, priority, execute, validate);

        public static void AddSeparator(string name, int priority) =>
            ReflectionWrapper.GetAction<string, int>(Type, nameof(AddSeparator))(name, priority);

        public static void RemoveMenuItem(string name) =>
            ReflectionWrapper.GetAction<string>(Type, nameof(RemoveMenuItem))(name);

        public static void RebuildAllMenus() => ReflectionWrapper.GetAction(Type, nameof(RebuildAllMenus))();

        public static bool MenuItemExists(string menuPath) =>
            ReflectionWrapper.GetFunc<string, bool>(Type, nameof(MenuItemExists))(menuPath);
    }
}