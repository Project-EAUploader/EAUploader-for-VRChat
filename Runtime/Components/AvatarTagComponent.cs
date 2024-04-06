using UnityEngine;

namespace EAUploader.Components
{
    // https://github.com/bdunderscore/modular-avatar/blob/db49e2e210bc070671af963ff89df853ae4514a5/Packages/nadena.dev.modular-avatar/Runtime/AvatarTagComponent.cs
    // Originally under MIT License
    // Copyright (c) 2022 bd_
    /**
     * This abstract base class is injected into the VRCSDK avatar component allowlist to avoid
     */
    [DefaultExecutionOrder(-9990)] // run before av3emu
    [ExecuteAlways]
    public abstract class AvatarTagComponent : MonoBehaviour
#if VRC_SDK3A
        , VRC.SDKBase.IEditorOnly
#endif
    {
    }
}