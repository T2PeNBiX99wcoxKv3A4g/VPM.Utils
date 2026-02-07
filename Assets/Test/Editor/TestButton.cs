using System.Threading.Tasks;
using io.github.ykysnk.utils;
using io.github.ykysnk.utils.Editor;
using UnityEditor;

namespace Test.Editor
{
    public static class TestButton
    {
        [MenuItem("Test/TestButton")]
        public static void Test()
        {
            _ = TestAsync();
        }

        private static async Task TestAsync()
        {
            if (await EditorUtils.DisplayDialogAsync(nameof(TestAsync), "testMessage", "okTest", "cancelTest"))
                Utils.Log(nameof(TestAsync), "ok");
            else
                Utils.Log(nameof(TestAsync), "cancel");
        }

        [MenuItem("Test/TestButton2")]
        public static void Test2()
        {
            _ = Test2Async();
        }

        private static async Task Test2Async()
        {
            if (await EditorUtils.DisplayDialogAsync(nameof(Test2Async),
                    "testMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2",
                    "okTest", "cancelTest"))
                Utils.Log(nameof(Test2Async), "ok");
            else
                Utils.Log(nameof(Test2Async), "cancel");
        }

        [MenuItem("Test/TestButton3")]
        public static void Test3()
        {
            EditorUtils.DisplayDialog(nameof(Test3), "test", onOk: () => Utils.Log(nameof(Test3), "ok"));
        }

        [MenuItem("Test/TestButton4")]
        public static void Test4()
        {
            EditorUtils.DisplayDialog(nameof(Test4),
                "testtesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttestt",
                onOk: () => Utils.Log(nameof(Test4), "ok"));
        }

        [MenuItem("Test/TestButton5")]
        public static void Test5()
        {
            _ = Test5Async();
        }

        private static async Task Test5Async()
        {
            if (await EditorUtils.DisplayDialogAsync(nameof(Test5Async), "testMessage", "okTest", "cancelTest", 10))
                Utils.Log(nameof(Test2Async), "ok");
            else
                Utils.Log(nameof(Test2Async), "cancel");
        }

        [MenuItem("Test/TestButton6")]
        public static void Test6()
        {
            EditorUtils.DisplayDialog(nameof(Test6), "testMessage", "okTest", "cancelTest",
                () => Utils.Log(nameof(Test6), "ok"),
                () => Utils.Log(nameof(Test6), "cancel"), 10);
        }
    }
}