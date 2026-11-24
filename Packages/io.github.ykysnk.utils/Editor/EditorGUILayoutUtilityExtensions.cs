using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Lets label content be created from a string, so a tooltip can be attached without spelling out
    ///     <see cref="EditorGUILayoutUtility.Label" />.
    /// </summary>
    [PublicAPI]
    public static class EditorGUILayoutUtilityExtensions
    {
        /// <summary>
        ///     Creates label content for this text, with an optional tooltip.
        /// </summary>
        /// <param name="text">Text to show; the extension target.</param>
        /// <param name="textTip">Tooltip shown on hover; <c>null</c> leaves the content without one.</param>
        /// <returns>Content holding the text and, when given, the tooltip.</returns>
        public static GUIContent Label(this string text, string? textTip = null) =>
            EditorGUILayoutUtility.Label(text, textTip);
    }
}