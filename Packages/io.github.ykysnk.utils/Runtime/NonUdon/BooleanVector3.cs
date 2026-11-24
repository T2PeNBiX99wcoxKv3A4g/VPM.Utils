using System;
using System.Linq;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon;

/// <summary>
///     Three independent boolean values kept as plain serializable fields, with a <see cref="Flags" /> view that
///     packs them into a single bitmask for switch-like tests.
/// </summary>
[Serializable]
[PublicAPI]
public struct BooleanVector3 : IEquatable<BooleanVector3>
{
    /// <summary>Whether the X component is set.</summary>
    public bool x;
    /// <summary>Whether the Y component is set.</summary>
    public bool y;
    /// <summary>Whether the Z component is set.</summary>
    public bool z;

    /// <summary>
    ///     The three components as a bitmask of <see cref="Flag" /> values. Assigning a mask overwrites all three
    ///     components, so a component that the mask does not name is cleared rather than left alone.
    /// </summary>
    /// <value>
    ///     The bits of the components that are currently set; assigning any bits outside <see cref="Flag.All" /> has
    ///     no effect because those bits are dropped.
    /// </value>
    public int Flags
    {
        get => (x ? (int)Flag.X : 0) | (y ? (int)Flag.Y : 0) | (z ? (int)Flag.Z : 0);
        set
        {
            x = (value & (int)Flag.X) != 0;
            y = (value & (int)Flag.Y) != 0;
            z = (value & (int)Flag.Z) != 0;
        }
    }

    /// <summary>Whether every bit of <paramref name="flag" /> is currently set.</summary>
    /// <param name="flag">One component or a combination; <see cref="Flag.None" /> always matches.</param>
    /// <returns><see langword="true" /> when all the requested bits are set.</returns>
    public bool HasFlag(Flag flag) => (Flags & (int)flag) == (int)flag;

    /// <summary>Whether every bit covered by <paramref name="flags" /> is currently set.</summary>
    /// <param name="flags">The components to test together; an empty array matches everything.</param>
    /// <returns><see langword="true" /> when all the requested bits are set.</returns>
    public bool HasFlags(params Flag[] flags)
    {
        var allFlags = flags.Aggregate(0, (current, flag) => current | (int)flag);
        return (Flags & allFlags) == allFlags;
    }

    /// <summary>Sets the bits of <paramref name="flag" /> and leaves the other components untouched.</summary>
    /// <param name="flag">The component or components to turn on.</param>
    public void AddFlag(Flag flag) => Flags |= (int)flag;
    /// <summary>Sets the bits covered by <paramref name="flags" /> and leaves the other components untouched.</summary>
    /// <param name="flags">The components to turn on; an empty array changes nothing.</param>
    public void AddFlags(params Flag[] flags) => Flags |= flags.Aggregate(0, (current, flag) => current | (int)flag);

    /// <summary>Clears the bits of <paramref name="flag" /> and leaves the other components untouched.</summary>
    /// <param name="flag">The component or components to turn off.</param>
    public void RemoveFlag(Flag flag) => Flags &= ~(int)flag;

    /// <summary>Clears the bits covered by <paramref name="flags" /> and leaves the other components untouched.</summary>
    /// <param name="flags">The components to turn off; an empty array changes nothing.</param>
    public void RemoveFlags(params Flag[] flags) =>
        Flags &= ~flags.Aggregate(0, (current, flag) => current | (int)flag);

    /// <summary>Replaces the whole mask with <paramref name="flag" />, so it behaves like an assignment.</summary>
    /// <param name="flag">The complete mask to store; components it does not name are cleared.</param>
    public void SetFlag(Flag flag) => Flags = (int)flag;
    /// <summary>Replaces the whole mask with the union of <paramref name="flags" />.</summary>
    /// <param name="flags">The complete set of components to keep; an empty array clears all three.</param>
    public void SetFlags(params Flag[] flags) => Flags = flags.Aggregate(0, (current, flag) => current | (int)flag);

    /// <summary>Assigns all three components in one call.</summary>
    /// <param name="x2">Value for the X component.</param>
    /// <param name="y2">Value for the Y component.</param>
    /// <param name="z2">Value for the Z component.</param>
    public void Set(bool x2, bool y2, bool z2)
    {
        x = x2;
        y = y2;
        z = z2;
    }

    /// <summary>Copies all three components from <paramref name="other" />.</summary>
    /// <param name="other">The vector to copy; it is read only, so copying onto itself is harmless.</param>
    public void Set(BooleanVector3 other) => Set(other.x, other.y, other.z);

