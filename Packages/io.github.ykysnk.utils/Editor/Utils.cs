using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.utils.Editor;

[PublicAPI]
public static class Utils
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
}