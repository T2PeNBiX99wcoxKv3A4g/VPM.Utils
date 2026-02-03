using System;
using System.Collections.Generic;

namespace io.github.ykysnk.utils.Editor
{
    [Serializable]
    public class WrapperBase<T>
    {
        public List<T> items = new();
    }
}