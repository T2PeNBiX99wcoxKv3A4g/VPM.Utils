using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Extensions;

[PublicAPI]
public static class Vector3Extensions
{
    public static float Distance(this Vector3 a, Vector3 b) => Vector3.Distance(a, b);

    public static float Distance(this Vector3 a, Transform b) => Vector3.Distance(a, b.position);

    public static float Distance2D(this Vector3 a, Vector3 b)
    {
        var differenceX = a.x - b.x;
        var differenceZ = a.z - b.z;
        return Mathf.Sqrt(differenceX * differenceX + differenceZ * differenceZ);
    }

    public static float Distance2D(this Vector3 a, Transform b) => a.Distance2D(b.position);
}