using io.github.ykysnk.utils.NonUdon;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.utils.Editor;

[CustomPropertyDrawer(typeof(BooleanVector3))]
public class BooleanVector3Drawer : PropertyDrawer
{
    private static readonly GUIContent[] SubLabels =
    {
        new("X"), new("Y"), new("Z")
    };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var propertyHeight = EditorGUI.GetPropertyHeight(SerializedPropertyType.Boolean, label);
        if (Screen.width < 332)
            position = EditorGUILayout.GetControlRect(true, propertyHeight);

        EditorGUI.MultiPropertyField(position, SubLabels,
            property.FindPropertyRelative("x"), GUIContent.none, EditorGUI.PropertyVisibility.OnlyVisible);
        EditorGUI.EndProperty();
    }
}