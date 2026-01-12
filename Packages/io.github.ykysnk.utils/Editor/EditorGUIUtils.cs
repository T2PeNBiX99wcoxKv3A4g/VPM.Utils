using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.utils.Editor;

[PublicAPI]
public static class EditorGUIUtils
{
    public static GUIContent IconContent(string name) =>
        EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_" + name : name);
}