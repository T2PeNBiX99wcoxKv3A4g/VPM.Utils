using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Floating utility window with a custom body, an Ok button and an optional Cancel button, used by
    ///     <see cref="EditorUtils" />. It is not modal: the window reports its result through callbacks and closes
    ///     itself, and its text can be copied from the context menu.
    /// </summary>
    [PublicAPI]
    public class CustomDialogWindow : EditorWindow
    {
        private static CustomDialogWindow? _instance;
        [SerializeField] private VisualTreeAsset? uxml;

        private DialogContent _content = new MessageDialogContent("");
        private string _ok = "Ok";
        private string _cancel = "";
        private int _waitTime;
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
            var tree = uxml!.CloneTree();
            rootVisualElement.Add(tree);

            tree.Q<VisualElement>("content")?.Add(_content.CreateContent());

            var copyText = _content.CopyText;
            if (!string.IsNullOrEmpty(copyText))
                rootVisualElement.AddManipulator(new ContextualMenuManipulator(contextMenuEvent =>
                    contextMenuEvent.menu.AppendAction("Copy", _ => EditorGUIUtility.systemCopyBuffer = copyText)));

            var okButton = tree.Q<Button>("ok");
            okButton.text = _ok;

            // waitTime keeps Ok unusable for that many seconds, so a long message cannot be dismissed unread.
            if (_waitTime > 0)
            {
                okButton.AddToClassList("dialog-button-wait");
                okButton.schedule.Execute(() =>
                {
                    if (_waitTime > 0)
                    {
                        okButton.text = $"{_ok} ({_waitTime})";
                        _waitTime--;
                    }
                    else if (okButton.text != _ok)
                    {
                        okButton.text = _ok;
                    }
                }).Every(1000);
            }

            okButton.clicked += () =>
            {
                if (_waitTime > 0) return;
                _onOk?.Invoke();
                _isClosed = true;
                Close();
            };

            var cancelButton = tree.Q<Button>("cancel");
            cancelButton.text = _cancel;
            cancelButton.clicked += () =>
            {
                _onCancel?.Invoke();
                _isClosed = true;
                Close();
            };

            if (string.IsNullOrEmpty(_cancel))
                cancelButton.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     Builds the dialog, shows it as a utility window and centers it over the main editor window. A dialog that
        ///     is still open is closed first, and its pending result is reported as cancel.
        /// </summary>
        /// <param name="title">Window title, also used for the title bar icon.</param>
        /// <param name="content">Body of the dialog, built when the window opens.</param>
        /// <param name="ok">Caption of the confirm button.</param>
        /// <param name="cancel">Caption of the cancel button; an empty string hides the button.</param>
        /// <param name="waitTime">Seconds the Ok button stays disabled; zero disables the delay.</param>
        /// <param name="onOk">Invoked when Ok is pressed.</param>
        /// <param name="onCancel">Invoked when Cancel is pressed or the window is closed without a choice.</param>
        internal static void Show(string title, DialogContent content, string ok, string cancel, int waitTime,
            Action onOk, Action onCancel)
        {
            if (_instance != null)
                _instance.Close();

            _instance = CreateInstance<CustomDialogWindow>();
            _instance.titleContent = EditorGUIUtils.IconContent(title, "unityeditor.inspectorwindow");
            _instance._content = content;
            _instance._ok = ok;
            _instance._cancel = cancel;
            _instance._waitTime = waitTime;
            _instance._onOk = onOk;
            _instance._onCancel = onCancel;

            var size = content.Size;
            _instance.minSize = size;
            _instance.maxSize = size;
            _instance.ShowUtility();

            var main = EditorGUIUtility.GetMainWindowPosition();
            var x = main.x + (main.width - size.x) * 0.5f;
            var y = main.y + (main.height - size.y) * 0.5f;

            _instance.position = new(x, y, size.x, size.y);
        }
    }
}
