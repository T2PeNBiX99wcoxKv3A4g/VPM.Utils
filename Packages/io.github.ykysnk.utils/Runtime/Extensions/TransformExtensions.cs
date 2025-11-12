using JetBrains.Annotations;
using UnityEngine;
using VRC.SDKBase;

namespace io.github.ykysnk.utils.Extensions;

[PublicAPI]
public static class TransformExtensions
{
    public static string FullName(this Transform transform)
    {
        var tmpName = transform.name;

        while (Utilities.IsValid(transform.parent))
        {
            transform = transform.parent;
            tmpName = transform.name + "/" + tmpName;
        }

        return tmpName;
    }

    // Refs: https://discussions.unity.com/t/world-scale/374693
    public static Vector3 GetWorldScale(this Transform transform)
    {
        var worldScale = transform.localScale;
        var parent = transform.parent;

        while (parent)
        {
            worldScale = Vector3.Scale(worldScale, parent.localScale);
            parent = parent.parent;
        }

        return worldScale;
    }
}