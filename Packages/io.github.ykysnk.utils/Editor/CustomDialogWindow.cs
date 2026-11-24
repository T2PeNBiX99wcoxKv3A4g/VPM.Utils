using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Floating utility window with a message, an Ok button and an optional Cancel button, used by
    ///     <see cref="EditorUtils" />. It is not modal: the window reports its result through callbacks and closes
    ///     itself, and its text can be copied from the context menu.
    /// </summary>
    [PublicAPI]
    public class CustomDialogWindow : EditorWindow
    {
        private const int Width = 500;
        private const int Height = 250;

        private static CustomDialogWindow? _instance;
        [SerializeField] private VisualTreeAsset? uxml;
        [SerializeField] private new string title = "UnTitled";
        [SerializeField] private string message = "UnMessage";
        [SerializeField] private string firstOk = "";
        [SerializeField] private string ok = "Ok";
        [SerializeField] private string cancel = "";
        [SerializeField] private int waitTime;
        private bool _isClosed;
        private Action? _onCancel;
        private Action? _onOk;

        private void OnDestroy()
        {
            // Closing without pressing Ok counts as a cancel, so the window's close button still reports a result.
            if (_isClosed) return;
            _onCancel?.Invoke();
        }

        private void CreateGUI()
        {
            var serializedObject = new SerializedObject(this);
            var tree = uxml?.CloneTree();
            tree.Bind(serializedObject);
            rootVisualElement.Add(tree);
            rootVisualElement.AddManipulator(new ContextualMenuManipulator(contextMenuEvent =>
                contextMenuEvent.menu.AppendAction("Copy", _ => EditorGUIUtility.systemCopyBuffer = message)));

            var okButton = tree.Q<Button>("ok");

            // waitTime keeps Ok unusable for that many seconds, so a long message cannot be dismissed unread.
            if (waitTime > 0)
            {
                okButton.AddToClassList("dialog-button-wait");
                okButton.schedule.Execute(() =>
                {
                    if (waitTime > 0)
                    {
                        okButton.text = $"{firstOk} ({waitTime})";
                        waitTime--;
                    }
                    else if (okButton.text != firstOk)
                        okButton.text = firstOk;
                }).Every(1000);
            }

            okButton.clicked += () =>
            {
                if (waitTime > 0) return;
                _onOk?.Invoke();
                _isClosed = true;
                Close();
            };
            var cancelButton = tree.Q<Button>("cancel");
            cancelButton.clicked += () =>
            {
                _onCancel?.Invoke();
                _isClosed = true;
                Close();
            };

            if (string.IsNullOrEmpty(cancel))
                cancelButton.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     Builds the dialog, shows it as a utility window and centers it over the main editor window. A dialog that
        ///     is still open is closed first, and its pending result is reported as cancel.
        /// </summary>
        /// <param name="title">Window title, also used for the title bar icon.</param>
        /// <param name="message">Text shown in the window and copied by the Copy context menu entry.</param>
        /// <param name="ok">Caption of the confirm button.</param>
        /// <param name="cancel">Caption of the cancel button; an empty string hides the button.</param>
        /// <param name="waitTime">Seconds the Ok button stays disabled; zero disables the delay.</param>
        /// <param name="onOk">Invoked when Ok is pressed.</param>
        /// <param name="onCancel">Invoked when Cancel is pressed or the window is closed without a choice.</param>
        internal static void Show(string title, string message, string ok, string cancel, int waitTime, Action onOk,
            Action onCancel)
        {
            if (_instance != null)
                _instance.Close();

            _instance = CreateInstance<CustomDialogWindow>();
            _instance.titleContent = EditorGUIUtils.IconContent(title, "unityeditor.inspectorwindow");
            _instance.title = title;
            _instance.message = message;
            _instance.firstOk = ok;
            _instance.ok = waitTime > 0 ? $"{ok} ({waitTime})" : ok;
            _instance.cancel = cancel;
            _instance.waitTime = waitTime;
            _instance._onOk = onOk;
            _instance._onCancel = onCancel;
            _instance.minSize = new(Width, Height);
            _instance.maxSize = new(Width, Height);
            _instance.ShowUtility();

            var main = EditorGUIUtility.GetMainWindowPosition();
            var x = main.x + (main.width - Width) * 0.5f;
            var y = main.y + (main.height - Height) * 0.5f;

            _instance.position = new(x, y, Width, Height);
        }
    }
}