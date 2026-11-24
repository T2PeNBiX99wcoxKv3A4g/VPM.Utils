using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Builds editor content and icons that follow the active editor skin, so custom inspectors show the same
    ///     icons as built-in ones.
    /// </summary>
    [PublicAPI]
    public static class EditorGUIUtils
    {
        /// <summary>
        ///     Loads a built-in editor icon by name. On the dark skin the name is prefixed with <c>d_</c>, which is
        ///     where Unity keeps the light-for-dark variants.
        /// </summary>
        /// <param name="name">Icon name without the <c>d_</c> prefix, e.g. <c>console.infoicon</c>.</param>
        /// <returns>Content carrying the icon; its image is <c>null</c> when the name does not resolve.</returns>
        public static GUIContent IconContent(string name) =>
            EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_" + name : name);

        /// <summary>
        ///     Creates label content that shows the text next to a built-in editor icon.
        /// </summary>
        /// <param name="text">Text shown next to the icon.</param>
        /// <param name="name">Icon name without the <c>d_</c> prefix.</param>
        /// <returns>Content holding the text and the icon.</returns>
        public static GUIContent IconContent(string text, string name) => new(text, IconTexture(name));

        /// <summary>
        ///     Returns only the texture of a built-in editor icon, for callers that need the image itself.
        /// </summary>
        /// <param name="name">Icon name without the <c>d_</c> prefix.</param>
        /// <returns>Texture of the icon, or <c>null</c> when the name does not resolve.</returns>
        public static Texture IconTexture(string name) => IconContent(name).image;
    }
}