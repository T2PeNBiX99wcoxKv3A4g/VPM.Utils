using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Test.Editor
{
    [CustomEditor(typeof(UIToolkitTest))]
    public class UIToolkitTestEditor : BasicEditor
    {
        [SerializeField] private StyleSheet uss;
        [SerializeField] private VisualTreeAsset uxml;

        protected override VisualElement CreateErrorHandleInspectorGUI()
        {
            var root = new VisualElement();

            var visualTree = uxml.CloneTree();
            var errorBox = visualTree.Q<HelpBox>("errorBox");

            throw new("test");

            root.Add(visualTree);

            return root;
        }
    }
}