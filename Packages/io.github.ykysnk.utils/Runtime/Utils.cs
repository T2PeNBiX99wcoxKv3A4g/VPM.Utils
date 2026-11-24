using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace io.github.ykysnk.utils
{
    /// <summary>
    ///     Small helpers that are safe to call from Udon: logging with a coloured prefix and a few state checks.
    /// </summary>
    [PublicAPI]
    public static class Utils
    {
        private const string LOGNameColor = "#D771C0";
        private const float MinSpeed = 0.05f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _isInPrefab()
        {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
            return PrefabStageUtility.GetCurrentPrefabStage();
#else
            return false;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _isPlaying()
        {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
            return EditorApplication.isPlayingOrWillChangePlaymode;
#else
            return true;
#endif
        }

        /// <summary>Whether the editor is currently in prefab editing mode. Always false in the player.</summary>
        public static bool IsInPrefab2() => _isInPrefab();

        /// <summary>Whether play mode is active or about to start. Always true in the player.</summary>
        public static bool IsPlaying2() => _isPlaying();

        /// <summary>
        ///     Returns the layer index of a single-layer mask, i.e. the position of its highest set bit. A mask of
        ///     zero returns 31, which is what the bit trick below produces.
        /// </summary>
        /// <param name="mask">The mask to read; only single-layer masks give a meaningful result.</param>
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

        /// <summary>Logs a message prefixed with <paramref name="prefix" />. The prefix frame is hidden from the stack.</summary>
        /// <param name="prefix">Short name shown as the coloured <c>[prefix]</c> part of the message.</param>
        /// <param name="message">Value appended to the prefix; null logs an empty message.</param>
        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log([NotNull] string prefix, [CanBeNull] object message) =>
            Debug.Log($"[<color={LOGNameColor}>{prefix}</color>] {message}");

        /// <inheritdoc cref="Log(string, object)" />
        /// <param name="context">Object Unity highlights when the console entry is clicked.</param>
        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log([NotNull] string prefix, [CanBeNull] object message, [NotNull] Object context) =>
            Debug.Log($"[<color={LOGNameColor}>{prefix}</color>] {message}", context);

        /// <inheritdoc cref="Log(string, object)" />
        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarning([NotNull] string prefix, [CanBeNull] object message) =>
            Debug.LogWarning($"[<color={LOGNameColor}>{prefix}</color>] {message}");

        /// <inheritdoc cref="Log(string, object)" />
        /// <param name="context">Object Unity highlights when the console entry is clicked.</param>
        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarning([NotNull] string prefix, [CanBeNull] object message, [NotNull] Object context) =>
            Debug.LogWarning($"[<color={LOGNameColor}>{prefix}</color>] {message}", context);

        /// <inheritdoc cref="Log(string, object)" />
        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError([NotNull] string prefix, [CanBeNull] object message) =>
            Debug.LogError($"[<color={LOGNameColor}>{prefix}</color>] {message}");

        /// <inheritdoc cref="Log(string, object)" />
        /// <param name="context">Object Unity highlights when the console entry is clicked.</param>
        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError([NotNull] string prefix, [CanBeNull] object message, [NotNull] Object context) =>
            Debug.LogError($"[<color={LOGNameColor}>{prefix}</color>] {message}", context);

        /// <summary>Kept for compatibility, use <see cref="Assert(bool, string, object)" /> instead.</summary>
        /// <param name="condition">Condition that should hold.</param>
        /// <param name="prefix">Short name shown as the coloured <c>[prefix]</c> part of the message.</param>
        /// <param name="message">Value appended to the prefix when the assertion fails.</param>
        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use Assert(bool condition, string prefix, object message) instead")]
        public static void LogAssert(bool condition, [NotNull] string prefix, [CanBeNull] object message) =>
            Debug.Assert(condition, $"[<color={LOGNameColor}>{prefix}</color>] {message}");

        /// <summary>Kept for compatibility, use <see cref="Assert(bool, string, object, Object)" /> instead.</summary>
        /// <param name="condition">Condition that should hold.</param>
        /// <param name="prefix">Short name shown as the coloured <c>[prefix]</c> part of the message.</param>
        /// <param name="message">Value appended to the prefix when the assertion fails.</param>
        /// <param name="context">Object Unity highlights when the console entry is clicked.</param>
        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use Assert(bool condition, string prefix, object message, Object context) instead")]
        public static void LogAssert(bool condition, [NotNull] string prefix, [CanBeNull] object message,
            [NotNull] Object context) =>
            Debug.Assert(condition, $"[<color={LOGNameColor}>{prefix}</color>] {message}", context);

        /// <summary>
        ///     Asserts <paramref name="condition" /> and reports the message with the coloured prefix when it fails.
        /// </summary>
        /// <param name="condition">Condition that should hold.</param>
        /// <param name="prefix">Short name shown as the coloured <c>[prefix]</c> part of the message.</param>
        /// <param name="message">Value appended to the prefix when the assertion fails.</param>
        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Assert(bool condition, [NotNull] string prefix, [CanBeNull] object message) =>
            Debug.Assert(condition, $"[<color={LOGNameColor}>{prefix}</color>] {message}");

        /// <inheritdoc cref="Assert(bool, string, object)" />
        /// <param name="context">Object Unity highlights when the console entry is clicked.</param>
        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Assert(bool condition, [NotNull] string prefix, [CanBeNull] object message,
            [NotNull] Object context) =>
            Debug.Assert(condition, $"[<color={LOGNameColor}>{prefix}</color>] {message}", context);

        /// <summary>
        ///     Whether a per-frame delta is large enough to count as movement, i.e. it exceeds
        ///     <c>MinSpeed</c> in either direction.
        /// </summary>
        /// <param name="value">Delta to test, normally a change in position or speed for this frame.</param>
        public static bool IsMoved(float value) => value < -MinSpeed || value > MinSpeed;

        // Not usable from Udon: `Scene` and `EditorApplication` are not exposed there, so the property is editor only.
#if !COMPILER_UDONSHARP
        /// <inheritdoc cref="IsInPrefab2" />
        public static bool IsInPrefab => _isInPrefab();

        /// <inheritdoc cref="IsPlaying2" />
        public static bool IsPlaying => _isPlaying();
#endif
    }
}
