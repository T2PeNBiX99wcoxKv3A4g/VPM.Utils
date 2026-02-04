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

        var (isSuccess, value, exception2) = Try.Run(() => JsonUtility.FromJson<T>(json));

        if (isSuccess && value == null)
        {
            isSuccess = false;
            exception2 ??= new NullReferenceException("Deserialized JSON value is null");
        }

        if (isSuccess)
        {
            result = value;
            return true;
        }

        exception = exception2 ?? new Exception("Failed to parse JSON");
        return false;
    }

    public static bool TryFromJson(string json, Type type, out object? result, out Exception? exception)
    {
        result = null;
        exception = null;

        var (isSuccess, value, exception2) = Try.Run(() => JsonUtility.FromJson(json, type));

        if (isSuccess && value == null)
        {
            isSuccess = false;
            exception2 ??= new NullReferenceException("Deserialized JSON value is null");
        }

        if (isSuccess)
        {
            result = value;
            return true;
        }

        exception = exception2 ?? new Exception("Failed to parse JSON");
        return false;
    }

    public static bool TryFromJsonOverwrite(string json, object objectToOverwrite, out Exception? exception)
    {
        exception = null;

        var (isSuccess, _, exception2) = Try.Run(() => JsonUtility.FromJsonOverwrite(json, objectToOverwrite));

        if (isSuccess)
        {
            JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
            return true;
        }

        exception = exception2 ?? new Exception("Failed to parse JSON");
        return false;
    }

    public static bool TryToJson(object obj, bool prettyPrint, out string? json, out Exception? exception)
    {
        exception = null;
        json = null;

        var (isSuccess, value, exception2) = Try.Run(() => JsonUtility.ToJson(obj, prettyPrint));

        if (isSuccess && value == null)
        {
            isSuccess = false;
            exception2 ??= new NullReferenceException("Deserialized JSON value is null");
        }

        if (isSuccess)
        {
            json = value;
            return true;
        }

        exception = exception2 ?? new Exception("Failed to serialize JSON");
        return false;
    }

    public static bool TryToJson(object obj, out string? json, out Exception? exception) =>
        TryToJson(obj, false, out json, out exception);

    public static T FromJsonOrDefault<T>(string json, T defaultValue) =>
        TryFromJson<T>(json, out var result, out _) ? result! : defaultValue;

    public static object FromJsonOrDefault(string json, Type type, object defaultValue) =>
        TryFromJson(json, type, out var result, out _) ? result! : defaultValue;

    public static string ToJsonOrDefault(object obj, bool prettyPrint, string defaultValue) =>
        TryToJson(obj, prettyPrint, out var json, out _) ? json! : defaultValue;

    public static string ToJsonOrDefault(object obj, string defaultValue) =>
        TryToJson(obj, out var json, out _) ? json! : defaultValue;
}