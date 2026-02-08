using System;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class FloatExtensions
    {
        /// <summary>
        ///     Returns the given float value, setting it to zero if its absolute value is less than the specified threshold.
        /// </summary>
        /// <param name="value">The float value to process.</param>
        /// <param name="threshold">
        ///     The threshold value below which the input value is set to zero. The default value is 0.0001f.
        /// </param>
        /// <returns>The processed float value, set to zero if its absolute value is below the threshold.</returns>
        public static float Clean(this float value, float threshold = 0.0001f) =>
            Mathf.Abs(value) < threshold ? 0f : value;

        /// <summary>
        ///     Determines whether the float value is nearly equal to the target value within a specified threshold.
        /// </summary>
        /// <param name="value">The float value to compare.</param>
        /// <param name="target">The target float value to compare against.</param>
        /// <param name="threshold">
        ///     The allowable threshold within which the two values are considered nearly equal. Defaults to 0.0001f.
        /// </param>
        /// <returns>
        ///     True if the absolute difference between the value and the target is less than the specified threshold;
        ///     otherwise, false.
        /// </returns>
        public static bool IsNearly(this float value, float target, float threshold = 0.0001f) =>
            Mathf.Abs(value - target) < threshold;

        /// <summary>
        ///     Determines whether the float value is nearly zero within a specified threshold.
        /// </summary>
        /// <param name="value">The float value to evaluate.</param>
        /// <param name="threshold">
        ///     The allowable threshold within which the value is considered nearly zero. Defaults to 0.0001f.
        /// </param>
        /// <returns>True if the absolute value of the float is less than the specified threshold; otherwise, false.</returns>
        public static bool IsNearlyZero(this float value, float threshold = 0.0001f) => value.IsNearly(0f, threshold);

        /// <summary>
        ///     Clamps the given float value between a specified minimum and maximum range.
        /// </summary>
        /// <param name="value">The float value to clamp.</param>
        /// <param name="min">The minimum allowable value.</param>
        /// <param name="max">The maximum allowable value.</param>
        /// <returns>The clamped float value, constrained within the specified range.</returns>
        public static float Clamp(this float value, float min, float max) => Mathf.Clamp(value, min, max);

        /// <summary>
        ///     Clamps the float value to the range [0, 1].
        /// </summary>
        /// <param name="value">The float value to clamp.</param>
        /// <returns>The clamped float value, constrained to the range [0, 1].</returns>
        public static float Clamp01(this float value) => Mathf.Clamp01(value);

        /// <summary>
        ///     Rounds the given float value to a specified number of fractional digits using the specified rounding mode.
        /// </summary>
        /// <param name="value">The float value to be rounded.</param>
        /// <param name="digits">The number of fractional digits to round to.</param>
        /// <param name="mode">The rounding mode to apply.</param>
        /// <returns>The rounded float value.</returns>
        public static float Round(this float value, int digits, MidpointRounding mode) =>
            MathF.Round(value, digits, mode);

        /// <summary>
        ///     Rounds the given float value to the specified number of fractional digits using the specified rounding mode.
        /// </summary>
        /// <param name="value">The float value to round.</param>
        /// <param name="digits">The number of fractional digits to round to. Must be between 0 and 15.</param>
        /// <returns>The rounded float value.</returns>
        public static float Round(this float value, int digits) => MathF.Round(value, digits);

        /// <summary>
        ///     Rounds the given float value to the nearest integral value using the specified rounding mode.
        /// </summary>
        /// <param name="value">The float value to round.</param>
        /// <param name="mode">The midpoint rounding strategy to use when rounding.</param>
        /// <returns>The rounded float value.</returns>
        public static float Round(this float value, MidpointRounding mode) => MathF.Round(value, mode);

        /// <summary>
        ///     Rounds the given float value to the nearest integer value.
        /// </summary>
        /// <param name="value">The float value to round.</param>
        /// <returns>The rounded integer value as a float.</returns>
        public static float Round(this float value) => MathF.Round(value);

        /// <summary>
        ///     Determines whether the specified float value is approximately equal to a target value,
        ///     using a margin of error based on the size of the values.
        /// </summary>
        /// <param name="value">The float value to compare.</param>
        /// <param name="target">The target float value to compare against.</param>
        /// <returns>True if the values are approximately equal; otherwise, false.</returns>
        public static bool Approximately(this float value, float target) => Mathf.Abs(target - value) <
                                                                            (double)Mathf.Max(
                                                                                1E-06f * Mathf.Max(Mathf.Abs(value),
                                                                                    Mathf.Abs(target)),
                                                                                Mathf.Epsilon * 8f);
    }
}