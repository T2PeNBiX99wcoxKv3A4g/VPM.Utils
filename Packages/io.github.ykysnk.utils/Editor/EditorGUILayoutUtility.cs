using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.utils.Editor;

[PublicAPI]
public static class EditorGUILayoutUtility
{
    private static readonly GUIContent LabelContent = new();

    private static GUIStyle? _boldLabel;

    private static GUIStyle? _boldLabel2;

    public static GUIStyle BoldLabel
    {
        get
        {
            _boldLabel ??= new(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 15
            };
            return _boldLabel;
        }
    }

    public static GUIStyle BoldLabel2
    {
        get
        {
            _boldLabel2 ??= new(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12
            };
            return _boldLabel2;
        }
    }

    public static GUIContent Label(string text, string? textTip = null)
    {
        var label = LabelContent;
        label.text = text;
        label.tooltip = textTip;

        return label;
    }

    public static void Line(Color color, float space = 20f, float lineHeight = 1f)
    {
        EditorGUILayout.BeginVertical();

        GUILayout.Space(space / 2);
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, lineHeight), color);
        GUILayout.Space(space / 2);

        EditorGUILayout.EndVertical();
    }

    public static void Line(float space = 20f, float lineHeight = 1f) => Line(Color.gray, space, lineHeight);
}