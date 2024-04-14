using EAUploader.Components;
using System;
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

        private const string PREFAB_LISTS_PATH = "Assets/EAUploader/PrefabLists.json";

        public static List<PrefabInfo> prefabInfoList;
        public static Dictionary<string, List<PrefabInfo>> prefabLists;

        public static void Initialize()
        {
            UpdatePrefabInfo();
            PrefabPreview.GenerateAndSaveAllPrefabPreviews();
        }

        public static void UpdatePrefabInfo()
        {
            LoadPrefabLists();

            var allPrefabs = GetAllPrefabs();

            if (prefabLists == null)
            {
                Debug.Log("prefabLists is null. Initializing...");
                prefabLists = new Dictionary<string, List<PrefabInfo>>
                {
                    { "default", new List<PrefabInfo>() }
                };
            }

            if (!prefabLists.ContainsKey("default"))
            {
                Debug.Log("prefabLists does not contain 'default' key. Adding...");
                prefabLists["default"] = new List<PrefabInfo>();
            }

            Debug.Log($"Updating 'default' list with {allPrefabs.Count} prefabs.");
            prefabLists["default"].Clear();
            prefabLists["default"].AddRange(allPrefabs);

            Debug.Log($"Saving prefab info for {allPrefabs.Count} prefabs.");
            SavePrefabsInfo(allPrefabs);

            Debug.Log($"Saving prefab lists. Lists count: {prefabLists.Count}");
            SavePrefabLists();

            if (prefabInfoList == null)
            {
                Debug.Log("prefabInfoList is null. Initializing...");
                prefabInfoList = allPrefabs;
            }
        }

        public static void ImportPrefab(string prefabPath)
        {
            GameObject prefab = GetPrefab(prefabPath);
            var existingMeta = prefab.GetComponent<EAUploaderMeta>();
            if (existingMeta == null)
            {
                var meta = prefab.AddComponent<EAUploaderMeta>();
                meta.type = GetPrefabType(prefabPath); 
            }

            Texture2D preview = PrefabPreview.GeneratePreview(prefab);
            PrefabPreview.SavePrefabPreview(prefabPath, preview);

            UI.ImportSettings.ManageModels.UpdateModelList();
        }

        internal static void SavePrefabsInfo(List<PrefabInfo> prefabs)
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

        internal static List<PrefabInfo> LoadPrefabsInfo()
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
                .Where(p => p.Status != EAUploaderMeta.PrefabStatus.Hidden)
                .OrderByDescending(p => p.Status == EAUploaderMeta.PrefabStatus.Pinned)
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

        public static List<PrefabInfo> GetAllPrefabsIncludingHidden()
        {
            var allPrefabs = GetAllPrefabs();
            allPrefabs = allPrefabs
                .OrderByDescending(p => p.Status == EAUploaderMeta.PrefabStatus.Pinned)
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

        private static EAUploaderMeta.PrefabType GetPrefabType(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                if (prefab.GetComponent("VRC_AvatarDescriptor") != null)
                    return EAUploaderMeta.PrefabType.VRChat;
                if (prefab.GetComponent("VRMMeta") != null)
                    return EAUploaderMeta.PrefabType.VRM;
            }
            return EAUploaderMeta.PrefabType.Other;
        }

        private static EAUploaderMeta.PrefabStatus GetPrefabStatus(string path)
        {
            // JSONファイルからプレハブのステータスを読み込む
            var allPrefabsInfo = LoadPrefabsInfo();
            var prefabInfo = allPrefabsInfo.FirstOrDefault(info => info.Path == path);

            var status = prefabInfo?.Status ?? EAUploaderMeta.PrefabStatus.Other;
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
                prefab.Status = (prefab.Status == EAUploaderMeta.PrefabStatus.Pinned) ? EAUploaderMeta.PrefabStatus.Show : EAUploaderMeta.PrefabStatus.Pinned;
                SavePrefabsInfo(allPrefabs);
            }
        }

        public static VRCAvatarDescriptor GetAvatarDescriptor(string prefabPath)
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
            return prefab?.Status == EAUploaderMeta.PrefabStatus.Pinned;
        }

        public static void SavePrefab(GameObject prefab, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(prefab, path);
            ImportPrefab(path);
        }

        /*
            Prefab List API
        */

        public static void CreatePrefabList(string listName)
        {
            if (!prefabLists.ContainsKey(listName))
            {
                prefabLists[listName] = new List<PrefabInfo>();
                SavePrefabLists();
            }
        }

        public static void DeletePrefabList(string listName)
        {
            if (prefabLists.ContainsKey(listName) && listName != "default")
            {
                prefabLists.Remove(listName);
                SavePrefabLists();
            }
        }

        public static void AddPrefabToList(string listName, string prefabPath)
        {
            if (prefabLists.ContainsKey(listName))
            {
                var prefabInfo = CreatePrefabInfo(prefabPath);
                if (!prefabLists[listName].Contains(prefabInfo))
                {
                    prefabLists[listName].Add(prefabInfo);
                    SavePrefabLists();
                }
            }
        }

        public static void RemovePrefabFromList(string listName, string prefabPath)
        {
            if (prefabLists.ContainsKey(listName))
            {
                prefabLists[listName].RemoveAll(p => p.Path == prefabPath);
                SavePrefabLists();
            }
        }

        private static void SavePrefabLists()
        {
            Debug.Log($"Saving prefab lists to {PREFAB_LISTS_PATH}");
            var prefabListItems = prefabLists.Select(kv => new PrefabListItem { ListName = kv.Key, Prefabs = kv.Value }).ToList();
            var wrapper = new PrefabListsWrapper { Lists = prefabListItems };
            string json = JsonUtility.ToJson(wrapper, true);
            Debug.Log($"Prefab lists JSON: {json}");
            File.WriteAllText(PREFAB_LISTS_PATH, json);
            Debug.Log($"Prefab lists saved. JSON: {json}");
        }

        private static void LoadPrefabLists()
        {
            if (File.Exists(PREFAB_LISTS_PATH))
            {
                string json = File.ReadAllText(PREFAB_LISTS_PATH);
                var wrapper = JsonUtility.FromJson<PrefabListsWrapper>(json);
                prefabLists = wrapper.Lists.ToDictionary(item => item.ListName, item => item.Prefabs);
            }
            else
            {
                prefabLists = new Dictionary<string, List<PrefabInfo>>
                {
                    { "default", new List<PrefabInfo>() }
                };
            }
        }
    }

    [Serializable]
    public class PrefabListsWrapper
    {
        public List<PrefabListItem> Lists;
    }

    [Serializable]
    public class PrefabListItem
    {
        public string ListName;
        public List<PrefabInfo> Prefabs;
    }
}