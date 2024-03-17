using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.CustomPrefabUtility
{
    public class RenamePrefabWindow : EditorWindow
    {
        public string FilePath;
        private bool _isChanged;

        public bool ShowWindow(string prefabPath)
        {
            RenamePrefabWindow wnd = GetWindow<RenamePrefabWindow>();
            wnd.FilePath = prefabPath;
            wnd.titleContent = new GUIContent("Prefab�̖��O��ύX");
            wnd.position = new Rect(100, 100, 400, 200);
            wnd.minSize = new Vector2(400, 200);
            wnd.maxSize = wnd.minSize;

            wnd.rootVisualElement.style.unityFont = AssetDatabase.LoadAssetAtPath<UnityEngine.Font>("Assets/EAUploader/UI/Noto_Sans_JP SDF.ttf");

            var visualTree = new VisualElement();
            var newPrefabName = new TextField("�V����Prefab�̖��O")
            {
                value = Path.GetFileNameWithoutExtension(prefabPath)
            };
            visualTree.Add(newPrefabName);

            var renameButton = new Button(() => wnd.Rename(newPrefabName.value)) { text = "���O��ύX" };
            visualTree.Add(renameButton);

            wnd.rootVisualElement.Add(visualTree);
            wnd.ShowModal();

            return wnd._isChanged;
        }

        private void Rename(string newPrefabName)
        {
            if (string.IsNullOrEmpty(newPrefabName))
            {
                EditorUtility.DisplayDialog("�G���[", "�V����Prefab�̖��O�����͂���Ă��܂���", "OK");
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
                EditorUtility.DisplayDialog("�G���[", "���̖��O�̃t�@�C���͊��ɂ���܂��B", "OK");
            }
        }
    }
}