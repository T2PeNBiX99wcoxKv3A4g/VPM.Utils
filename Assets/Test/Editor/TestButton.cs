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
    }
}