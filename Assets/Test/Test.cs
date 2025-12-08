using io.github.ykysnk.utils;
using io.github.ykysnk.utils.Extensions;
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

            var test = new Vector3(1, 2, 3);
            // Vector3.D
        }
    }
}