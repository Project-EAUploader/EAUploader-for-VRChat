using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace EAUploader.CustomPrefabUtility
{
    public class PrefabManager
    {
        private const string PREFABS_INFO_PATH = "Assets/EAUploader/PrefabManager.json";

        public static List<PrefabInfo> prefabInfoList;

        public static void Initialize()
        {
            UpdatePrefabInfo();
            PrefabPreview.GenerateAndSaveAllPrefabPreviews();
        }

        public static void UpdatePrefabInfo()
        {
            var allPrefabs = GetAllPrefabs();

            allPrefabs = allPrefabs
                .OrderByDescending(p => p.Status == PrefabStatus.Pinned)
                .ThenByDescending(p => p.LastModified)
                .ToList();

            SavePrefabsInfo(allPrefabs);

            if (prefabInfoList == null)
            {
                prefabInfoList = allPrefabs;
            }
        }

        public static void ImportPrefab(string prefabPath)
        {
            GameObject prefab = GetPrefab(prefabPath);
            Texture2D preview = PrefabPreview.GeneratePreview(prefab);
            PrefabPreview.SavePrefabPreview(prefabPath, preview);

            UI.ImportSettings.ManageModels.UpdateModelList();
        }

        private static void SavePrefabsInfo(List<PrefabInfo> prefabs)
        {
            string directory = Path.GetDirectoryName(PREFABS_INFO_PATH);

            // ディレクトリが存在しない場合は作成
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // JSONデータを生成
            var prefabList = new PrefabInfoList { Prefabs = prefabs };
            string json = JsonUtility.ToJson(prefabList, true);

            // ファイルに書き込む
            File.WriteAllText(PREFABS_INFO_PATH, json);
        }

        private static List<PrefabInfo> LoadPrefabsInfo()
        {
            if (!File.Exists(PREFABS_INFO_PATH)) return new List<PrefabInfo>();

            string json = File.ReadAllText(PREFABS_INFO_PATH);
            PrefabInfoList prefabList = JsonUtility.FromJson<PrefabInfoList>(json);
            return prefabList.Prefabs;
        }

        internal static List<PrefabInfo> GetAllPrefabs()
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            return prefabGuids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => CreatePrefabInfo(path))
                .OrderBy(p => p.LastModified)
                .ToList();
        }

        public static List<PrefabInfo> GetAllPrefabsWithPreview()
        {
            var allPrefabs = GetAllPrefabs();
            allPrefabs = allPrefabs
                .OrderByDescending(p => p.Status == PrefabStatus.Pinned)
                .ThenByDescending(p => p.LastModified)
                .ToList();

            foreach (var prefab in allPrefabs)
            {
                string previewImagePath = PrefabPreview.GetPreviewImagePath(prefab.Path);
                if (File.Exists(previewImagePath))
                {
                    prefab.Preview = PrefabPreview.LoadTextureFromFile(previewImagePath);
                }
            }
            return allPrefabs;
        }

        private static PrefabInfo CreatePrefabInfo(string path)
        {
            return new PrefabInfo
            {
                Path = path,
                Name = Path.GetFileNameWithoutExtension(path),
                LastModified = File.GetLastWriteTime(path),
                Type = GetPrefabType(path),
                Status = GetPrefabStatus(path)
            };
        }

        private static PrefabType GetPrefabType(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                if (prefab.GetComponent("VRC_AvatarDescriptor") != null)
                    return PrefabType.VRChat;
                if (prefab.GetComponent("VRMMeta") != null)
                    return PrefabType.VRM;
            }
            return PrefabType.Other;
        }

        private static PrefabStatus GetPrefabStatus(string path)
        {
            // JSONファイルからプレハブのステータスを読み込む
            var allPrefabsInfo = LoadPrefabsInfo();
            var prefabInfo = allPrefabsInfo.FirstOrDefault(info => info.Path == path);

            var status = prefabInfo?.Status ?? PrefabStatus.Other;
            return status;
        }

        public static GameObject GetPrefab(string prefabPath)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        public static bool ShowDeletePrefabDialog(string prefabPath)
        {
            if (EditorUtility.DisplayDialog("Prefabの消去", "本当にPrefabを消去しますか？", "消去", "キャンセル"))
            {
                DeletePrefabPreview(prefabPath);
                AssetDatabase.DeleteAsset(prefabPath);
                EAUploaderCore.selectedPrefabPath = null;
                return true;
            }
            return false;
        }

        public static void DeletePrefabPreview(string prefabPath)
        {
            string previewImagePath = PrefabPreview.GetPreviewImagePath(prefabPath);

            if (File.Exists(previewImagePath))
            {
                File.Delete(previewImagePath);
            }
        }

        public static void PinPrefab(string prefabPath)
        {
            var allPrefabs = LoadPrefabsInfo();
            var prefab = allPrefabs.FirstOrDefault(p => p.Path == prefabPath);
            if (prefab != null)
            {
                prefab.Status = (prefab.Status == PrefabStatus.Pinned) ? PrefabStatus.Show : PrefabStatus.Pinned;
                SavePrefabsInfo(allPrefabs);
            }
        }

        public static VRCAvatarDescriptor? GetAvatarDescriptor(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                var avatarDescriptor = prefab.GetComponent<VRCAvatarDescriptor>();
                return avatarDescriptor;
            }
            return null;
        }

        public static bool IsPinned(string prefabPath)
        {
            var allPrefabs = LoadPrefabsInfo();
            var prefab = allPrefabs.FirstOrDefault(p => p.Path == prefabPath);
            return prefab?.Status == PrefabStatus.Pinned;
        }
    }
}