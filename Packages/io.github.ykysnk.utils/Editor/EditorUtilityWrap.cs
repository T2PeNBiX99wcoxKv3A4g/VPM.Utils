using System;
using io.github.ykysnk.utils.NonUdon;
using io.github.ykysnk.utils.NonUdon.ReflectionDelegate;
using JetBrains.Annotations;
using UnityEditor;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Reflection wrapper over the internal part of <see cref="EditorUtility" />. That member is not public API,
    ///     so Unity can rename or remove it in any version.
    /// </summary>
    [PublicAPI]
    public static class EditorUtilityWrap
    {
        private static readonly Type Type = typeof(EditorUtility);

        // ReSharper disable once InconsistentNaming
        /// <summary>
        ///     The internal <c>EditorUtility.Internal_UpdateAllMenus</c> method, which makes Unity rebuild its menus.
        ///     Needed after adding menu items at runtime.
        /// </summary>
        public static readonly WrapAction Internal_UpdateAllMenus =
            ReflectionWrapper.GetAction(Type, nameof(Internal_UpdateAllMenus));
    }
}