using System;
using io.github.ykysnk.utils.NonUdon;
using io.github.ykysnk.utils.NonUdon.Extensions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.utils.Editor
{
    [PublicAPI]
    public abstract class BasicEditor : UnityEditor.Editor
    {
        public delegate void ErrorEvent(Exception e, Type type);

        public enum Type
        {
            UGUI,
            UIToolkit
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the inspector GUI should use the old rendering style.
        /// </summary>
        /// <remarks>
        ///     When set to <c>true</c>, the inspector GUI will be based on the old Unity Editor GUI.
        /// </remarks>
        protected virtual bool IsBaseOnOldInspectorGUI => false;

        /// <summary>
        ///     Gets or sets a value indicating whether detailed console logging is enabled.
        /// </summary>
        /// <remarks>
        ///     Enabling this property allows for verbose logging of runtime information, which can be useful for debugging
        ///     or understanding application behavior. When disabled, the console output will be hidden.
        /// </remarks>
        protected virtual bool ConsoleLog => false;

        private const float ErrorDetailsHeight = 140f;

        private static GUIStyle? _errorDetailsStyle;

        private Vector2 _errorScrollPosition;

        // ReSharper disable once Unity.RedundantEventFunction
        /// <summary>
        ///     Called when the script is enabled in the Unity Editor.
        ///     This method can be overridden in derived classes to perform additional initialization
        ///     logic or to set up resources needed by the custom editor.
        /// </summary>
        protected virtual void OnEnable()
        {
        }

        /// <summary>
        ///     Represents an event triggered when an error occurs during the execution of a custom editor process.
        /// </summary>
        /// <remarks>
        ///     This event allows subscribers to handle exceptions raised during the runtime of the custom editor.
        ///     It can be used for logging, displaying error messages, or executing custom recovery logic.
        /// </remarks>
        public static event ErrorEvent? OnErrorEvent;

        /// <summary>
        ///     Called to render and handle the custom inspector GUI for the associated object.
        ///     This method can be overridden in derived classes to implement specific logic for
        ///     displaying and managing the inspector interface of a custom editor.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (IsBaseOnOldInspectorGUI)
                base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            Try.Run(OnErrorHandleInspectorGUI).OnFailure(e =>
            {
                if (ConsoleLog)
                    Debug.LogException(e);
                OnError(e, Type.UGUI);
                OnErrorEvent?.Invoke(e, Type.UGUI);
                DrawErrorUI(e);
            });

            if (!EditorGUI.EndChangeCheck())
                return;
            OnChange();
            serializedObject.ApplyModifiedProperties();
        }

        public override VisualElement? CreateInspectorGUI()
        {
            return Try.Run(CreateErrorHandleInspectorGUI).GetOrElse(e =>
            {
                if (ConsoleLog)
                    Debug.LogException(e);
                OnError(e, Type.UIToolkit);
                OnErrorEvent?.Invoke(e, Type.UIToolkit);

                var errorUxml =
                    AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                        AssetDatabase.GUIDToAssetPath("041e01f1ddee3c640989a147c46c76a9"));

                if (errorUxml == null) return CreateUxmlImportErrorUI();

                var tree = errorUxml.CloneTree();
                var errorBox = tree.Q<HelpBox>("errorBox");
                errorBox.text = ErrorTitle(e);
                var errorDetails = tree.Q<TextField>("errorDetails");
                errorDetails.isReadOnly = true;
                errorDetails.value = ErrorDetails(e);
                var copy = tree.Q<Button>("copy");
                copy.clicked += () => EditorGUIUtility.systemCopyBuffer = errorDetails.value;
                return tree;
            });
        }

        private static GUIStyle ErrorDetailsStyle => _errorDetailsStyle ??= new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true,
            richText = false,
            padding = new RectOffset(6, 6, 4, 4),
            margin = new RectOffset(0, 0, 0, 0)
        };

        /// <summary>The one line title shown in the error UI.</summary>
        protected static string ErrorTitle(Exception e) => $"Editor Error: {e.Message}";

        /// <summary>The copyable error text: the message followed by the stack trace.</summary>
        protected static string ErrorDetails(Exception e) => $"{e.Message}\n{e.StackTrace}";

        /// <summary>
        ///     Draws the IMGUI error UI. It mirrors <c>BasicEditorErrorUI.uxml</c>: the message alone in a HelpBox,
        ///     a header row whose button copies the details, and the details themselves in a scrollable selectable box.
        /// </summary>
        protected virtual void DrawErrorUI(Exception e)
        {
            EditorGUILayout.HelpBox(ErrorTitle(e), MessageType.Error);

            var details = ErrorDetails(e);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Error details", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Copy", EditorStyles.miniButton, GUILayout.Width(60)))
                    EditorGUIUtility.systemCopyBuffer = details;
            }

            GUILayout.Space(2);

            // Grow with the content and only start scrolling once the details get long, so a short stack trace
            // does not leave an empty box behind. The width is a little conservative so the measured height
            // never comes out short of the wrapped text.
            var width = Mathf.Max(EditorGUIUtility.currentViewWidth - 46f, 50f);
            var contentHeight = Mathf.Max(ErrorDetailsStyle.CalcHeight(new GUIContent(details), width),
                ErrorDetailsStyle.lineHeight);
            var viewHeight = Mathf.Min(contentHeight + 8f, ErrorDetailsHeight);

            using (var scroll = new EditorGUILayout.ScrollViewScope(_errorScrollPosition, "box",
                       GUILayout.Height(viewHeight)))
            {
                _errorScrollPosition = scroll.scrollPosition;

                EditorGUILayout.SelectableLabel(details, ErrorDetailsStyle, GUILayout.Height(contentHeight),
                    GUILayout.ExpandWidth(true));
            }
        }

        public static VisualElement CreateUxmlImportErrorUI() =>
            CreateErrorUI("Failed to load uxml assets, please reimport the package to fix this issue.");

        public static VisualElement CreateInfoUI(string errorMessage) => CreateHelpUI(errorMessage);

        public static VisualElement CreateErrorUI(string errorMessage) =>
            CreateHelpUI(errorMessage, HelpBoxMessageType.Error);

        public static VisualElement CreateWarningUI(string errorMessage) =>
            CreateHelpUI(errorMessage, HelpBoxMessageType.Warning);

        public static VisualElement CreateHelpUI(string message, HelpBoxMessageType type = HelpBoxMessageType.Info)
        {
            var tree = new VisualElement();
            tree.Add(new HelpBox(message, type));
            return tree;
        }

        /// <summary>
        ///     Called when changes are detected in the custom editor's inspector.
        ///     This method can be overridden in derived classes to handle any logic
        ///     that should occur in response to user modifications in the inspector.
        /// </summary>
        protected virtual void OnChange()
        {
        }

        /// <summary>
        ///     Called when an exception occurs during the custom editor's OnInspectorGUI process.
        ///     This method can be overridden in derived classes to handle error logging, display custom
        ///     error messages, or execute additional recovery logic after an exception is thrown.
        /// </summary>
        /// <param name="e">The exception object that contains details about the error.</param>
        /// <param name="type">The type of ui type</param>
        protected virtual void OnError(Exception e, Type type)
        {
        }

        /// <summary>
        ///     Called to handle the drawing of the custom editor's inspector GUI.
        ///     This method can be overridden in derived classes to implement custom rendering logic,
        ///     allowing the editor to display and manage specific data or controls within the inspector.
        /// </summary>
        protected virtual void OnErrorHandleInspectorGUI()
        {
        }

        protected virtual VisualElement? CreateErrorHandleInspectorGUI() => null;
    }
}