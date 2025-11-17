using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEngine;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
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

        public static int ToLayer(LayerMask mask)
        {
            var bitmask = mask.value;
            var result = bitmask > 0 ? 0 : 31;

            while (bitmask > 1)
            {
                bitmask >>= 1;
                result++;
            }

            return result;
        }

        private const string LOGNameColor = "#D771C0";

        public static void Log(string prefix, object message) =>
            Debug.Log($"[<color={LOGNameColor}>{prefix}</color>] {message}");

        public static void LogWarning(string prefix, object message) =>
            Debug.LogWarning($"[<color={LOGNameColor}>{prefix}</color>] {message}");

        public static void LogError(string prefix, object message) =>
            Debug.LogError($"[<color={LOGNameColor}>{prefix}</color>] {message}");

        private const float MinSpeed = 0.05f;

        [SuppressMessage("ReSharper", "MergeIntoPattern")]
        public static bool IsMoved(float value) => value < -MinSpeed && value > MinSpeed;
    }
}