using JetBrains.Annotations;
#if UTILS_NDMF
using nadena.dev.ndmf;

#elif UTILS_VRC_SDK3_BASE
using VRC.SDKBase;
#endif

namespace io.github.ykysnk.utils
{
    [PublicAPI]
    public interface IUtilsEditorOnly
#if UTILS_NDMF
        : INDMFEditorOnly
#elif UTILS_VRC_SDK3_BASE
    : IEditorOnly
#endif
    {
    }
}