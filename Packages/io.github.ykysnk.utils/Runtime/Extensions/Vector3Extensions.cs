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
    }
}