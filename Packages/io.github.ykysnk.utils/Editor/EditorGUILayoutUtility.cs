using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Cached label styles and layout helpers that the package's inspectors share.
    /// </summary>
    [PublicAPI]
    public static class EditorGUILayoutUtility
    {
        private static GUIStyle? _boldLabel;

        private static GUIStyle? _boldLabel2;

        /// <summary>
        ///     Bold label at 15pt for section headings. Created on first access, so it keeps the editor skin that was
        ///     active at that moment.
        /// </summary>
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

        /// <summary>
        ///     Smaller bold label at 12pt, for sub headings under <see cref="BoldLabel" />.
        /// </summary>
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

        /// <summary>
        ///     Creates label content from the text and an optional tooltip.
        /// </summary>
        /// <param name="text">Label text.</param>
        /// <param name="textTip">Tooltip shown on hover; <c>null</c> leaves the content without one.</param>
        /// <returns>Content that can be passed to any <see cref="EditorGUILayout" /> control.</returns>
        public static GUIContent Label(string text, string? textTip = null) => new(text, null, textTip);

        /// <summary>
        ///     Draws a horizontal rule in the given color, with half of <paramref name="space" /> above and half below
        ///     it, so the total vertical space is always <paramref name="space" />.
        /// </summary>
        /// <param name="color">Color of the rule.</param>
        /// <param name="space">Total vertical space to occupy, split evenly above and below the rule.</param>
        /// <param name="lineHeight">Height of the drawn rule in pixels.</param>
        public static void Line(Color color, float space = 20f, float lineHeight = 1f)
        {
            EditorGUILayout.BeginVertical();

            GUILayout.Space(space / 2);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, lineHeight), color);
            GUILayout.Space(space / 2);

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        ///     Draws a gray separator line; the shortcut used by most inspectors.
        /// </summary>
        /// <param name="space">Total vertical space to occupy, split evenly above and below the line.</param>
        /// <param name="lineHeight">Height of the drawn rule in pixels.</param>
        public static void Line(float space = 20f, float lineHeight = 1f) => Line(Color.gray, space, lineHeight);
    }
}