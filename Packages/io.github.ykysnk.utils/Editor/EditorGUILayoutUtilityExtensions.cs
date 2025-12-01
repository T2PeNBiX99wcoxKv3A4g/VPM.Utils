using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Editor;

[PublicAPI]
public static class EditorGUILayoutUtilityExtensions
{
    public static GUIContent Label(this string text, string? textTip = null) =>
        EditorGUILayoutUtility.Label(text, textTip);
}