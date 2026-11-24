using System;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class FloatExtensions
    {
        /// <summary>
        ///     Replaces the value with 0 when its absolute value is below <paramref name="threshold" />, which removes
        ///     floating-point noise. The comparison is strict, so a value exactly at the threshold is kept.
        /// </summary>
        /// <param name="value">The value to clean; it is returned unchanged unless it counts as noise.</param>
        /// <param name="threshold">
        ///     Magnitude below which the value counts as zero, in the value's units. Defaults to 0.0001f.
        /// </param>
        /// <returns>Either 0 or the original value.</returns>
        public static float Clean(this float value, float threshold = 0.0001f) =>
            Mathf.Abs(value) < threshold ? 0f : value;

        /// <summary>
        ///     Checks whether the value differs from <paramref name="target" /> by less than
        ///     <paramref name="threshold" />, using the absolute difference with a strict comparison.
        /// </summary>
        /// <param name="value">The value under test.</param>
        /// <param name="target">The value to compare against, in the same units as <paramref name="value" />.</param>
        /// <param name="threshold">
        ///     Largest accepted absolute difference, with a strict comparison. Defaults to 0.0001f.
        /// </param>
        /// <returns><c>true</c> when the two values are close enough; otherwise <c>false</c>.</returns>
        public static bool IsNearly(this float value, float target, float threshold = 0.0001f) =>
            Mathf.Abs(value - target) < threshold;

        /// <summary>
        ///     Checks whether the value's absolute value is below <paramref name="threshold" />, with a strict
        ///     comparison.
        /// </summary>
        /// <param name="value">The value under test.</param>
        /// <param name="threshold">
        ///     Magnitude below which the value counts as zero, in the value's units. Defaults to 0.0001f.
        /// </param>
        /// <returns><c>true</c> when the value is close enough to zero; otherwise <c>false</c>.</returns>
        public static bool IsNearlyZero(this float value, float threshold = 0.0001f) => value.IsNearly(0f, threshold);

        /// <summary>
        ///     Clamps the value into [<paramref name="min" />, <paramref name="max" />].
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">
        ///     Lower bound, in the same units as <paramref name="value" />; expected to be at or below
        ///     <paramref name="max" />.
        /// </param>
        /// <param name="max">
        ///     Upper bound, in the same units as <paramref name="value" />; must not be below <paramref name="min" />.
        /// </param>
        /// <returns>The clamped value.</returns>
        public static float Clamp(this float value, float min, float max) => Mathf.Clamp(value, min, max);

        /// <summary>
        ///     Clamps the value to the range [0, 1].
        /// </summary>
        /// <param name="value">The value to clamp; values outside [0, 1] are pulled back to the nearest end.</param>
        /// <returns>The clamped value.</returns>
        public static float Clamp01(this float value) => Mathf.Clamp01(value);

        /// <summary>
        ///     Rounds the value to <paramref name="digits" /> fractional digits, using <paramref name="mode" /> to
        ///     resolve midpoints such as 0.5.
        /// </summary>
        /// <param name="value">The value to round.</param>
        /// <param name="digits">
        ///     Fractional digits to keep, 0 to 15; any other value throws <see cref="ArgumentOutOfRangeException" />.
        /// </param>
        /// <param name="mode">
        ///     Midpoint rule, one of the <see cref="MidpointRounding" /> values such as
        ///     <see cref="MidpointRounding.ToEven" /> or <see cref="MidpointRounding.AwayFromZero" />.
        /// </param>
        /// <returns>The rounded value; the input is not modified.</returns>
        public static float Round(this float value, int digits, MidpointRounding mode) =>
            MathF.Round(value, digits, mode);

        /// <summary>
        ///     Rounds the value to <paramref name="digits" /> fractional digits, rounding midpoints to the nearest
        ///     even digit (<see cref="MidpointRounding.ToEven" />, so 0.5 becomes 0 and 1.5 becomes 2).
        /// </summary>
        /// <param name="value">The value to round.</param>
        /// <param name="digits">
        ///     Fractional digits to keep, 0 to 15; any other value throws <see cref="ArgumentOutOfRangeException" />.
        /// </param>
        /// <returns>The rounded value; the input is not modified.</returns>
        public static float Round(this float value, int digits) => MathF.Round(value, digits);

        /// <summary>
        ///     Rounds the value to the nearest whole number, using <paramref name="mode" /> to resolve midpoints.
        /// </summary>
        /// <param name="value">The value to round.</param>
        /// <param name="mode">Midpoint rule applied when the value is exactly between two whole numbers.</param>
        /// <returns>The rounded value, still typed as <c>float</c>.</returns>
        public static float Round(this float value, MidpointRounding mode) => MathF.Round(value, mode);

        /// <summary>
        ///     Rounds the value to the nearest whole number, rounding midpoints to the nearest even digit.
        /// </summary>
        /// <param name="value">The value to round; midpoints such as 0.5 and 1.5 go to the even side.</param>
        /// <returns>The rounded value, still typed as <c>float</c>.</returns>
        public static float Round(this float value) => MathF.Round(value);

        /// <summary>
        ///     Checks whether two values are approximately equal using a tolerance that grows with their magnitude:
        ///     1e-6 times the larger absolute value, but never less than <c>Mathf.Epsilon * 8</c>. Because that floor is
        ///     denormal-small, values near zero must match almost exactly; use <see cref="IsNearly" /> when an absolute
        ///     tolerance is needed.
        /// </summary>
        /// <param name="value">The value under test.</param>
        /// <param name="target">The value to compare against; only its magnitude affects the tolerance.</param>
        /// <returns><c>true</c> when the values are approximately equal; otherwise <c>false</c>.</returns>
        public static bool Approximately(this float value, float target) => Mathf.Abs(target - value) <
                                                                            (double)Mathf.Max(
                                                                                1E-06f * Mathf.Max(Mathf.Abs(value),
                                                                                    Mathf.Abs(target)),
                                                                                Mathf.Epsilon * 8f);
    }
}
