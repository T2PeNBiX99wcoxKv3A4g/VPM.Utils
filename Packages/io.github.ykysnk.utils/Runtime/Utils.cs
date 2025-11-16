using JetBrains.Annotations;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace io.github.ykysnk.utils
{
    [PublicAPI]
    public static class Utils
    {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public static bool IsInPrefab() => PrefabStageUtility.GetCurrentPrefabStage();
#else
        public static bool IsInPrefab() => false;
#endif
    }
}