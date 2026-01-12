using System.Threading;
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
    }
}