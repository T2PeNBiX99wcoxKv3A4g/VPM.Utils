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

    public static bool TryFromJson(string json, Type type, out object? result, out Exception? exception)
    {
        result = null;
        exception = null;

        try
        {
            result = JsonUtility.FromJson(json, type);
            return true;
        }
        catch (Exception e)
        {
            exception = e;
            return false;
        }
    }

    public static bool TryFromJsonOverwrite(string json, object objectToOverwrite, out Exception? exception)
    {
        exception = null;

        try
        {
            JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
            return true;
        }
        catch (Exception e)
        {
            exception = e;
            return false;
        }
    }

    public static bool TryToJson(object obj, bool prettyPrint, out string? json, out Exception? exception)
    {
        exception = null;
        json = null;

        try
        {
            json = JsonUtility.ToJson(obj, prettyPrint);
            return true;
        }
        catch (Exception e)
        {
            exception = e;
            return false;
        }
    }

    public static bool TryToJson(object obj, out string? json, out Exception? exception) =>
        TryToJson(obj, false, out json, out exception);
}