using System;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class Vector3Extensions
    {
        /// <summary>
        ///     Calculates the distance between two 3D points represented by Vector3 instances.
        /// </summary>
        /// <param name="a">The first point in 3D space.</param>
        /// <param name="b">The second point in 3D space.</param>
        /// <returns>The distance between the two points as a float.</returns>
        public static float Distance(this Vector3 a, Vector3 b) => Vector3.Distance(a, b);

        /// <summary>
        ///     Calculates the distance between a Vector3 instance and the position of a Transform.
        /// </summary>
        /// <param name="a">The Vector3 point in 3D space.</param>
        /// <param name="b">The Transform whose position will be used in the distance calculation.</param>
        /// <returns>The distance between the Vector3 point and the Transform's position as a float.</returns>
        public static float Distance(this Vector3 a, [NotNull] Transform b) => Vector3.Distance(a, b.position);

        /// <summary>
        ///     Calculates the 2D distance between two points in the XZ plane, represented by Vector3 instances.
        /// </summary>
        /// <param name="a">The first point in 3D space.</param>
        /// <param name="b">The second point in 3D space.</param>
        /// <returns>The 2D distance between the two points as a float.</returns>
        public static float Distance2D(this Vector3 a, Vector3 b)
        {
            var differenceX = a.x - b.x;
            var differenceZ = a.z - b.z;
            return Mathf.Sqrt(differenceX * differenceX + differenceZ * differenceZ);
        }

        /// <summary>
        ///     Calculates the 2D distance between two 3D points represented by Vector3 instances, ignoring the y-axis.
        /// </summary>
        /// <param name="a">The first point in 3D space.</param>
        /// <param name="b">The second point in 3D space.</param>
        /// <returns>The 2D distance between the points as a float.</returns>
        public static float Distance2D(this Vector3 a, [NotNull] Transform b) => a.Distance2D(b.position);

        /// <summary>
        ///     Divides the corresponding components of two Vector3 instances.
        /// </summary>
        /// <param name="a">The numerator Vector3.</param>
        /// <param name="b">
        ///     The denominator Vector3. Each component is used to divide the respective component of
        ///     <paramref name="a" />.
        /// </param>
        /// <returns>
        ///     A new Vector3 where each component is the result of dividing the corresponding components of
        ///     <paramref name="a" /> by <paramref name="b" />.
        /// </returns>
        public static Vector3 Divide(this Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

        /// <summary>
        ///     Multiplies the corresponding components of two Vector3 instances.
        /// </summary>
        /// <param name="a">The first Vector3 instance.</param>
        /// <param name="b">The second Vector3 instance.</param>
        /// <returns>
        ///     A Vector3 instance with each component being the product of the corresponding components from the input
        ///     vectors.
        /// </returns>
        public static Vector3 Multiply(this Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);

        /// <summary>
        ///     Rounds the components of a Vector3 to the specified number of fractional digits, using the specified rounding mode.
        /// </summary>
        /// <param name="a">The Vector3 whose components are to be rounded.</param>
        /// <param name="digits">The number of fractional digits to round to.</param>
        /// <param name="mode">The rounding mode to use when rounding each component.</param>
        /// <returns>A new Vector3 with components rounded to the specified number of fractional digits.</returns>
        public static Vector3 Round(this Vector3 a, int digits, MidpointRounding mode) =>
            new Vector3(MathF.Round(a.x, digits, mode), MathF.Round(a.y, digits, mode), MathF.Round(a.z, digits, mode));

        /// <summary>
        ///     Rounds the components of the Vector3 to the specified number of decimal places using the specified rounding mode.
        /// </summary>
        /// <param name="a">The Vector3 instance to round.</param>
        /// <param name="digits">The number of decimal places to round to.</param>
        /// <returns>A new Vector3 instance with each component rounded to the specified number of decimal places.</returns>
        public static Vector3 Round(this Vector3 a, int digits) => a.Round(digits, MidpointRounding.ToEven);

        /// <summary>
        ///     Rounds each component of the Vector3 to the specified number of decimal places using the specified rounding mode.
        /// </summary>
        /// <param name="a">The Vector3 instance whose components are to be rounded.</param>
        /// <param name="mode">The rounding mode to be used for rounding each component.</param>
        /// <returns>A new Vector3 instance with the rounded components.</returns>
        public static Vector3 Round(this Vector3 a, MidpointRounding mode) => a.Round(0, mode);

        /// <summary>
        ///     Rounds the components of the Vector3 to the nearest integral value based on the specified rounding mode and number
        ///     of fractional digits.
        /// </summary>
        /// <param name="a">The Vector3 instance to round.</param>
        /// <returns>A new Vector3 with each component rounded to the specified number of digits and rounding mode.</returns>
        public static Vector3 Round(this Vector3 a) => a.Round(0);

        /// <summary>
        ///     Calculates the delta angle between the components of the vector and a given angle.
        /// </summary>
        /// <param name="a">The vector whose components are used as target angles.</param>
        /// <param name="current">The current angle used as a reference for each component.</param>
        /// <returns>
        ///     A new vector with each component being the delta angle between the corresponding component of the vector and
        ///     the current angle.
        /// </returns>
        public static Vector3 DeltaAngle(this Vector3 a, float current) => new Vector3(Mathf.DeltaAngle(current, a.x),
            Mathf.DeltaAngle(current, a.y), Mathf.DeltaAngle(current, a.z));

        /// <summary>
        ///     Calculates the delta angle for each component of a Vector3 relative to a specified current value.
        /// </summary>
        /// <param name="a">The Vector3 instance representing the target angles.</param>
        /// <returns>A new Vector3 with the delta angles for each component (x, y, z).</returns>
        public static Vector3 DeltaAngle(this Vector3 a) => a.DeltaAngle(0);

        /// <summary>
        ///     Limits the size of a Vector3 instance to the specified maximum value.
        ///     If the size of the vector exceeds the maximum length, the vector is scaled to match the maximum length.
        /// </summary>
        /// <param name="vector">The Vector3 instance to be clamped.</param>
        /// <param name="maxLength">The maximum allowable magnitude.</param>
        /// <returns>A Vector3 with its magnitude clamped to the specified maximum value.</returns>
        public static Vector3 ClampMagnitude(this Vector3 vector, float maxLength) =>
            vector.magnitude > maxLength ? vector.normalized * maxLength : vector;

        /// <summary>
        ///     Returns a new Vector3 where each component of the input vector is replaced with its absolute value.
        /// </summary>
        /// <param name="vector">The input Vector3 whose components are to be processed.</param>
        /// <returns>
        ///     A new Vector3 with each component set to the absolute value of the corresponding component in the input
        ///     vector.
        /// </returns>
        public static Vector3 Abs(this Vector3 vector) =>
            new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));

        /// <summary>
        ///     Returns a new Vector3 where each component is the minimum value between the corresponding components of two Vector3
        ///     instances.
        /// </summary>
        /// <param name="a">The first Vector3 instance.</param>
        /// <param name="b">The second Vector3 instance.</param>
        /// <returns>
        ///     A Vector3 where each component is the smaller value between the corresponding components of the input vectors.
        /// </returns>
        public static Vector3 Min(this Vector3 a, Vector3 b) =>
            new Vector3(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));

        /// <summary>
        ///     Returns a new Vector3 instance where each component is the maximum of the corresponding components of the two input
        ///     Vector3 instances.
        /// </summary>
        /// <param name="a">The first Vector3 instance.</param>
        /// <param name="b">The second Vector3 instance.</param>
        /// <returns>A Vector3 containing the maximum values for each component from the two input vectors.</returns>
        public static Vector3 Max(this Vector3 a, Vector3 b) =>
            new Vector3(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));

        /// <summary>
        ///     Clamps the components of a Vector3 to be within the specified minimum and maximum bounds.
        /// </summary>
        /// <param name="vector">The Vector3 to be clamped.</param>
        /// <param name="min">The minimum Vector3 bounds.</param>
        /// <param name="max">The maximum Vector3 bounds.</param>
        /// <returns>A Vector3 with each component clamped between the corresponding components of the minimum and maximum bounds.</returns>
        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max) => new Vector3(
            Mathf.Clamp(vector.x, min.x, max.x), Mathf.Clamp(vector.y, min.y, max.y),
            Mathf.Clamp(vector.z, min.z, max.z));

        /// <summary>
        ///     Clamps the components of a Vector3 to the range [0, 1].
        /// </summary>
        /// <param name="vector">The Vector3 instance whose components will be clamped.</param>
        /// <returns>A new Vector3 where each component is clamped to the range [0, 1].</returns>
        public static Vector3 Clamp01(this Vector3 vector) =>
            new Vector3(Mathf.Clamp01(vector.x), Mathf.Clamp01(vector.y), Mathf.Clamp01(vector.z));

        /// <summary>
        ///     Calculates the direction vector from one 3D point to another, normalized to a unit vector.
        /// </summary>
        /// <param name="from">The starting point in 3D space.</param>
        /// <param name="to">The target point in 3D space.</param>
        /// <returns>A normalized Vector3 representing the direction from the starting point to the target point.</returns>
        public static Vector3 DirectionTo(this Vector3 from, Vector3 to) => (to - from).normalized;

        /// <summary>
        ///     Calculates the inverse direction vector from one 3D point to another, normalized to a unit vector.
        /// </summary>
        /// <param name="from">The starting point in 3D space.</param>
        /// <param name="to">The target point in 3D space.</param>
        /// <returns>A normalized Vector3 representing the direction from the target point to the starting point.</returns>
        public static Vector3 InverseDirectionTo(this Vector3 from, Vector3 to) => (from - to).normalized;

        /// <summary>
        ///     Interpolates linearly between two Vector3 points without clamping the interpolant.
        /// </summary>
        /// <param name="a">The start vector.</param>
        /// <param name="b">The end vector.</param>
        /// <param name="t">The interpolant value used for linear interpolation. Values outside the range [0, 1] are allowed.</param>
        /// <returns>A Vector3 that is the linear interpolation between the points, based on the interpolant value.</returns>
        public static Vector3 LerpUnclamped(this Vector3 a, Vector3 b, float t) => Vector3.LerpUnclamped(a, b, t);

        /// <summary>
        ///     Linearly interpolates between two vectors based on the given interpolation factor.
        /// </summary>
        /// <param name="a">The starting vector.</param>
        /// <param name="b">The target vector.</param>
        /// <param name="t">The interpolation factor, typically in the range [0, 1].</param>
        /// <returns>A vector that is the linear interpolation between the two input vectors.</returns>
        public static Vector3 Lerp(this Vector3 a, Vector3 b, float t) => Vector3.Lerp(a, b, t);

        /// <summary>
        ///     Moves a vector towards a target vector by a maximum distance delta.
        /// </summary>
        /// <param name="current">The starting vector position.</param>
        /// <param name="target">The target vector position.</param>
        /// <param name="maxDistanceDelta">The maximum distance the vector can move towards the target.</param>
        /// <returns>The moved vector position, which is closer to the target vector.</returns>
        public static Vector3 MoveTowards(this Vector3 current, Vector3 target, float maxDistanceDelta) =>
            Vector3.MoveTowards(current, target, maxDistanceDelta);

        /// <summary>
        ///     Rotates a vector towards a target vector by a specified maximum angular and magnitude distance.
        /// </summary>
        /// <param name="current">The starting vector to rotate from.</param>
        /// <param name="target">The target vector to rotate towards.</param>
        /// <param name="maxRadiansDelta">The maximum angular change in radians allowed during the rotation.</param>
        /// <param name="maxMagnitudeDelta">The maximum change in magnitude allowed during the rotation.</param>
        /// <returns>The resulting vector after applying the rotation towards the target.</returns>
        public static Vector3 RotateTowards(this Vector3 current, Vector3 target, float maxRadiansDelta,
            float maxMagnitudeDelta) => Vector3.RotateTowards(current, target, maxRadiansDelta, maxMagnitudeDelta);

        /// <summary>
        ///     Projects a vector onto a plane defined by its normal.
        /// </summary>
        /// <param name="vector">The vector to be projected onto the plane.</param>
        /// <param name="planeNormal">The normal vector defining the plane.</param>
        /// <returns>The projected vector lying on the plane.</returns>
        public static Vector3 ProjectOnPlane(this Vector3 vector, Vector3 planeNormal) =>
            Vector3.ProjectOnPlane(vector, planeNormal);

        /// <summary>
        ///     Projects a Vector3 onto another Vector3 that represents a normal vector.
        /// </summary>
        /// <param name="vector">The vector to be projected.</param>
        /// <param name="onNormal">The normal vector onto which the projection is made.</param>
        /// <returns>The projected vector as a Vector3.</returns>
        public static Vector3 Project(this Vector3 vector, Vector3 onNormal) => Vector3.Project(vector, onNormal);

        /// <summary>
        ///     Reflects a vector off the plane defined by a normal vector.
        /// </summary>
        /// <param name="inDirection">The incident vector to reflect.</param>
        /// <param name="inNormal">
        ///     The normal vector of the plane off which the vector is reflected. This vector must be
        ///     normalized.
        /// </param>
        /// <returns>The reflected vector as a new Vector3 instance.</returns>
        public static Vector3 Reflect(this Vector3 inDirection, Vector3 inNormal) =>
            Vector3.Reflect(inDirection, inNormal);

        /// <summary>
        ///     Returns a new Vector3 with components set to zero if their absolute values are less than the specified threshold.
        /// </summary>
        /// <param name="vector">The input Vector3 to process.</param>
        /// <param name="threshold">The threshold value below which components are set to zero. Defaults to 0.0001f.</param>
        /// <returns>A new Vector3 with cleaned components based on the threshold.</returns>
        public static Vector3 Clean(this Vector3 vector, float threshold = 0.0001f) =>
            new Vector3(vector.x.Clean(threshold), vector.y.Clean(threshold), vector.z.Clean(threshold));

        /// <summary>
        ///     Determines whether the current Vector3 is nearly equal to the target Vector3 within a specified threshold.
        /// </summary>
        /// <param name="vector">The current Vector3 to compare.</param>
        /// <param name="target">The target Vector3 to compare against.</param>
        /// <param name="threshold">
        ///     The allowable threshold within which the two vectors are considered nearly equal. Defaults to
        ///     0.0001f.
        /// </param>
        /// <returns>True if each corresponding component of the vectors is within the specified threshold; otherwise, false.</returns>
        public static bool IsNearly(this Vector3 vector, Vector3 target, float threshold = 0.0001f) =>
            vector.x.IsNearly(target.x, threshold) && vector.y.IsNearly(target.y, threshold) &&
            vector.z.IsNearly(target.z, threshold);

        /// <summary>
        ///     Determines whether all components of the specified Vector3 are nearly zero within a given threshold.
        /// </summary>
        /// <param name="vector">The Vector3 instance to evaluate.</param>
        /// <param name="threshold">
        ///     The threshold used to determine if the components are considered nearly zero. Defaults to
        ///     0.0001f.
        /// </param>
        /// <returns>True if all components of the vector are within the threshold of zero; otherwise, false.</returns>
        public static bool IsNearlyZero(this Vector3 vector, float threshold = 0.0001f) =>
            vector.x.IsNearlyZero(threshold) && vector.y.IsNearlyZero(threshold) && vector.z.IsNearlyZero(threshold);

        /// <summary>
        ///     Copies the values of the specified Vector3 instance to the current Vector3 instance.
        /// </summary>
        /// <param name="self">The Vector3 instance to copy the values to.</param>
        /// <param name="other">The Vector3 instance to copy the values from.</param>
        public static void CopyFrom(this ref Vector3 self, Vector3 other)
        {
            self.x = other.x;
            self.y = other.y;
            self.z = other.z;
        }

        /// <summary>
        ///     Modifies the current Vector3 instance by zeroing out components that are below the specified threshold.
        /// </summary>
        /// <param name="self">The Vector3 instance to modify.</param>
        /// <param name="threshold">The threshold below which components are set to zero. Defaults to 0.0001f.</param>
        public static void CleanInPlace(this ref Vector3 self, float threshold = 0.0001f) =>
            self.CopyFrom(self.Clean(threshold));

        /// <summary>
        ///     Converts a Vector3 instance to a string representation with high precision for each component.
        /// </summary>
        /// <param name="v">The Vector3 instance to convert to a string.</param>
        /// <returns>A string representing the Vector3 in the format "(x, y, z)" with high precision on each component.</returns>
        public static string ToFullString(this Vector3 v) => $"({v.x:R}, {v.y:R}, {v.z:R})";
    }
}