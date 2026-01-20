using System.Collections.Concurrent;
using JetBrains.Annotations;
using UnityEditor;

// #if UTILS_LILTOOLBOX
// using jp.lilxyzw.editortoolbox;
// #endif

namespace io.github.ykysnk.utils.Editor.Extensions
{
    [PublicAPI]
    [InitializeOnLoad]
    public static class MenuExtensions
    {
        private static readonly ConcurrentDictionary<string, MenuItemObject> MenuItemObjects = new();

// #if UTILS_LILTOOLBOX
//         private static readonly Type MenuItemModifierType =
//             typeof(CallerUtils).Assembly.GetType("jp.lilxyzw.editortoolbox.MenuItemModifier");
//
//         private static Dictionary<string, (MethodInfo, MenuItem)[]> MenuItems =>
//             ReflectionWrapper.GetFieldGetter<Dictionary<string, (MethodInfo, MenuItem)[]>>(MenuItemModifierType,
//                 "menuItems")();
// #endif

        static MenuExtensions()
        {
            // Using EditorApplication.hierarchyChanged because lilEditorToolbox only adds MenuItemAttribute methods when rebuilding all menu items.
            // AddMenuItem can add menu item at any time.
            // lilEditorToolbox MenuItemModifier will not affect this
            EditorApplication.hierarchyChanged -= UpdateMenuItemEvent;
            EditorApplication.hierarchyChanged += UpdateMenuItemEvent;
// #if UTILS_LILTOOLBOX
//             EditorApplication.delayCall -= UpdateLilToolboxMenuItemEvent;
//             EditorApplication.delayCall += UpdateLilToolboxMenuItemEvent;
// #endif
        }

// #if UTILS_LILTOOLBOX
//         private static void UpdateLilToolboxMenuItemEvent()
//         {
//             var merged = new Dictionary<string, (MethodInfo, MenuItem)[]>();
//
//             var nonValidateItems = MenuItemObjects.Values.GroupBy(x => x.Name)
//                 .ToDictionary(x => x.Key,
//                     x => x.Select(y => (y.Execute.Method,
//                         new MenuItem(string.IsNullOrEmpty(y.Shortcut) ? y.Name : $"{y.Name} {y.Shortcut}", false,
//                             y.Priority))).ToArray());
//
//             var validateItems = MenuItemObjects.Values.GroupBy(x => x.Name)
//                 .ToDictionary(x => x.Key,
//                     x => x.Select(y => (y.Validate?.Method,
//                         new MenuItem(string.IsNullOrEmpty(y.Shortcut) ? y.Name : $"{y.Name} {y.Shortcut}", true,
//                             y.Priority))).Where(z => z.Method != null).Cast<(MethodInfo Method, MenuItem)>().ToArray());
//
//             foreach (var pair in nonValidateItems)
//                 merged[pair.Key] = pair.Value;
//
//             foreach (var pair in validateItems)
//             {
//                 if (merged.TryGetValue(pair.Key, out var existing))
//                     merged[pair.Key] = existing.Union(pair.Value).ToArray();
//                 else
//                     merged[pair.Key] = pair.Value;
//             }
//
//             foreach (var pair in merged)
//             {
//                 if (MenuItems.TryGetValue(pair.Key, out var existing))
//                     MenuItems[pair.Key] = existing.Union(pair.Value).ToArray();
//                 else
//                     MenuItems[pair.Key] = pair.Value;
//             }
//         }
// #endif

        private static void UpdateMenuItemEvent()
        {
            foreach (var menuItemObject in MenuItemObjects.Values)
                MenuWrap.AddMenuItem(menuItemObject.Name, menuItemObject.Shortcut, menuItemObject.Checked,
                    menuItemObject.Priority, menuItemObject.Execute, menuItemObject.Validate);

            EditorUtilityWrap.Internal_UpdateAllMenus();
        }

        public static bool AddMenuItem(MenuItemObject menuItemObject) =>
            MenuItemObjects.TryAdd(menuItemObject.Name, menuItemObject);

        // kinda pointless
        public static MenuItemObject? RemoveMenuItem(string name) =>
            MenuItemObjects.TryRemove(name, out var item) ? item : null;
    }
}