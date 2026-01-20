using System;
using io.github.ykysnk.utils.NonUdon;
using JetBrains.Annotations;
using UnityEditor;

namespace io.github.ykysnk.utils.Editor
{
    [PublicAPI]
    public static class EditorUtilityWrap
    {
        private static readonly Type Type = typeof(EditorUtility);

        // ReSharper disable once InconsistentNaming
        public static readonly ReflectionWrapper.WrapAction Internal_UpdateAllMenus =
            ReflectionWrapper.GetAction(Type, nameof(Internal_UpdateAllMenus));
    }
}