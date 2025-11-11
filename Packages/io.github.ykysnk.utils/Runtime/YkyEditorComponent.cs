using JetBrains.Annotations;
using UnityEngine;
using VRC.SDKBase;

namespace io.github.ykysnk.utils
{
    [PublicAPI]
    public abstract class YkyEditorComponent : MonoBehaviour, IEditorOnly
    {
        public virtual void OnInspectorGUI()
        {
        }
    }
}