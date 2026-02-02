using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Test.Editor
{
    [CustomEditor(typeof(Test))]
    public class TestEditor : BasicEditor
    {
        [SerializeField] private VisualTreeAsset uxml;

        protected override VisualElement CreateErrorHandleInspectorGUI()
        {
            var tree = uxml?.CloneTree();
            tree.Bind(serializedObject);
            return tree;
        }
    }
}