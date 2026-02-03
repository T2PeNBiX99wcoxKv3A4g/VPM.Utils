using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace io.github.ykysnk.utils.Editor
{
    [Serializable]
    [PublicAPI]
    public class WrapperBase<T>
    {
        public List<T> items = new();
    }
}