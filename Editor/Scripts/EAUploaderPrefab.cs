using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static CustomPrefabUtility;

namespace EAUploader_beta.Prefab
{
    public class PrefabManager
    {
        private const string PREFABS_INFO_PATH = "Assets/EAUploader/PrefabManager.json";
        private const string PREVIEW_SAVE_PATH = "Assets/EAUploader/PrefabPreviews";

        public static List<PrefabInfo> LoadPrefabsInfo(string filePath)
        {
            if (!File.Exists(filePath)) return new List<PrefabInfo>();

            string json = File.ReadAllText(filePath);
            PrefabInfoList prefabList = JsonUtility.FromJson<PrefabInfoList>(json);
            return prefabList.Prefabs;
        }

        public static Dictionary<string, Texture2D> GetPrefabList()
        {
            prefabsWithPreview.Clear();
            var allPrefabs = LoadPrefabsInfo(PREFABS_INFO_PATH)
                                .OrderByDescending(p => p.Status == "editing")
                                .Where(p => p.Status != "hidden")
                                .ToList();

            foreach (var prefab in allPrefabs)
            {
                string previewImagePath = Path.Combine(PREVIEW_SAVE_PATH, Path.GetFileNameWithoutExtension(prefab.Path) + ".png");
                if (File.Exists(previewImagePath))
                {
                    Texture2D preview = LoadTextureFromFile(previewImagePath);
                    prefabsWithPreview[prefab.Path] = preview;
                }
            }
            return prefabsWithPreview;
        }

        private static Texture2D LoadTextureFromFile(string filePath)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            return texture;
        }

    }

    internal class RenamePrefab : EditorWindow
    {
        public string FilePath;
        private bool _isChanged;

        public static bool ShowWindow(string prefabPath)
        {
            RenamePrefab wnd = GetWindow<RenamePrefab>();
            wnd.FilePath = prefabPath;
            wnd.titleContent = new GUIContent("Prefabの名前を変更");
            wnd.position = new Rect(100, 100, 400, 200);
            wnd.minSize = new Vector2(400, 200);
            wnd.maxSize = wnd.minSize;

            wnd.rootVisualElement.style.unityFont = Resources.Load<Font>("Fonts/NotoSansJP-Regular");

            var visualTree = new VisualElement();
            var newPrefabName = new TextField("新しいPrefabの名前");
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
}
