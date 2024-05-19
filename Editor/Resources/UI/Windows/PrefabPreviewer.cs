using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using EAUploader;
using EAUploader.UI.Components;
using EAUploader.CustomPrefabUtility;

namespace EAUploader.UI.Windows
{
    public class PrefabPreviewer : EditorWindow
    {
        private static PrefabPreviewer previewWindow;
        private string prefabPath;
        private Image preview;
        private static Texture2D _image;

        public static void ShowLargeImage(string prefabPath, Texture2D image)
        {
            _image = image;

            if (previewWindow == null)
            {
                previewWindow = CreateInstance<PrefabPreviewer>();
                previewWindow.titleContent = new GUIContent(UnityEditor.L10n.Tr("Preview"));
                previewWindow.minSize = new Vector2(500, 500);
                previewWindow.prefabPath = prefabPath;
                previewWindow.CreateGUI();
                previewWindow.UpdatePreview();
                previewWindow.Show();
            }
            else
            {
                if (previewWindow.titleContent.text == UnityEditor.L10n.Tr("Preview"))
                {
                    previewWindow.Focus();
                    previewWindow.prefabPath = prefabPath;
                    if (previewWindow.preview == null)
                    {
                        previewWindow.CreateGUI();
                    }
                    previewWindow.UpdatePreview();
                }
                else
                {
                    previewWindow = null;
                    ShowLargeImage(prefabPath, image);
                }
            }
        }

        private void CreateGUI()
        {
            rootVisualElement.Clear();

            var visualTree = Resources.Load<VisualTreeAsset>("UI/Windows/PrefabPreviewer");
            visualTree.CloneTree(rootVisualElement);

            rootVisualElement.styleSheets.Add(EAUploader.styles);
            rootVisualElement.styleSheets.Add(EAUploader.tailwind);

            preview = rootVisualElement.Q<Image>("preview");
            preview.image = _image;

            var regenerateButton = rootVisualElement.Q<ShadowButton>("regenerate");
            regenerateButton.clicked += RegeneratePreview;

            LanguageUtility.Localization(rootVisualElement);
        }

        private void UpdatePreview()
        {
            preview.image = _image;
        }

        private void RegeneratePreview()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                Texture2D previewTexture = PrefabPreview.GeneratePreview(prefab);
                _image = previewTexture;
                UpdatePreview();
                PrefabPreview.SavePrefabPreview(prefabPath, previewTexture);
            }
        }
    }
}