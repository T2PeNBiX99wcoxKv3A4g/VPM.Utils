using System.Collections.Generic;
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
            var test2 = Wrapper.Create(new List<bool>());
            var test3 = Wrapper.Create(new Dictionary<string, bool>());
            Utils.Log(nameof(CheckBoolVector3), testBoolVector3.ToString());
        }

        [ContextMenu("TestUpmInstall")]
        private void TestUpmInstall()
        {
            UpmInstaller.UpdateAsync().WaitEditor(() => Utils.Log(nameof(TestUpmInstall), "Done"));
        }
    }
}