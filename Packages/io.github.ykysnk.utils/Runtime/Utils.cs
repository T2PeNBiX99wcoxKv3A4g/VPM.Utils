using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;
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

        public static void Log([NotNull] string prefix, [CanBeNull] object message) =>
            Debug.Log($"[<color={LOGNameColor}>{prefix}</color>] {message}");

        public static void Log([NotNull] string prefix, [CanBeNull] object message, [NotNull] Object context) =>
            Debug.Log($"[<color={LOGNameColor}>{prefix}</color>] {message}", context);

        public static void LogWarning([NotNull] string prefix, [CanBeNull] object message) =>
            Debug.LogWarning($"[<color={LOGNameColor}>{prefix}</color>] {message}");

        public static void LogWarning([NotNull] string prefix, [CanBeNull] object message, [NotNull] Object context) =>
            Debug.LogWarning($"[<color={LOGNameColor}>{prefix}</color>] {message}", context);

        public static void LogError([NotNull] string prefix, [CanBeNull] object message) =>
            Debug.LogError($"[<color={LOGNameColor}>{prefix}</color>] {message}");

        public static void LogError([NotNull] string prefix, [CanBeNull] object message, [NotNull] Object context) =>
            Debug.LogError($"[<color={LOGNameColor}>{prefix}</color>] {message}", context);

        public static void LogAssert(bool condition, [NotNull] string prefix, [CanBeNull] object message) =>
            Debug.Assert(condition, $"[<color={LOGNameColor}>{prefix}</color>] {message}");

        public static void LogAssert(bool condition, [NotNull] string prefix, [CanBeNull] object message,
            [NotNull] Object context) =>
            Debug.Assert(condition, $"[<color={LOGNameColor}>{prefix}</color>] {message}", context);

        private const float MinSpeed = 0.05f;

        public static bool IsMoved(float value) => value < -MinSpeed && value > MinSpeed;
    }
}