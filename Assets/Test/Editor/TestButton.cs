using System.Threading;
using System.Threading.Tasks;
using io.github.ykysnk.utils;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Editor.Extensions;
using UnityEditor;

namespace Test.Editor
{
    public static class TestButton
    {
        [MenuItem("Test/TestButton")]
        public static void Test()
        {
            UpmInstaller.UpdateAsync(CancellationToken.None).WaitEditor(packages =>
            {
                Utils.Log(nameof(TestButton), $"Need Update: {string.Join(",", packages)}");
            });
        }

        [MenuItem("Test/TestButton2")]
        public static void Test2()
        {
            UpmInstaller.Install(new[]
            {
                "com.unity.memoryprofiler"
            });
        }

        [MenuItem("Test/TestButton3")]
        public static void Test3()
        {
            UpmInstaller.Remove(new[]
            {
                "com.unity.memoryprofiler"
            });
        }

        [MenuItem("Test/TestButton4")]
        public static void Test4()
        {
            UpmInstaller.Upgrade();
        }

        [MenuItem("Test/TestButton5")]
        public static void Test5()
        {
            _ = Test5Async();
        }

        private static async Task Test5Async()
        {
            if (await EditorUtils.DisplayDialogAsync("testTitle", "testMessage", "okTest", "cancelTest"))
                Utils.Log(nameof(TestButton), "ok");
            else
                Utils.Log(nameof(TestButton), "cancel");
        }

        [MenuItem("Test/TestButton6")]
        public static void Test6()
        {
            _ = Test6Async();
        }

        private static async Task Test6Async()
        {
            if (await EditorUtils.DisplayDialogAsync("testTitle2",
                    "testMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2\ntestMessage2",
                    "okTest", "cancelTest"))
                Utils.Log(nameof(TestButton), "ok");
            else
                Utils.Log(nameof(TestButton), "cancel");
        }

        [MenuItem("Test/TestButton7")]
        public static void Test7()
        {
            EditorUtils.DisplayDialog("test7", "test", onOk: () => Utils.Log(nameof(Test7), "ok"));
        }
    }
}