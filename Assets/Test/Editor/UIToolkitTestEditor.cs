using System;
using io.github.ykysnk.utils.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Test.Editor
{
    [CustomEditor(typeof(UIToolkitTest))]
    public class UIToolkitTestEditor : BasicEditor
    {
        [SerializeField] private VisualTreeAsset uxml;

        protected override VisualElement CreateErrorHandleInspectorGUI() => throw new ArgumentOutOfRangeException();
    }
}