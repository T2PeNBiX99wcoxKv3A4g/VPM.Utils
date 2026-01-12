using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.utils.Editor;

[PublicAPI]
public static class EditorGUIUtils
{
    public static GUIContent IconContent(string name) =>
        EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_" + name : name);

    public static GUIContent IconContent(string text, string name) => new(text, IconTexture(name));

    public static Texture IconTexture(string name) => IconContent(name).image;
}