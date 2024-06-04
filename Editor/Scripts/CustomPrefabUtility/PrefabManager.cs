using EAUploader.Components;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace EAUploader.CustomPrefabUtility
{
    public class PrefabManager
    {
        private const string PREFABS_INFO_PATH = "Assets/EAUploader/PrefabManager.json";

        public static List<PrefabInfo> prefabInfoList;

        public static async void Initialize()
        {
            await UpdatePrefabInfoAsync();
            await PrefabPreview.GenerateAndSaveAllPrefabPreviewsAsync();
        }

        public static async Task UpdatePrefabInfoAsync()
        {
            List<PrefabInfo> allPrefabs = await GetAllPrefabsAsync();

            UnityEditor.EditorApplication.delayCall += () =>
            {
                allPrefabs = allPrefabs
                    .OrderByDescending(p => p.Status == EAUploaderMeta.PrefabStatus.Pinned)
                    .ThenByDescending(p => p.LastModified)
                    .ToList();

                SavePrefabsInfo(allPrefabs);

                if (prefabInfoList == null)
                {
                    prefabInfoList = allPrefabs;
                }
            };
        }

        public static async void ImportPrefab(string prefabPath)
        {
            GameObject prefab = GetPrefab(prefabPath);
            var existingMeta = prefab.GetComponent<EAUploaderMeta>();
            if (existingMeta == null)
            {
                var meta = prefab.AddComponent<EAUploaderMeta>();
                meta.type = GetPrefabType(prefabPath);
            }

            Texture2D preview = await PrefabPreview.GeneratePreviewAsync(prefab);
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

        internal static async Task<List<PrefabInfo>> GetAllPrefabsAsync(System.Action<PrefabInfo> onPrefabInfoAdded = null)
        {
            List<string> prefabGuids = new List<string>();
            List<PrefabInfo> prefabInfos = new List<PrefabInfo>();

            await Task.Run(() =>
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }).ToList();

                    foreach (string guid in prefabGuids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        PrefabInfo prefabInfo = CreatePrefabInfo(path);
                        prefabInfos.Add(prefabInfo);
                        onPrefabInfoAdded?.Invoke(prefabInfo);
                    }
                };
            });

            return prefabInfos.OrderBy(p => p.LastModified).ToList();
        }

        public static async Task<List<PrefabInfo>> GetAllPrefabsWithPreviewAsync(System.Action<PrefabInfo> onPrefabInfoAdded = null)
        {
            var allPrefabs = await GetAllPrefabsAsync(onPrefabInfoAdded);
            allPrefabs = allPrefabs
                .Where(p => p.Status != EAUploaderMeta.PrefabStatus.Hidden)
                .OrderByDescending(p => p.Status == EAUploaderMeta.PrefabStatus.Pinned)
                .ThenByDescending(p => p.LastModified)
                .ToList();

            await Task.Run(() =>
            {
                foreach (var prefab in allPrefabs)
                {
                    string previewImagePath = PrefabPreview.GetPreviewImagePath(prefab.Path);
                    if (File.Exists(previewImagePath))
                    {
                        prefab.Preview = PrefabPreview.LoadTextureFromFile(previewImagePath);
                    }
                }
            });

            return allPrefabs;
        }

        public static async Task<List<PrefabInfo>> GetAllPrefabsIncludingHiddenAsync(System.Action<PrefabInfo> onPrefabInfoAdded = null)
        {
            var allPrefabs = await GetAllPrefabsAsync(onPrefabInfoAdded);

            UnityEditor.EditorApplication.delayCall += () =>
            {
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
            };

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
    }
}