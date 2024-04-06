using UnityEngine;

namespace EAUploader.Components
{
    [DisallowMultipleComponent]
    public class EAUploaderMeta : AvatarTagComponent
    {
        public enum PrefabStatus { Pinned, Show, Hidden, Other }
        public enum PrefabType { VRChat, VRM, Other }

        public PrefabStatus status;
        public PrefabType type;
    }
}
