using System;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class Vector3Extensions
    {
        public static float Distance(this Vector3 a, Vector3 b) => Vector3.Distance(a, b);

        public static float Distance(this Vector3 a, [NotNull] Transform b) => Vector3.Distance(a, b.position);

        public static float Distance2D(this Vector3 a, Vector3 b)
        {
            var differenceX = a.x - b.x;
            var differenceZ = a.z - b.z;
            return Mathf.Sqrt(differenceX * differenceX + differenceZ * differenceZ);
        }

        public static float Distance2D(this Vector3 a, [NotNull] Transform b) => a.Distance2D(b.position);

        public static Vector3 Divide(this Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

        public static Vector3 Multiply(this Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);

        public static Vector3 Round(this Vector3 a, int digits, MidpointRounding mode) =>
            new Vector3(MathF.Round(a.x, digits, mode), MathF.Round(a.y, digits, mode), MathF.Round(a.z, digits, mode));

        public static Vector3 Round(this Vector3 a, int digits) => a.Round(digits, MidpointRounding.ToEven);

        public static Vector3 Round(this Vector3 a, MidpointRounding mode) => a.Round(0, mode);

        public static Vector3 Round(this Vector3 a) => a.Round(0);

        public static Vector3 DeltaAngle(this Vector3 a, float current) => new Vector3(Mathf.DeltaAngle(current, a.x),
            Mathf.DeltaAngle(current, a.y), Mathf.DeltaAngle(current, a.z));

        public static Vector3 DeltaAngle(this Vector3 a) => a.DeltaAngle(0);

        public static Vector3 ClampMagnitude(this Vector3 vector, float maxLength) =>
            vector.magnitude > maxLength ? vector.normalized * maxLength : vector;

        public static Vector3 Abs(this Vector3 vector) =>
            new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));

        public static Vector3 Min(this Vector3 a, Vector3 b) =>
            new Vector3(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));

        public static Vector3 Max(this Vector3 a, Vector3 b) =>
            new Vector3(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));

        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max) => new Vector3(
            Mathf.Clamp(vector.x, min.x, max.x), Mathf.Clamp(vector.y, min.y, max.y),
            Mathf.Clamp(vector.z, min.z, max.z));

        public static Vector3 Clamp01(this Vector3 vector) =>
            new Vector3(Mathf.Clamp01(vector.x), Mathf.Clamp01(vector.y), Mathf.Clamp01(vector.z));

        public static Vector3 DirectionTo(this Vector3 from, Vector3 to) => (to - from).normalized;

        public static Vector3 InverseDirectionTo(this Vector3 from, Vector3 to) => (from - to).normalized;

        public static Vector3 LerpUnclamped(this Vector3 a, Vector3 b, float t) => Vector3.LerpUnclamped(a, b, t);

        public static Vector3 Lerp(this Vector3 a, Vector3 b, float t) => Vector3.Lerp(a, b, t);

        public static Vector3 MoveTowards(this Vector3 current, Vector3 target, float maxDistanceDelta) =>
            Vector3.MoveTowards(current, target, maxDistanceDelta);

        public static Vector3 RotateTowards(this Vector3 current, Vector3 target, float maxRadiansDelta,
            float maxMagnitudeDelta) => Vector3.RotateTowards(current, target, maxRadiansDelta, maxMagnitudeDelta);

        public static Vector3 ProjectOnPlane(this Vector3 vector, Vector3 planeNormal) =>
            Vector3.ProjectOnPlane(vector, planeNormal);

        public static Vector3 Project(this Vector3 vector, Vector3 onNormal) => Vector3.Project(vector, onNormal);

        public static Vector3 Reflect(this Vector3 inDirection, Vector3 inNormal) =>
            Vector3.Reflect(inDirection, inNormal);

        public static Vector3 Clean(this Vector3 vector, float threshold = 0.0001f) =>
            new Vector3(vector.x.Clean(threshold), vector.y.Clean(threshold), vector.z.Clean(threshold));

        public static bool IsNearly(this Vector3 vector, Vector3 target, float threshold = 0.0001f) =>
            vector.x.IsNearly(target.x, threshold) && vector.y.IsNearly(target.y, threshold) &&
            vector.z.IsNearly(target.z, threshold);

        public static bool IsNearlyZero(this Vector3 vector, float threshold = 0.0001f) =>
            vector.x.IsNearlyZero(threshold) && vector.y.IsNearlyZero(threshold) && vector.z.IsNearlyZero(threshold);

        public static void CopyFrom(this ref Vector3 self, Vector3 other)
        {
            self.x = other.x;
            self.y = other.y;
            self.z = other.z;
        }

        public static void CleanInPlace(this ref Vector3 self, float threshold = 0.0001f) =>
            self.CopyFrom(self.Clean(threshold));

        public static string ToFullString(this Vector3 v) => $"({v.x:R}, {v.y:R}, {v.z:R})";
    }
}