using System;
using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.NonUdon;

/// <summary>
///     Wrappers around <see cref="JsonUtility" /> that report failure instead of letting a call throw or quietly
///     return <see langword="null" />. A null result is always treated as an error, because a payload that parsed
///     successfully should have produced an object.
/// </summary>
[PublicAPI]
public static class JsonUtils
{
    /// <summary>
    ///     Deserializes <paramref name="json" /> into <typeparamref name="T" />. <see cref="JsonUtility" /> reports
    ///     some malformed input by returning null, which is converted into a
    ///     <see cref="NullReferenceException" /> here.
    /// </summary>
    /// <param name="json">JSON text produced by <see cref="JsonUtility" />; null or blank is a failure, not an empty object.</param>
    /// <param name="result">The deserialized value on success, otherwise the default of <typeparamref name="T" />.</param>
    /// <param name="exception">The thrown exception, or a synthetic one when the parse produced null.</param>
    /// <returns>Whether a non-null value was deserialized.</returns>
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

    /// <summary>
    ///     Deserializes <paramref name="json" /> into an instance of <paramref name="type" />, for cases where the
    ///     target type is only known at run time. A null result is treated as failure, as in
    ///     <see cref="TryFromJson{T}(string, out T, out Exception)" />.
    /// </summary>
    /// <param name="json">JSON text produced by <see cref="JsonUtility" />.</param>
    /// <param name="type">The concrete type to create; Unity requires it to be serializable.</param>
    /// <param name="result">The created instance on success, otherwise <see langword="null" />.</param>
    /// <param name="exception">The thrown exception, or a synthetic one when the parse produced null.</param>
    /// <returns>Whether a non-null instance was deserialized.</returns>
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

    /// <summary>
    ///     Applies <paramref name="json" /> onto an existing instance, so fields that the JSON omits keep their
    ///     current values. There is no return value to check for null, so only a thrown exception reports failure.
    /// </summary>
    /// <param name="json">JSON text produced by <see cref="JsonUtility" />.</param>
    /// <param name="objectToOverwrite">The instance whose fields are written; it is modified in place.</param>
    /// <param name="exception">The exception that stopped the call, or <see langword="null" /> on success.</param>
    /// <returns>Whether the JSON was applied without throwing.</returns>
    public static bool TryFromJsonOverwrite(string json, object objectToOverwrite, out Exception? exception)
    {
        exception = null;

        var (isSuccess, _, exception2) = Try.Run(() => JsonUtility.FromJsonOverwrite(json, objectToOverwrite));

        if (isSuccess) return true;

        exception = exception2 ?? new Exception("Failed to parse JSON");
        return false;
    }

    /// <summary>
    ///     Serializes <paramref name="obj" />. A null result is treated as failure, which mirrors
    ///     <see cref="TryFromJson{T}(string, out T, out Exception)" />.
    /// </summary>
    /// <param name="obj">The object to serialize; Unity only writes public and <c>[SerializeField]</c> members.</param>
    /// <param name="prettyPrint">Whether to indent the output for reading instead of emitting compact JSON.</param>
    /// <param name="json">The serialized text on success, otherwise <see langword="null" />.</param>
    /// <param name="exception">The thrown exception, or a synthetic one when serialization produced null.</param>
    /// <returns>Whether a non-null JSON string was produced.</returns>
    public static bool TryToJson(object obj, bool prettyPrint, out string? json, out Exception? exception)
    {
        exception = null;
        json = null;

        var (isSuccess, value, exception2) = Try.Run(() => JsonUtility.ToJson(obj, prettyPrint));

        if (isSuccess && value == null)
        {
            isSuccess = false;
            exception2 ??= new NullReferenceException("Serialized JSON value is null");
        }

        if (isSuccess)
        {
            json = value;
            return true;
        }

        exception = exception2 ?? new Exception("Failed to serialize JSON");
        return false;
    }

    /// <summary>Serializes <paramref name="obj" /> without indentation.</summary>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="json">The compact JSON text on success, otherwise <see langword="null" />.</param>
    /// <param name="exception">The exception that stopped the call, or <see langword="null" /> on success.</param>
    /// <returns>Whether a non-null JSON string was produced.</returns>
    public static bool TryToJson(object obj, out string? json, out Exception? exception) =>
        TryToJson(obj, false, out json, out exception);

    /// <summary>Deserializes <paramref name="json" />, falling back to a caller-supplied value instead of an exception.</summary>
    /// <param name="json">JSON text produced by <see cref="JsonUtility" />.</param>
    /// <param name="defaultValue">The value to return when the parse fails or yields null.</param>
    /// <returns>The deserialized value, or <paramref name="defaultValue" />.</returns>
    public static T FromJsonOrDefault<T>(string json, T defaultValue) =>
        TryFromJson<T>(json, out var result, out _) ? result! : defaultValue;

    /// <summary>Deserializes <paramref name="json" /> into <paramref name="type" />, falling back to a default object.</summary>
    /// <param name="json">JSON text produced by <see cref="JsonUtility" />.</param>
    /// <param name="type">The concrete type to create.</param>
    /// <param name="defaultValue">The object to return when the parse fails or yields null.</param>
    /// <returns>The deserialized instance, or <paramref name="defaultValue" />.</returns>
    public static object FromJsonOrDefault(string json, Type type, object defaultValue) =>
        TryFromJson(json, type, out var result, out _) ? result! : defaultValue;

    /// <summary>Serializes <paramref name="obj" />, falling back to a caller-supplied string.</summary>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="prettyPrint">Whether to indent the output.</param>
    /// <param name="defaultValue">The text to return when serialization fails or yields null.</param>
    /// <returns>The JSON text, or <paramref name="defaultValue" />.</returns>
    public static string ToJsonOrDefault(object obj, bool prettyPrint, string defaultValue) =>
        TryToJson(obj, prettyPrint, out var json, out _) ? json! : defaultValue;

    /// <summary>Serializes <paramref name="obj" /> without indentation, falling back to a caller-supplied string.</summary>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="defaultValue">The text to return when serialization fails or yields null.</param>
    /// <returns>The compact JSON text, or <paramref name="defaultValue" />.</returns>
    public static string ToJsonOrDefault(object obj, string defaultValue) =>
        TryToJson(obj, out var json, out _) ? json! : defaultValue;
}