using System;
using UnityEngine;

namespace io.github.ykysnk.utils.NonUdon;

/// <summary>
///     A <see cref="Color" /> together with its HTML hex form, so a colour can be serialized and inspected as text.
///     Only the operators update both fields at once; assigning one field directly leaves the other unchanged, and a
///     default-initialized struct derives its text from <see cref="color" /> when <see cref="hex" /> is null.
/// </summary>
[Serializable]
public struct ColorHex
{
    /// <summary>The colour value that <see cref="hex" /> spells out.</summary>
    public Color color;
    /// <summary>
    ///     The colour as <c>#RRGGBBAA</c>, including the leading <c>#</c>. Read straight from the field, so it can be
    ///     <see langword="null" /> on a default-initialized struct; conversions fall back to <see cref="color" /> then.
    /// </summary>
    public string hex;

    private string ResolvedHex => hex ?? $"#{ColorUtility.ToHtmlStringRGBA(color)}";

    /// <summary>Renders both fields as <c>Color: …, Hex: …</c>.</summary>
    /// <returns>The two values as text, with the hex derived from <see cref="color" /> when <see cref="hex" /> is null.</returns>
    public override string ToString() => $"Color: {color}, Hex: {ResolvedHex}";

    /// <summary>
    ///     Returns the hex text, so the struct can stand in wherever a colour string is expected. A null
    ///     <see cref="hex" /> is derived from <see cref="color" />, so the result is never null.
    /// </summary>
    /// <param name="colorHex">The struct to convert; <c>hex</c> is used when present, <c>color</c> otherwise.</param>
    /// <returns><c>#RRGGBBAA</c> for the stored colour.</returns>
    public static implicit operator string(ColorHex colorHex) => colorHex.ResolvedHex;
    /// <summary>Returns <see cref="color" />, so the struct can be used directly as a colour.</summary>
    /// <param name="colorHex">The struct to convert; only its <c>color</c> field is read.</param>
    /// <returns>The stored colour value.</returns>
    public static implicit operator Color(ColorHex colorHex) => colorHex.color;

    /// <summary>
    ///     Parses an HTML colour string. Input that <see cref="ColorUtility.TryParseHtmlString" /> rejects becomes
    ///     white instead of an error, and <c>hex</c> is then rewritten from the parsed colour, so the stored text is
    ///     normalized rather than kept verbatim.
    /// </summary>
    /// <param name="hex">A string such as <c>#FF8000</c> or <c>#FF8000FF</c>; null or unparsable text falls back to white.</param>
    /// <returns>A struct holding the parsed colour and its normalized <c>#RRGGBBAA</c> text.</returns>
    public static implicit operator ColorHex(string hex)
    {
        var getColor = ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.white;
        var getHex = $"#{ColorUtility.ToHtmlStringRGBA(getColor)}";

        return new()
        {
            color = getColor,
            hex = getHex
        };
    }

    /// <summary>Wraps a colour and derives its <c>#RRGGBBAA</c> text.</summary>
    /// <param name="color">The colour to store, including its alpha channel.</param>
    /// <returns>A struct whose <c>hex</c> field is generated from <paramref name="color" />.</returns>
    public static implicit operator ColorHex(Color color) => new()
    {
        color = color,
        hex = $"#{ColorUtility.ToHtmlStringRGBA(color)}"
    };
}