using System;
using System.Reflection;
using io.github.ykysnk.utils.NonUdon;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Editor
{
    [PublicAPI]
    public static class MenuServiceWrap
    {
        private static readonly Type Type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.MenuService");

        public static string SanitizeMenuItemName(string menuName) =>
            ReflectionWrapper.GetFunc<string, string>(Type, nameof(SanitizeMenuItemName))(menuName);

        public static bool ValidateMethodForMenuCommand(MethodInfo methodInfo) =>
            ReflectionWrapper.GetFunc<MethodInfo, bool>(Type, nameof(ValidateMethodForMenuCommand))(methodInfo);
    }
}