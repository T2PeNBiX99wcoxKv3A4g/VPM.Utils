using io.github.ykysnk.utils.NonUdon;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Draws a <see cref="ColorHex" /> as Unity's color swatch and writes the picked color back to the serialized hex
    ///     string too, so both fields stay in sync for edits made through the swatch.
    /// </summary>
    [CustomPropertyDrawer(typeof(ColorHex))]
    public class ColorHexDrawer : PropertyDrawer
    {
        private Color _color;

        /// <summary>
        ///     Draws the label followed by the color swatch. A change is written to both the <c>color</c> and
        ///     <c>hex</c> fields at once, in <c>#RRGGBBAA</c> form.
        /// </summary>
        /// <param name="position">Rect the inspector reserved for this field.</param>
        /// <param name="property">
        ///     Serialized <see cref="ColorHex" />; it must have <c>color</c> and <c>hex</c> children.
        /// </param>
        /// <param name="label">Label drawn in front of the swatch.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var colorProperty = property.FindPropertyRelative("color");
            var colorHexProperty = property.FindPropertyRelative("hex");

            EditorGUI.BeginChangeCheck();

            _color = EditorGUI.ColorField(position, colorProperty.colorValue);

            if (EditorGUI.EndChangeCheck())
            {
                colorProperty.colorValue = _color;
                colorHexProperty.stringValue = $"#{ColorUtility.ToHtmlStringRGBA(_color)}";
            }

            EditorGUI.EndProperty();
        }
    }
}