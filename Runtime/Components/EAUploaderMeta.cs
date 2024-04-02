using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EAUploader.Components
{
    public class EAUploaderMeta : MonoBehaviour
    {
        public enum PrefabStatus { Pinned, Show, Hidden, Other }
        public enum PrefabType { VRChat, VRM, Other }

        public PrefabStatus status;
        public PrefabType type;
    }
}
