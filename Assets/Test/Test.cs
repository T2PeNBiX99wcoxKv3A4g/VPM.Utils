using io.github.ykysnk.utils;
using io.github.ykysnk.utils.Extensions;
using io.github.ykysnk.utils.NonUdon;
using UnityEngine;

namespace Test
{
    [ExecuteInEditMode]
    public class Test : MonoBehaviour
    {
        [SerializeField] private GameObject target;

        [ContextMenu("Test")]
        private void TestMethod()
        {
            Utils.Log(nameof(Test),
                $"Test: {transform.lossyScale} {transform.GetLocalScaleFollowWorldScale()} {transform.GetTargetLocalScale(target)}");

            var test = ReflectionWrapper.GetPropertyGetter<Transform, Vector3>("lossyScale");
            var test2 = test(transform);

            var test3 = typeof(ReflectionWrapper);
        }
    }
}