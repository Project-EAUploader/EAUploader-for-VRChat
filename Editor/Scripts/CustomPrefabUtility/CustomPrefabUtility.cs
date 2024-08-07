using EAUploader.Components;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EAUploader.CustomPrefabUtility
{
    [Serializable]
    public class PrefabInfo
    {
        public string Path;
        public string Name;
        public DateTime LastModified;
        public EAUploaderMeta.PrefabType Type;
        public EAUploaderMeta.PrefabStatus Status;
        public EAUploaderMeta.PrefabGenre Genre;
        public Texture2D Preview { get; internal set; }
    }

    [Serializable]
    public class PrefabInfoList
    {
        public List<PrefabInfo> Prefabs;
    }
}
