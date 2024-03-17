using System;
using System.Collections.Generic;
using UnityEngine;

namespace EAUploader.CustomPrefabUtility
{
    public enum PrefabStatus { Pinned, Show, Hidden, Other }
    public enum PrefabType { VRChat, VRM, Other }

    [Serializable]
    public class PrefabInfo
    {
        public string Path;
        public string Name;
        public DateTime LastModified;
        public PrefabType Type;
        public PrefabStatus Status;
        public Texture2D Preview { get; internal set; }
    }

    [Serializable]
    public class PrefabInfoList
    {
        public List<PrefabInfo> Prefabs;
    }
}
