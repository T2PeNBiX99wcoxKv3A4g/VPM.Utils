using System;
using UnityEngine;

namespace io.github.ykysnk.utils.NonUdon;

/// <summary>
///     A <see cref="Color" /> together with its HTML hex form, so a colour can be serialized and inspected as text.
///     Only the operators update both fields at once; assigning one field directly leaves the other unchanged.
/// </summary>
[Serializable]
public struct ColorHex
{
    /// <summary>The colour value that <see cref="hex" /> spells out.</summary>
    public Color color;
    /// <summary>
    ///     The colour as <c>#RRGGBBAA</c>, including the leading <c>#</c>. Read straight from the field, so it can be
    ///     <see langword="null" /> on a default-initialized struct.
    /// </summary>
    public string hex;

    /// <summary>Renders both fields as <c>Color: …, Hex: …</c>.</summary>
    /// <returns>The two field values as text; a null <see cref="hex" /> is rendered as an empty string.</returns>
    public override string ToString() => $"Color: {color}, Hex: {hex}";

    /// <summary>Returns <see cref="hex" />, so the struct can stand in wherever a colour string is expected.</summary>
    /// <param name="colorHex">The struct to convert; only its <c>hex</c> field is read.</param>
    /// <returns>The stored hex text, which may be <see langword="null" />.</returns>
    public static implicit operator string(ColorHex colorHex) => colorHex.hex;
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