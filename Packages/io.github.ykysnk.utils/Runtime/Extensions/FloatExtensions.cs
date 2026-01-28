using System;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class FloatExtensions
    {
        public static float Clean(this float value, float threshold = 0.0001f) =>
            Mathf.Abs(value) < threshold ? 0f : value;

        public static bool IsNearly(this float value, float target, float threshold = 0.0001f) =>
            Mathf.Abs(value - target) < threshold;

        public static bool IsNearlyZero(this float value, float threshold = 0.0001f) => value.IsNearly(0f, threshold);

        public static float Clamp(this float value, float min, float max) => Mathf.Clamp(value, min, max);

        public static float Clamp01(this float value) => Mathf.Clamp01(value);

        public static float Round(this float value, int digits, MidpointRounding mode) =>
            MathF.Round(value, digits, mode);

        public static float Round(this float value, int digits) => MathF.Round(value, digits);

        public static float Round(this float value, MidpointRounding mode) => MathF.Round(value, mode);

        public static float Round(this float value) => MathF.Round(value);
    }
}