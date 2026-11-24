using System;
using io.github.ykysnk.utils.NonUdon;
using io.github.ykysnk.utils.NonUdon.Extensions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Base class for custom inspectors that runs the drawing code inside a try/catch and shows a consistent error
    ///     UI when it throws, for both IMGUI and UI Toolkit inspectors.
    /// </summary>
    [PublicAPI]
    public abstract class BasicEditor : UnityEditor.Editor
    {
        /// <summary>Receives an exception that escaped the inspector body.</summary>
        public delegate void ErrorEvent(Exception e, Type type);

        /// <summary>Which inspector technology failed.</summary>
        public enum Type
        {
            /// <summary>The classic IMGUI inspector.</summary>
            UGUI,

            /// <summary>The UI Toolkit inspector built in <see cref="CreateInspectorGUI" />.</summary>
            UIToolkit
        }

        /// <summary>
        ///     Whether <see cref="OnInspectorGUI" /> should draw Unity's default inspector before the custom body.
        /// </summary>
        protected virtual bool IsBaseOnOldInspectorGUI => false;

        /// <summary>
        ///     Whether caught exceptions are also written to the console with <c>Debug.LogException</c>. The error UI is
        ///     shown either way.
        /// </summary>
        protected virtual bool ConsoleLog => false;

        private const float ErrorDetailsHeight = 140f;

        private static GUIStyle? _errorDetailsStyle;

        private Vector2 _errorScrollPosition;

        private bool _errorDetailsExpanded = true;

        // ReSharper disable once Unity.RedundantEventFunction
        /// <summary>Unity callback. Override to initialise the editor; the body is not guarded.</summary>
        protected virtual void OnEnable()
        {
        }

        /// <summary>
        ///     Raised after an exception was caught and the error UI was drawn, for tooling that wants to report it.
        /// </summary>
        public static event ErrorEvent? OnErrorEvent;

        /// <summary>
        ///     Draws <see cref="OnErrorHandleInspectorGUI" /> and applies modified properties. When the body throws, the
        ///     exception is reported through <see cref="OnError" /> / <see cref="OnErrorEvent" /> and the IMGUI error UI
        ///     is drawn.
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

        /// <summary>
        ///     Builds <see cref="CreateErrorHandleInspectorGUI" />. When it throws, the exception is reported and the UI
        ///     Toolkit error UI (<c>BasicEditorErrorUI.uxml</c>) is returned instead.
        /// </summary>
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
                var errorType = tree.Q<Label>("errorType");
                if (errorType != null)
                {
                    errorType.text = e.GetType().Name;
                    errorType.tooltip = e.GetType().FullName;
                }

                var detailsFoldout = tree.Q<Foldout>("detailsFoldout");
                if (detailsFoldout != null)
                    detailsFoldout.value = true;

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

        /// <summary>The copyable error text: the message followed by the original stack trace.</summary>
        protected static string ErrorDetails(Exception e) => $"{e.Message}\n{e.StackTrace}";

        /// <summary>
        ///     Draws the IMGUI error UI. It mirrors <c>BasicEditorErrorUI.uxml</c>: the message alone in a HelpBox, a
        ///     header row with the exception type and a button that copies the details, and the details themselves in a
        ///     scrollable, selectable box that grows with its content.
        /// </summary>
        protected virtual void DrawErrorUI(Exception e)
        {
            EditorGUILayout.HelpBox(ErrorTitle(e), MessageType.Error);

            var details = ErrorDetails(e);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(e.GetType().Name, EditorStyles.miniBoldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Copy", EditorStyles.miniButton, GUILayout.Width(60)))
                    EditorGUIUtility.systemCopyBuffer = details;
            }

            _errorDetailsExpanded = EditorGUILayout.Foldout(_errorDetailsExpanded, "Details", true);
            if (!_errorDetailsExpanded) return;

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

        /// <summary>Error UI used when <c>BasicEditorErrorUI.uxml</c> itself cannot be loaded.</summary>
        public static VisualElement CreateUxmlImportErrorUI() =>
            CreateErrorUI("Failed to load uxml assets, please reimport the package to fix this issue.");

        /// <summary>Builds a UI Toolkit help box with an informational message.</summary>
        public static VisualElement CreateInfoUI(string errorMessage) => CreateHelpUI(errorMessage);

        /// <summary>Builds a UI Toolkit help box with an error message.</summary>
        public static VisualElement CreateErrorUI(string errorMessage) =>
            CreateHelpUI(errorMessage, HelpBoxMessageType.Error);

        /// <summary>Builds a UI Toolkit help box with a warning message.</summary>
        public static VisualElement CreateWarningUI(string errorMessage) =>
            CreateHelpUI(errorMessage, HelpBoxMessageType.Warning);

        /// <summary>Builds a one-element visual tree holding a <see cref="HelpBox" />.</summary>
        public static VisualElement CreateHelpUI(string message, HelpBoxMessageType type = HelpBoxMessageType.Info)
        {
            var tree = new VisualElement();
            tree.Add(new HelpBox(message, type));
            return tree;
        }

        /// <summary>
        ///     Called after the inspector body reported a change and before the modified properties are applied.
        /// </summary>
        protected virtual void OnChange()
        {
        }

        /// <summary>
        ///     Called for every exception caught from the inspector body, before the error UI is drawn. Override to add
        ///     custom logging or recovery.
        /// </summary>
        /// <param name="e">The caught exception.</param>
        /// <param name="type">The inspector technology that threw.</param>
        protected virtual void OnError(Exception e, Type type)
        {
        }

        /// <summary>
        ///     Draws the IMGUI inspector body. Override this instead of <see cref="OnInspectorGUI" />: exceptions are
        ///     caught and replaced by the error UI.
        /// </summary>
        protected virtual void OnErrorHandleInspectorGUI()
        {
        }

        /// <summary>
        ///     Builds the UI Toolkit inspector body. Override this: exceptions are caught and replaced by the error UI.
        /// </summary>
        protected virtual VisualElement? CreateErrorHandleInspectorGUI() => null;
    }
}