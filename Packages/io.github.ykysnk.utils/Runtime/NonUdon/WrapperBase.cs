using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.NonUdon
{
    [Serializable]
    [PublicAPI]
    public class WrapperBase<T>
    {
        public List<T> items = new();
    }
}