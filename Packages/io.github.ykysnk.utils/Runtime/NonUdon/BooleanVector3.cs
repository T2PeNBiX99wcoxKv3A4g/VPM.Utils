using System;
using System.Linq;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon
{
    [Serializable]
    [PublicAPI]
    public struct BooleanVector3
    {
        public bool x;
        public bool y;
        public bool z;

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

        public bool HasFlag(Flag flag) => (Flags & (int)flag) == (int)flag;

        public bool HasFlags(params Flag[] flags)
        {
            var allFlags = flags.Aggregate(0, (current, flag) => current | (int)flag);
            return (Flags & allFlags) == allFlags;
        }

        public void AddFlag(Flag flag) => Flags |= (int)flag;
        public void AddFlags(params Flag[] flags) => Flags |= flags.Aggregate(0, (current, flag) => current | (int)flag);
        public void RemoveFlag(Flag flag) => Flags &= ~(int)flag;

        public void RemoveFlags(params Flag[] flags) =>
            Flags &= ~flags.Aggregate(0, (current, flag) => current | (int)flag);

        public void Set(bool x2, bool y2, bool z2)
        {
            x = x2;
            y = y2;
            z = z2;
        }

        public void Set(BooleanVector3 other) => Set(other.x, other.y, other.z);

        public void SetAll(bool value) => Set(value, value, value);

        public void Invert() => Set(!x, !y, !z);

        public void SetX(bool value) => x = value;
        public void SetY(bool value) => y = value;
        public void SetZ(bool value) => z = value;

        public void SetTrue() => SetAll(true);
        public void SetFalse() => SetAll(false);

        public override string ToString() => $"BooleanVector3(x:{x},y:{y},z:{z})";

        public static BooleanVector3 True => new()
        {
            x = true,
            y = true,
            z = true
        };

        public static BooleanVector3 False => new();

        public static BooleanVector3 operator !(BooleanVector3 booleanVector3)
        {
            booleanVector3.Invert();
            return booleanVector3;
        }

        public static bool operator true(BooleanVector3 booleanVector3) =>
            booleanVector3 is { x: true, y: true, z: true };

        public static bool operator false(BooleanVector3 booleanVector3) =>
            booleanVector3 is { x: false, y: false, z: false };

        [PublicAPI]
        public enum Flag
        {
            X = 1 << 0,
            Y = 1 << 1,
            Z = 1 << 2,
            None = 0,
            All = X | Y | Z
        }
    }
}