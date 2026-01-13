using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.utils.Editor
{
    [PublicAPI]
    public class CustomDialogWindow : EditorWindow
    {
        private const int Width = 500;
        private const int Height = 250;
        [SerializeField] private VisualTreeAsset? uxml;
        [SerializeField] private new string title = "UnTitled";
        [SerializeField] private string message = "UnMessage";
        [SerializeField] private string ok = "Ok";
        [SerializeField] private string cancel = "";

        private bool _isClosed;
        private Action? _onCancel;
        private Action? _onOk;

        private void OnDestroy()
        {
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

            var copyButton = tree.Q<Button>("copy");
            copyButton.clicked += () => EditorGUIUtility.systemCopyBuffer = message;
            var okButton = tree.Q<Button>("ok");
            okButton.clicked += () =>
            {
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

        internal static void Show(string title, string message, string ok, string cancel, Action onOk, Action onCancel)
        {
            var window = CreateInstance<CustomDialogWindow>();
            window.titleContent = EditorGUIUtils.IconContent(title, "unityeditor.inspectorwindow");
            window.title = title;
            window.message = message;
            window.ok = ok;
            window.cancel = cancel;
            window._onOk = onOk;
            window._onCancel = onCancel;
            window.minSize = new(Width, Height);
            window.maxSize = new(Width, Height);
            window.ShowUtility();

            var main = EditorGUIUtility.GetMainWindowPosition();
            var x = main.x + (main.width - Width) * 0.5f;
            var y = main.y + (main.height - Height) * 0.5f;

            window.position = new(x, y, Width, Height);
        }
    }
}