using JetBrains.Annotations;
using UnityEngine;
using VRC.SDKBase;

namespace io.github.ykysnk.utils.Extensions;

[PublicAPI]
public static class GameObjectExtensions
{
    public static string? FullName(this GameObject obj) =>
        !Utilities.IsValid(obj.transform) ? null : obj.transform.FullName();

    public static bool IsCloseRange(this GameObject obj, GameObject other, float distance) =>
        obj.transform.IsCloseRange(other.transform, distance);

    public static bool IsCloseRange2D(this GameObject obj, GameObject other, float distance) =>
        obj.transform.IsCloseRange2D(other.transform, distance);

    public static bool IsPlayerCloseRange(this GameObject obj, float distance) =>
        obj.transform.IsPlayerCloseRange(distance);

    public static bool IsPlayerCloseRange2D(this GameObject obj, float distance) =>
        obj.transform.IsPlayerCloseRange2D(distance);
}