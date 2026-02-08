using io.github.ykysnk.utils;
using io.github.ykysnk.utils.Editor;
using io.github.ykysnk.utils.Editor.Extensions;
using io.github.ykysnk.utils.Extensions;
using io.github.ykysnk.utils.NonUdon;
using UnityEngine;

namespace Test
{
    [ExecuteInEditMode]
    public class Test : MonoBehaviour
    {
        [SerializeField] private GameObject target;
        [SerializeField] private BooleanVector3 testBoolVector3;

        [ContextMenu("Test")]
        private void TestMethod()
        {
            Utils.Log(nameof(TestMethod),
                $"Test: {transform.lossyScale} {transform.GetLocalScaleFollowWorldScale()} {transform.GetTargetLocalScale(target)}");

            var test = ReflectionWrapper.GetPropertyGetter<Transform, Vector3>("lossyScale");
            var test2 = test(transform);
            var x = 6.830189e-06f.Clean();

            var test3 = typeof(ReflectionWrapper);
        }

        [ContextMenu("CheckBoolVector3")]
        private void CheckBoolVector3()
        {
            Utils.Log(nameof(CheckBoolVector3), testBoolVector3.ToString());
        }

        [ContextMenu("TestUpmInstall")]
        private void TestUpmInstall()
        {
            UpmInstaller.UpdateAsync().WaitEditor(() => Utils.Log(nameof(TestUpmInstall), "Done"));
        }

        [ContextMenu("TestVector3")]
        private void TestVector3()
        {
            var test = new Vector3(1.999999999999999999999f, 2.123456789f, 3);
            var test2 = new Vector3(0.00001f, -0.00003f, 0.000099f);
            Utils.Log(nameof(TestVector3), $"1 {test.ToFullString()} {test2.ToFullString()}");
            test.CopyFrom(test2);
            Utils.Log(nameof(TestVector3), $"2 {test.ToFullString()} {test2.ToFullString()}");
            test2.CleanInPlace();
            Utils.Log(nameof(TestVector3), $"3 {test.ToFullString()} {test2.ToFullString()}");
        }

        [ContextMenu("TestTransform")]
        private void TestTransform()
        {
            transform.localPosition.Set(1, 2, 3);
        }

        [ContextMenu("TestTransform2")]
        private void TestTransform2()
        {
            transform.localPosition = new(1, 2, 3);
        }

        [ContextMenu("TestTransform3")]
        private void TestTransform3()
        {
            transform.SetLocalPositionAndRotation(new(1, 2, 3), transform.localRotation);
        }
    }
}