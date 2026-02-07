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
            _instance.ok = ok;
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