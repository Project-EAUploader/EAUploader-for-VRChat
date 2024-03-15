using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

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
            PrefabInfoList prefabList = new PrefabInfoList { Prefabs = prefabs };
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
                if (prefab.Status == PrefabStatus.Pinned)
                {
                    prefab.Status = PrefabStatus.Show;
                }
                else
                {
                    prefab.Status = PrefabStatus.Pinned;
                }
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
            return prefab?.Status == PrefabStatus.Pinned;
        }
    }

    public class RenamePrefabWindow : EditorWindow
    {
        public string FilePath;
        private bool _isChanged;

        public bool ShowWindow(string prefabPath)
        {
            RenamePrefabWindow wnd = GetWindow<RenamePrefabWindow>();
            wnd.FilePath = prefabPath;
            wnd.titleContent = new GUIContent("Prefabの名前を変更");
            wnd.position = new Rect(100, 100, 400, 200);
            wnd.minSize = new Vector2(400, 200);
            wnd.maxSize = wnd.minSize;

            wnd.rootVisualElement.style.unityFont = AssetDatabase.LoadAssetAtPath<UnityEngine.Font>("Assets/EAUploader/UI/Noto_Sans_JP SDF.ttf");

            var visualTree = new VisualElement();
            var newPrefabName = new TextField("新しいPrefabの名前")
            {
                value = Path.GetFileNameWithoutExtension(prefabPath)
            };
            visualTree.Add(newPrefabName);

            var renameButton = new Button(() => wnd.Rename(newPrefabName.value)) { text = "名前を変更" };
            visualTree.Add(renameButton);

            wnd.rootVisualElement.Add(visualTree);
            wnd.ShowModal();

            return wnd._isChanged;
        }

        private void Rename(string newPrefabName)
        {
            if (string.IsNullOrEmpty(newPrefabName))
            {
                EditorUtility.DisplayDialog("エラー", "新しいPrefabの名前が入力されていません", "OK");
                return;
            }

            string directory = Path.GetDirectoryName(FilePath);
            string newFilePath = Path.Combine(directory, newPrefabName + Path.GetExtension(FilePath));

            if (!File.Exists(newFilePath))
            {
                AssetDatabase.MoveAsset(FilePath, newFilePath);

                string previewImagePath = PrefabPreview.GetPreviewImagePath(FilePath);
                string newPreviewImagePath = Path.Combine(Path.GetDirectoryName(previewImagePath), newPrefabName + ".png");

                if (File.Exists(previewImagePath))
                {
                    try
                    {
                        Debug.Log($"Moving preview image: {previewImagePath} -> {newPreviewImagePath}");
                        if (File.Exists(newPreviewImagePath))
                        {
                            File.Delete(newPreviewImagePath);
                        }
                        File.Move(previewImagePath, newPreviewImagePath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error moving preview image: {e.Message}");
                    }
                }

                AssetDatabase.Refresh();
                _isChanged = true;
                Close();
            }
            else
            {
                EditorUtility.DisplayDialog("エラー", "この名前のファイルは既にあります。", "OK");
            }
        }
    }

    public class PrefabPreview
    {
        private const string EAUPLOADER_SCENE_PATH = "Assets/EAUploader.unity";
        private const string PREVIEW_SAVE_PATH = "Assets/EAUploader/PrefabPreviews";

        public static Texture2D GeneratePreview(GameObject prefab)
        {
            var previewRenderUtility = new PreviewRenderUtility();
            var rect = new Rect(0, 0, 1080, 1080);

            var gameObject = previewRenderUtility.InstantiatePrefabInScene(prefab);

            Bounds bounds = CalculateBounds(gameObject);

            previewRenderUtility.BeginStaticPreview(rect);
            previewRenderUtility.AddSingleGO(gameObject);

            previewRenderUtility.camera.backgroundColor = new UnityEngine.Color(0.9f, 0.9f, 0.9f, 1);
            previewRenderUtility.camera.clearFlags = CameraClearFlags.SolidColor;
            previewRenderUtility.camera.orthographic = true;
            previewRenderUtility.camera.orthographicSize = bounds.size.y / 2;

            float size = bounds.size.magnitude;
            float distance = size / (4 * Mathf.Tan(previewRenderUtility.camera.fieldOfView * 0.5f * Mathf.Deg2Rad));
            previewRenderUtility.camera.transform.position = bounds.center + previewRenderUtility.camera.transform.forward * distance;
            previewRenderUtility.camera.transform.LookAt(bounds.center);

            previewRenderUtility.Render();

            Texture2D texture = previewRenderUtility.EndStaticPreview();

            previewRenderUtility.Cleanup();

            return texture;
        }

        private static Bounds CalculateBounds(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

        public static void GenerateAndSaveAllPrefabPreviews()
        {
            var allPrefabs = PrefabManager.GetAllPrefabs();

            foreach (var prefabInfo in allPrefabs)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabInfo.Path);
                if (prefab != null)
                {
                    Texture2D preview = GeneratePreview(prefab);
                    SavePrefabPreview(prefabInfo.Path, preview);
                }
            }
        }

        internal static void SavePrefabPreview(string prefabPath, Texture2D preview)
        {
            string fileName = Path.GetFileNameWithoutExtension(prefabPath);
            string savePath = Path.Combine(PREVIEW_SAVE_PATH, $"{fileName}.png");

            string directoryPath = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                byte[] pngData = preview.EncodeToPNG();
                File.WriteAllBytes(savePath, pngData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving preview for prefab: {prefabPath}. \nError: {e.Message}");
                return;
            }
        }

        internal static Texture2D LoadTextureFromFile(string filePath)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            return texture;
        }

        public static void SaveTextureToFile(Texture2D texture, string filePath)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
        }

        public static string GetPreviewImagePath(string prefabPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(prefabPath);
            return Path.Combine(PREVIEW_SAVE_PATH, $"{fileName}.png");
        }


        public static Texture2D GetPrefabPreview(string prefabPath)
        {
            string previewImagePath = PrefabPreview.GetPreviewImagePath(prefabPath);
            if (File.Exists(previewImagePath))
            {
                return PrefabPreview.LoadTextureFromFile(previewImagePath);
            }
            return null;
        }
    }

    public class Utility
    {
        public static float GetAvatarHeight(GameObject avatar)
        {
            var avatarDescriptor = avatar.GetComponent<VRC_AvatarDescriptor>();
            if (avatarDescriptor != null)
            {
                // ViewPosition.y がアバターの目線の高さ
                return avatarDescriptor.ViewPosition.y;
            }

            // デフォルト
            return 0f;
        }

        public static bool CheckAvatarHasVRCAvatarDescriptor(GameObject avatar)
        {
            if (avatar == null) return false;
            return avatar.GetComponent<VRC_AvatarDescriptor>() != null;
        }

        public static VRC_AvatarDescriptor GetAvatarDescriptor(GameObject avatar)
        {
            return avatar.GetComponent<VRC_AvatarDescriptor>();
        }
    }
}
