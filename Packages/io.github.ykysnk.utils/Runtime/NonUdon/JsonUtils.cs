using System;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.NonUdon;

[PublicAPI]
public static class JsonUtils
{
    public static bool TryFromJson<T>(string json, out T? result, out Exception? exception)
    {
        result = default;
        exception = null;

        try
        {
            result = JsonUtility.FromJson<T>(json);
            return true;
        }
        catch (Exception e)
        {
            exception = e;
            return false;
        }
    }
}