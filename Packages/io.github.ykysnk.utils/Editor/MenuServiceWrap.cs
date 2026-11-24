using System;
using System.Reflection;
using io.github.ykysnk.utils.NonUdon;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Reflection wrapper over the internal <c>UnityEditor.MenuService</c> class. Those members are not public API,
    ///     so Unity can rename or remove them in any version.
    /// </summary>
    [PublicAPI]
    public static class MenuServiceWrap
    {
        private static readonly Type Type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.MenuService");

        /// <summary>
        ///     Runs Unity's own menu path cleanup, so a path added at runtime is registered exactly as Unity would
        ///     register it from a <c>MenuItem</c> attribute.
        /// </summary>
        /// <param name="menuName">Menu path as written, e.g. <c>Tools/My Tool/Item</c>.</param>
        /// <returns>The path Unity registers the item under.</returns>
        public static string SanitizeMenuItemName(string menuName) =>
            ReflectionWrapper.GetFunc<string, string>(Type, nameof(SanitizeMenuItemName))(menuName);

        /// <summary>
        ///     Asks Unity whether a method is usable as a menu command, using the same rules it applies to
        ///     <c>MenuItem</c> methods.
        /// </summary>
        /// <param name="methodInfo">Method to check.</param>
        /// <returns><c>true</c> when Unity would accept the method as a menu command.</returns>
        public static bool ValidateMethodForMenuCommand(MethodInfo methodInfo) =>
            ReflectionWrapper.GetFunc<MethodInfo, bool>(Type, nameof(ValidateMethodForMenuCommand))(methodInfo);
    }
}