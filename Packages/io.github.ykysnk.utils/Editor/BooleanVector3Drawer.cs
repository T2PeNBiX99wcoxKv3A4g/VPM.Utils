using io.github.ykysnk.utils.NonUdon;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Draws a <see cref="BooleanVector3" /> as three X/Y/Z toggles on one row, the same way Unity draws a
    ///     <see cref="Vector3" />, by reusing the built-in multi property field.
    /// </summary>
    [CustomPropertyDrawer(typeof(BooleanVector3))]
    public class BooleanVector3Drawer : PropertyDrawer
    {
        private static readonly GUIContent[] SubLabels =
        {
            new("X"), new("Y"), new("Z")
        };

        /// <summary>
        ///     Draws the label followed by the three component toggles and writes edits back through the serialized
        ///     property.
        /// </summary>
        /// <param name="position">Rect the inspector reserved for this field.</param>
        /// <param name="property">Serialized <see cref="BooleanVector3" /> whose x, y and z children are edited.</param>
        /// <param name="label">Label drawn in front of the toggles.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Below roughly this width the prefixed rect cannot hold three toggles, so take the full control rect.
            var propertyHeight = EditorGUI.GetPropertyHeight(SerializedPropertyType.Boolean, label);
            if (EditorGUIUtility.currentViewWidth < 332)
                position = EditorGUILayout.GetControlRect(true, propertyHeight);

            EditorGUI.MultiPropertyField(position, SubLabels,
                property.FindPropertyRelative("x"), GUIContent.none, EditorGUI.PropertyVisibility.OnlyVisible);
            EditorGUI.EndProperty();
        }
    }
}