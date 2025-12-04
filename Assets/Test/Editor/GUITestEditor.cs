using io.github.ykysnk.utils.Editor;
using UnityEditor;

namespace Test.Editor
{
    [CustomEditor(typeof(GUITest))]
    public class GUITestEditor : BasicEditor
    {
        protected override void OnErrorHandleInspectorGUI()
        {
            throw new("test");
        }
    }
}