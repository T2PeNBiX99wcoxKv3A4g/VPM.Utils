using System;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class Vector3Extensions
    {
        /// <summary>
        ///     Returns the distance in world units between two points.
        /// </summary>
        /// <param name="a">The point this extension is called on, in world units.</param>
        /// <param name="b">The point to measure to, in world units.</param>
        /// <returns>The distance in world units, 0 when both points coincide.</returns>
        public static float Distance(this Vector3 a, Vector3 b) => Vector3.Distance(a, b);

        /// <summary>
        ///     Returns the distance in world units between a point and a Transform's world position.
        /// </summary>
        /// <param name="a">The point to measure from, in world units.</param>
        /// <param name="b">
        ///     The Transform to measure to; only its <c>position</c> is read, in world space. A null value throws
        ///     <see cref="NullReferenceException" />.
        /// </param>
        /// <returns>The distance in world units.</returns>
        public static float Distance(this Vector3 a, [NotNull] Transform b) => Vector3.Distance(a, b.position);

        /// <summary>
        ///     Returns the distance in world units between two points projected onto the XZ plane, so the Y axis is
        ///     ignored.
        /// </summary>
        /// <param name="a">The first point, in world units.</param>
        /// <param name="b">The second point, in world units.</param>
        /// <returns>The XZ distance in world units, 0 when both points share the same X and Z.</returns>
        public static float Distance2D(this Vector3 a, Vector3 b)
        {
            var differenceX = a.x - b.x;
            var differenceZ = a.z - b.z;
            return Mathf.Sqrt(differenceX * differenceX + differenceZ * differenceZ);
        }

        /// <summary>
        ///     Returns the distance in world units between a point and a Transform's world position, ignoring the Y axis.
        /// </summary>
        /// <param name="a">The point to measure from, in world units.</param>
        /// <param name="b">
        ///     The Transform to measure to; only its <c>position</c> is read, in world space. A null value throws
        ///     <see cref="NullReferenceException" />.
        /// </param>
        /// <returns>The XZ distance in world units.</returns>
        public static float Distance2D(this Vector3 a, [NotNull] Transform b) => a.Distance2D(b.position);

        /// <summary>
        ///     Divides each component of <paramref name="a" /> by the matching component of <paramref name="b" />.
        /// </summary>
        /// <param name="a">The dividend vector, one value per axis.</param>
        /// <param name="b">The divisor vector; a zero component yields infinity or NaN for that axis instead of throwing.</param>
        /// <returns>A new vector holding the component-wise quotients.</returns>
        public static Vector3 Divide(this Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

        /// <summary>
        ///     Multiplies each component of <paramref name="a" /> by the matching component of <paramref name="b" />.
        /// </summary>
        /// <param name="a">Vector supplying one factor per axis.</param>
        /// <param name="b">Vector supplying the other factor per axis.</param>
        /// <returns>A new vector holding the component-wise products.</returns>
        public static Vector3 Multiply(this Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);

        /// <summary>
        ///     Rounds each component to <paramref name="digits" /> fractional digits, using <paramref name="mode" /> to
        ///     resolve midpoints such as 0.5.
        /// </summary>
        /// <param name="a">The vector to round; its components are rounded independently.</param>
        /// <param name="digits">
        ///     Fractional digits to keep, 0 to 15; any other value throws <see cref="ArgumentOutOfRangeException" />.
        /// </param>
        /// <param name="mode">
        ///     Midpoint rule, one of the <see cref="MidpointRounding" /> values such as
        ///     <see cref="MidpointRounding.ToEven" /> or <see cref="MidpointRounding.AwayFromZero" />.
        /// </param>
        /// <returns>A new vector with every component rounded.</returns>
        public static Vector3 Round(this Vector3 a, int digits, MidpointRounding mode) =>
            new Vector3(MathF.Round(a.x, digits, mode), MathF.Round(a.y, digits, mode), MathF.Round(a.z, digits, mode));

        /// <summary>
        ///     Rounds each component to <paramref name="digits" /> fractional digits, rounding midpoints to the nearest
        ///     even digit (<see cref="MidpointRounding.ToEven" />, so 0.5 becomes 0 and 1.5 becomes 2).
        /// </summary>
        /// <param name="a">The vector to round; its components are rounded independently.</param>
        /// <param name="digits">
        ///     Fractional digits to keep, 0 to 15; any other value throws <see cref="ArgumentOutOfRangeException" />.
        /// </param>
        /// <returns>A new vector with every component rounded.</returns>
        public static Vector3 Round(this Vector3 a, int digits) => a.Round(digits, MidpointRounding.ToEven);

        /// <summary>
        ///     Rounds each component to the nearest whole number, using <paramref name="mode" /> to resolve midpoints.
        /// </summary>
        /// <param name="a">The vector to round; its components are rounded independently.</param>
        /// <param name="mode">Midpoint rule applied when a component is exactly between two whole numbers.</param>
        /// <returns>A new vector with every component rounded to an integer value.</returns>
        public static Vector3 Round(this Vector3 a, MidpointRounding mode) => a.Round(0, mode);

        /// <summary>
        ///     Rounds each component to the nearest whole number, rounding midpoints to the nearest even digit.
        /// </summary>
        /// <param name="a">The vector to round; its components are rounded independently.</param>
        /// <returns>A new vector with every component rounded to an integer value.</returns>
        public static Vector3 Round(this Vector3 a) => a.Round(0);

        /// <summary>
        ///     Returns, for each component, the shortest signed difference in degrees between that component and
        ///     <paramref name="current" />, wrapped into the range [-180, 180].
        /// </summary>
        /// <param name="a">Target angles in degrees, one per axis.</param>
        /// <param name="current">Reference angle in degrees, subtracted from every component of <paramref name="a" />.</param>
        /// <returns>A new vector of per-axis deltas in degrees, each within [-180, 180].</returns>
        public static Vector3 DeltaAngle(this Vector3 a, float current) => new Vector3(Mathf.DeltaAngle(current, a.x),
            Mathf.DeltaAngle(current, a.y), Mathf.DeltaAngle(current, a.z));

        /// <summary>
        ///     Returns, for each component, the shortest signed difference in degrees between that component and 0,
        ///     wrapped into the range [-180, 180].
        /// </summary>
        /// <param name="a">Angles to wrap in degrees, one per axis.</param>
        /// <returns>A new vector of per-axis deltas in degrees, each within [-180, 180].</returns>
        public static Vector3 DeltaAngle(this Vector3 a) => a.DeltaAngle(0);

        /// <summary>
        ///     Limits the magnitude of a vector to <paramref name="maxLength" />. Vectors already at or below that
        ///     magnitude are returned unchanged; longer ones are scaled down by multiplying <c>normalized</c> by
        ///     <paramref name="maxLength" />, which keeps their direction.
        /// </summary>
        /// <param name="vector">The vector whose magnitude is limited; direction is preserved.</param>
        /// <param name="maxLength">
        ///     The largest allowed magnitude, in world units. A negative value flips the direction of an over-long vector;
        ///     a zero vector stays <c>Vector3.zero</c>.
        /// </param>
        /// <returns>The original vector when it fits, otherwise a shortened copy of it.</returns>
        public static Vector3 ClampMagnitude(this Vector3 vector, float maxLength) =>
            vector.magnitude > maxLength ? vector.normalized * maxLength : vector;

        /// <summary>
        ///     Returns a new vector with the absolute value taken of each component.
        /// </summary>
        /// <param name="vector">The vector whose components are made non-negative.</param>
        /// <returns>A new vector whose components are all non-negative.</returns>
        public static Vector3 Abs(this Vector3 vector) =>
            new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));

        /// <summary>
        ///     Returns a new vector built from the smaller value of each pair of corresponding components.
        /// </summary>
        /// <param name="a">Vector contributing the first candidate value per axis.</param>
        /// <param name="b">Vector contributing the value it is compared against per axis.</param>
        /// <returns>The component-wise minimum, one value per axis.</returns>
        public static Vector3 Min(this Vector3 a, Vector3 b) =>
            new Vector3(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));

        /// <summary>
        ///     Returns a new vector built from the larger value of each pair of corresponding components.
        /// </summary>
        /// <param name="a">Vector contributing the first candidate value per axis.</param>
        /// <param name="b">Vector contributing the value it is compared against per axis.</param>
        /// <returns>The component-wise maximum, one value per axis.</returns>
        public static Vector3 Max(this Vector3 a, Vector3 b) =>
            new Vector3(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));

        /// <summary>
        ///     Clamps each component independently between the matching components of <paramref name="min" /> and
        ///     <paramref name="max" />.
        /// </summary>
        /// <param name="vector">The vector to clamp; its components are clamped independently.</param>
        /// <param name="min">
        ///     Lower bound per axis, in the vector's units; expected to be at or below the matching component of
        ///     <paramref name="max" />.
        /// </param>
        /// <param name="max">Upper bound per axis, in the vector's units; must not be below <paramref name="min" />.</param>
        /// <returns>A new vector with every component clamped into its bounds.</returns>
        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max) => new Vector3(
            Mathf.Clamp(vector.x, min.x, max.x), Mathf.Clamp(vector.y, min.y, max.y),
            Mathf.Clamp(vector.z, min.z, max.z));

        /// <summary>
        ///     Clamps each component to the range [0, 1].
        /// </summary>
        /// <param name="vector">The vector to clamp; its components are clamped independently.</param>
        /// <returns>A new vector with every component clamped to [0, 1].</returns>
        public static Vector3 Clamp01(this Vector3 vector) =>
            new Vector3(Mathf.Clamp01(vector.x), Mathf.Clamp01(vector.y), Mathf.Clamp01(vector.z));

        /// <summary>
        ///     Returns the normalized direction pointing from <paramref name="from" /> to <paramref name="to" />.
        /// </summary>
        /// <param name="from">The starting point, in world units.</param>
        /// <param name="to">The point the returned unit vector points at, in world units.</param>
        /// <returns>A unit vector, or <c>Vector3.zero</c> when both points are identical.</returns>
        public static Vector3 DirectionTo(this Vector3 from, Vector3 to) => (to - from).normalized;

        /// <summary>
        ///     Returns the normalized direction pointing from <paramref name="to" /> back to <paramref name="from" />,
        ///     the opposite of <see cref="DirectionTo" />.
        /// </summary>
        /// <param name="from">The point the returned unit vector points at, in world units.</param>
        /// <param name="to">The point the direction starts from, in world units.</param>
        /// <returns>A unit vector, or <c>Vector3.zero</c> when both points are identical.</returns>
        public static Vector3 InverseDirectionTo(this Vector3 from, Vector3 to) => (from - to).normalized;

        /// <summary>
        ///     Interpolates linearly from <paramref name="a" /> to <paramref name="b" /> without clamping
        ///     <paramref name="t" />, so values outside [0, 1] extrapolate along the same line.
        /// </summary>
        /// <param name="a">The vector returned when <paramref name="t" /> is 0.</param>
        /// <param name="b">The vector returned when <paramref name="t" /> is 1.</param>
        /// <param name="t">The interpolant, unclamped; below 0 or above 1 it extrapolates past either input.</param>
        /// <returns>The interpolated or extrapolated vector.</returns>
        public static Vector3 LerpUnclamped(this Vector3 a, Vector3 b, float t) => Vector3.LerpUnclamped(a, b, t);

        /// <summary>
        ///     Interpolates linearly from <paramref name="a" /> to <paramref name="b" />, clamping
        ///     <paramref name="t" /> to [0, 1] first so the result never passes either input.
        /// </summary>
        /// <param name="a">The vector returned when <paramref name="t" /> is 0 or below.</param>
        /// <param name="b">The vector returned when <paramref name="t" /> is 1 or above.</param>
        /// <param name="t">The interpolation factor, clamped to [0, 1] before use.</param>
        /// <returns>The vector at the clamped interpolation factor.</returns>
        public static Vector3 Lerp(this Vector3 a, Vector3 b, float t) => Vector3.Lerp(a, b, t);

        /// <summary>
        ///     Moves <paramref name="current" /> towards <paramref name="target" /> by at most
        ///     <paramref name="maxDistanceDelta" /> world units, never overshooting the target.
        /// </summary>
        /// <param name="current">The position to move from, in world units.</param>
        /// <param name="target">The destination, in world units.</param>
        /// <param name="maxDistanceDelta">
        ///     The largest step this call may take, in world units. Zero returns <paramref name="current" /> unchanged;
        ///     a negative value moves away from <paramref name="target" />.
        /// </param>
        /// <returns>The stepped position, equal to <paramref name="target" /> once it is within one step.</returns>
        public static Vector3 MoveTowards(this Vector3 current, Vector3 target, float maxDistanceDelta) =>
            Vector3.MoveTowards(current, target, maxDistanceDelta);

        /// <summary>
        ///     Rotates <paramref name="current" /> towards <paramref name="target" /> by at most
        ///     <paramref name="maxRadiansDelta" /> radians while changing the magnitude by at most
        ///     <paramref name="maxMagnitudeDelta" />.
        /// </summary>
        /// <param name="current">The vector to rotate; both its direction and its magnitude are stepped.</param>
        /// <param name="target">The vector to rotate towards; its direction and magnitude are the goals.</param>
        /// <param name="maxRadiansDelta">The maximum rotation for this call, in radians; 0 keeps the direction.</param>
        /// <param name="maxMagnitudeDelta">The maximum magnitude change for this call, in world units; 0 keeps the length.</param>
        /// <returns>The rotated vector, or <paramref name="target" /> when both limits are reached.</returns>
        public static Vector3 RotateTowards(this Vector3 current, Vector3 target, float maxRadiansDelta,
            float maxMagnitudeDelta) => Vector3.RotateTowards(current, target, maxRadiansDelta, maxMagnitudeDelta);

        /// <summary>
        ///     Projects a vector onto the plane that passes through the origin with the given normal, which the
        ///     implementation normalizes internally.
        /// </summary>
        /// <param name="vector">The vector to project; the component along the normal is removed.</param>
        /// <param name="planeNormal">
        ///     The normal of the plane, any non-zero direction in the vector's units; it is normalized before use, and a
        ///     zero normal leaves <paramref name="vector" /> unchanged.
        /// </param>
        /// <returns>The part of the vector lying in the plane.</returns>
        public static Vector3 ProjectOnPlane(this Vector3 vector, Vector3 planeNormal) =>
            Vector3.ProjectOnPlane(vector, planeNormal);

        /// <summary>
        ///     Projects a vector onto the line defined by <paramref name="onNormal" />, keeping only the component
        ///     parallel to that direction.
        /// </summary>
        /// <param name="vector">The vector to project.</param>
        /// <param name="onNormal">The direction of the target line; it does not need to be normalized.</param>
        /// <returns>The projection, or <c>Vector3.zero</c> when <paramref name="onNormal" /> is zero.</returns>
        public static Vector3 Project(this Vector3 vector, Vector3 onNormal) => Vector3.Project(vector, onNormal);

        /// <summary>
        ///     Reflects a vector off the plane defined by its normal.
        /// </summary>
        /// <param name="inDirection">The incident vector, normally pointing towards the surface.</param>
        /// <param name="inNormal">
        ///     The surface normal; it must be unit length, otherwise the reflection is scaled wrongly.
        /// </param>
        /// <returns>The reflected vector, with the same magnitude as <paramref name="inDirection" />.</returns>
        public static Vector3 Reflect(this Vector3 inDirection, Vector3 inNormal) =>
            Vector3.Reflect(inDirection, inNormal);

        /// <summary>
        ///     Returns a new vector with every component whose absolute value is below
        ///     <paramref name="threshold" /> replaced by 0. The comparison is strict, so a component exactly at the
        ///     threshold is kept.
        /// </summary>
        /// <param name="vector">The vector to clean; each component is tested on its own.</param>
        /// <param name="threshold">
        ///     Magnitude below which a component counts as noise, in the component's units. Defaults to 0.0001f.
        /// </param>
        /// <returns>A copy of the vector with noise-level components zeroed.</returns>
        public static Vector3 Clean(this Vector3 vector, float threshold = 0.0001f) =>
            new Vector3(vector.x.Clean(threshold), vector.y.Clean(threshold), vector.z.Clean(threshold));

        /// <summary>
        ///     Checks whether every component differs from the matching component of
        ///     <paramref name="target" /> by less than <paramref name="threshold" />, using absolute differences with
        ///     a strict comparison.
        /// </summary>
        /// <param name="vector">The vector under test.</param>
        /// <param name="target">The values compared against, component by component.</param>
        /// <param name="threshold">
        ///     Largest accepted absolute difference per component, with a strict comparison. Defaults to 0.0001f.
        /// </param>
        /// <returns><c>true</c> when all three components are close enough; otherwise <c>false</c>.</returns>
        public static bool IsNearly(this Vector3 vector, Vector3 target, float threshold = 0.0001f) =>
            vector.x.IsNearly(target.x, threshold) && vector.y.IsNearly(target.y, threshold) &&
            vector.z.IsNearly(target.z, threshold);

        /// <summary>
        ///     Checks whether every component has an absolute value below <paramref name="threshold" />, that is,
        ///     whether the vector is close enough to the origin.
        /// </summary>
        /// <param name="vector">The vector under test; each component is tested on its own.</param>
        /// <param name="threshold">
        ///     Magnitude below which a component counts as zero, with a strict comparison. Defaults to 0.0001f.
        /// </param>
        /// <returns><c>true</c> when all three components are noise-level; otherwise <c>false</c>.</returns>
        public static bool IsNearlyZero(this Vector3 vector, float threshold = 0.0001f) =>
            vector.x.IsNearlyZero(threshold) && vector.y.IsNearlyZero(threshold) && vector.z.IsNearlyZero(threshold);

        /// <summary>
        ///     Overwrites every component of <paramref name="self" /> with the value from
        ///     <paramref name="other" />. Since Vector3 is a struct, <paramref name="self" /> must be passed by
        ///     reference (<c>ref</c>) for the caller to observe the change.
        /// </summary>
        /// <param name="self">
        ///     The vector to write into. It is mutated in place, so it must be a <c>ref</c>-able variable such as a
        ///     field, array element or local, not a temporary value.
        /// </param>
        /// <param name="other">The vector whose components are read; it is left unchanged.</param>
        public static void CopyFrom(this ref Vector3 self, Vector3 other)
        {
            self.x = other.x;
            self.y = other.y;
            self.z = other.z;
        }

        /// <summary>
        ///     Zeroes, in place, every component of <paramref name="self" /> whose absolute value is below
        ///     <paramref name="threshold" />. <paramref name="self" /> must be passed by reference (<c>ref</c>).
        /// </summary>
        /// <param name="self">
        ///     The vector to modify. It is mutated in place, so it must be a <c>ref</c>-able variable such as a field,
        ///     array element or local, not a temporary value.
        /// </param>
        /// <param name="threshold">
        ///     Magnitude below which a component is zeroed, with a strict comparison. Defaults to 0.0001f.
        /// </param>
        public static void CleanInPlace(this ref Vector3 self, float threshold = 0.0001f) =>
            self.CopyFrom(self.Clean(threshold));

        /// <summary>
        ///     Formats the vector as <c>"(x, y, z)"</c>, writing each component with the round-trip format
        ///     <c>"R"</c> so parsing the text back yields the same float values.
        /// </summary>
        /// <param name="v">The vector to format; each component is written and formatted independently.</param>
        /// <returns>The parenthesized, comma-separated component list.</returns>
        public static string ToFullString(this Vector3 v) => $"({v.x:R}, {v.y:R}, {v.z:R})";

        /// <summary>
        ///     Checks whether all three components are approximately equal, using the relative epsilon of
        ///     <see cref="FloatExtensions.Approximately" />: 1e-6 times the larger magnitude, with a floor of
        ///     <c>Mathf.Epsilon * 8</c>.
        /// </summary>
        /// <param name="self">The vector under test.</param>
        /// <param name="other">The values compared against, component by component.</param>
        /// <returns><c>true</c> when every component pair is approximately equal; otherwise <c>false</c>.</returns>
        public static bool Approximately(this Vector3 self, Vector3 other) => self.x.Approximately(other.x) &&
                                                                              self.y.Approximately(other.y) &&
                                                                              self.z.Approximately(other.z);
    }
}
