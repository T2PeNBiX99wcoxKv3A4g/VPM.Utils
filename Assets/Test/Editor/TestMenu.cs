using io.github.ykysnk.utils;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Editor.Extensions;
using UnityEditor;

namespace Test.Editor
{
    [InitializeOnLoad]
    public static class TestMenu
    {
        static TestMenu()
        {
            EditorApplication.hierarchyChanged -= TestAdd;
            EditorApplication.hierarchyChanged += TestAdd;

            var testMenuItem = new MenuItemObject("Test3/Test", () => Utils.Log(nameof(TestMenu), "Test"));
            MenuExtensions.AddMenuItem(testMenuItem);
        }

        [MenuItem("coco/temp")]
        private static void TempMenu()
        {
        }

        [MenuItem("Test/TestAdd")]
        private static void TestAdd()
        {
            MenuWrap.AddMenuItem("Test2/TestAdd", "", false, 100,
                () => Utils.Log(nameof(TestMenu), "TestAdded"), null);
            MenuWrap.AddMenuItem("Test2/TestAdd2", "", false, 111,
                () => Utils.Log(nameof(TestMenu), "TestAdded"), null);
            EditorUtilityWrap.Internal_UpdateAllMenus();
        }
    }
}