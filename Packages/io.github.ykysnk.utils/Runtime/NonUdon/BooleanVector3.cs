using System;
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

        public override string ToString() => $"BooleanVector3(x:{x},y:{y},z:{z})";

        public static BooleanVector3 True => new()
        {
            x = true,
            y = true,
            z = true
        };
    }
}