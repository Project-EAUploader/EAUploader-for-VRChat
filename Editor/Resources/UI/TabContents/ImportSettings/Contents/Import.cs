using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader_beta.UI.ImportSettings
{
    internal class Import
    {
        public static void ShowContent(VisualElement root)
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/ImportSettings/Contents/Import");
            visualTree.CloneTree(root);

            if (EAUploaderCore.HasVRM)
            {
                root.Q<Button>("import_vrm").SetEnabled(true);
            }
            else
            {
                root.Q<Button>("import_vrm").SetEnabled(false);
            }

            root.Q<Button>("import_prefab").clicked += ImportPrefabButtonClicked;
            root.Q<Button>("import_folder").clicked += ImportFolderButtonClicked;
            root.Q<Button>("import_vrm").clicked += ImportVRMButtonClicked;
        }

        private static void ImportPrefabButtonClicked()
        {
            ImportAsset(EditorUtility.OpenFilePanelWithFilters("Assetのインポート", "", new[] { "PrefabかUnitypackageを選択", "prefab,unitypackage", "All files", "*" }));
        }

        private static void ImportFolderButtonClicked()
        {
            ImportAllAssetsFromFolder(EditorUtility.OpenFolderPanel("Assetをフォルダで複数インポート", "", ""));
        }

        private static void ImportVRMButtonClicked()
        {
            EditorApplication.ExecuteMenuItem("VRM0/Import from VRM 0.x");
        }

        private static void ImportAsset(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            var fileExtension = Path.GetExtension(filePath)?.ToLower();
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            var existingAsset = AssetDatabase.FindAssets($"t:GameObject {fileName}");
            var existingAssetNames = string.Join("\n", existingAsset.Select(AssetDatabase.GUIDToAssetPath));

            if (existingAsset.Length > 0)
            {
                var overwrite = EditorUtility.DisplayDialog("同じ名前のAssetが見つかりました", $"{fileName} という名前のAssetが既に存在します。\n続行しますか？\n{existingAssetNames}", "上書き", "キャンセル");
                if (!overwrite) return;
            }

            switch (fileExtension)
            {
                case ".prefab":
                    AssetDatabase.ImportAsset(filePath, ImportAssetOptions.Default);
                    break;
                case ".unitypackage":
                    AssetDatabase.ImportPackage(filePath, true);
                    break;
            }
            AssetDatabase.Refresh();
        }

        private static void ImportAllAssetsFromFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return;

            var allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".prefab") || s.EndsWith(".unitypackage")).ToList();

            if (allFiles.Count == 0)
            {
                EditorUtility.DisplayDialog("Assetが見つかりません", "選択されたフォルダにprefab や unityパッケージファイル がありません。", "削除");
                return;
            }

            var fileList = string.Join("\n", allFiles.Select(Path.GetFileName));
            var confirmImport = EditorUtility.DisplayDialog("インポートの確認", $"以下のファイルがインポートされます:\n{fileList}", "インポート", "キャンセル");
            if (!confirmImport) return;

            foreach (var file in allFiles)
            {
                ImportAsset(file);
            }
        }
    }
}