    /// <summary>Assigns the same value to all three components.</summary>
    /// <param name="value">The value to store in X, Y and Z.</param>
    public void SetAll(bool value) => Set(value, value, value);

    /// <summary>Negates every component.</summary>
    public void Invert() => Set(!x, !y, !z);

    /// <summary>Assigns the X component only.</summary>
    /// <param name="value">The new X value.</param>
    public void SetX(bool value) => x = value;
    /// <summary>Assigns the Y component only.</summary>
    /// <param name="value">The new Y value.</param>
    public void SetY(bool value) => y = value;
    /// <summary>Assigns the Z component only.</summary>
    /// <param name="value">The new Z value.</param>
    public void SetZ(bool value) => z = value;

    /// <summary>Sets all three components, the same as <see cref="SetAll" /> with <c>true</c>.</summary>
    public void SetTrue() => SetAll(true);
    /// <summary>Clears all three components, the same as <see cref="SetAll" /> with <c>false</c>.</summary>
    public void SetFalse() => SetAll(false);

    /// <summary>Renders the components as <c>BooleanVector3(x:…,y:…,z:…)</c>.</summary>
    /// <returns>The component values as text, with no invariant-culture formatting involved.</returns>
    public override string ToString() => $"BooleanVector3(x:{x},y:{y},z:{z})";

    /// <summary>All three components set.</summary>
    /// <value>A new vector whose X, Y and Z are all <see langword="true" />.</value>
    public static BooleanVector3 True => new()
    {
        x = true,
        y = true,
        z = true
    };

    /// <summary>All three components cleared.</summary>
    /// <value>A new vector whose X, Y and Z are all <see langword="false" />.</value>
    public static BooleanVector3 False => new();

    /// <summary>Negates every component and returns the negated copy, leaving the operand unchanged.</summary>
    /// <param name="booleanVector3">The vector to negate; it is passed by value and not modified.</param>
    /// <returns>A vector in which every component of the operand is flipped.</returns>
    public static BooleanVector3 operator !(BooleanVector3 booleanVector3)
    {
        booleanVector3.Invert();
        return booleanVector3;
    }

    /// <summary>
    ///     True only when all three components are set, so a conditional on the vector acts as a three-way AND
    ///     rather than an "any component set" test.
    /// </summary>
    /// <param name="booleanVector3">The vector tested by <c>if</c>, <c>&amp;&amp;</c> or <c>||</c>.</param>
    /// <returns><see langword="true" /> when X, Y and Z are all set.</returns>
    public static bool operator true(BooleanVector3 booleanVector3) =>
        booleanVector3.Flags == (int)Flag.All;

    /// <summary>
    ///     True whenever at least one component is cleared. The compiler requires this alongside the <c>true</c>
    ///     operator whenever the type takes part in <c>&amp;&amp;</c> or <c>||</c>.
    /// </summary>
    /// <param name="booleanVector3">The vector whose components are tested.</param>
    /// <returns><see langword="true" /> when any component is cleared.</returns>
    public static bool operator false(BooleanVector3 booleanVector3) =>
        booleanVector3.Flags != (int)Flag.All;

    /// <summary>Bit selector for one component or a combination of them.</summary>
    [PublicAPI]
    public enum Flag
    {
        /// <summary>The X component.</summary>
        X = 1 << 0,
        /// <summary>The Y component.</summary>
        Y = 1 << 1,
        /// <summary>The Z component.</summary>
        Z = 1 << 2,
        /// <summary>No component.</summary>
        None = 0,
        /// <summary>X, Y and Z together.</summary>
        All = X | Y | Z
    }

    /// <summary>Whether <paramref name="other" /> holds the same three components.</summary>
    /// <param name="other">The vector to compare against.</param>
    /// <returns><see langword="true" /> when all three components are equal.</returns>
    public bool Equals(BooleanVector3 other) => x == other.x && y == other.y && z == other.z;
    /// <summary>Whether <paramref name="obj" /> is a <see cref="BooleanVector3" /> with the same three components.</summary>
    /// <param name="obj">The instance to compare; any other type returns <see langword="false" />.</param>
    /// <returns><see langword="true" /> when <paramref name="obj" /> is a vector with equal components.</returns>
    public override bool Equals(object? obj) => obj is BooleanVector3 other && Equals(other);
    /// <summary>Hash code built from the three components.</summary>
    /// <returns>A hash consistent with <see cref="Equals(BooleanVector3)" />.</returns>
    public override int GetHashCode() => HashCode.Combine(x, y, z);
}