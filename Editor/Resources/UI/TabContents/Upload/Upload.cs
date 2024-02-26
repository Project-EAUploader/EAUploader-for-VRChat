using EAUploader.CustomPrefabUtility;
using EAUploader.UI.Components;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3A.Editor;

namespace EAUploader.UI.Upload
{
    internal class Main
    {
        public static VisualElement root;
        public static Components.Preview preview;
        public static VRCSdkControlPanelAvatarBuilder _builder;

        public static void ShowContent(VisualElement rootContainer)
        {
            root = rootContainer;
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Upload/Upload");
            visualTree.CloneTree(root);

            UploadForm.ShowContent(root.Q("upload_form"));

            preview = new Components.Preview(root.Q("avatar_preview"), EAUploaderCore.selectedPrefabPath);
            preview.ShowContent();

            CreatePrefabList();
        }

        private static void CreatePrefabList()
        {
            var prefabList = PrefabManager.GetAllPrefabsWithPreview();
            var prefabListContainer = root.Q("prefab_list_container");

            foreach (var prefab in prefabList)
            {
                var hasDescriptor = Utility.CheckAvatarHasVRCAvatarDescriptor(PrefabManager.GetPrefab(prefab.Path));
                var prefabButton = new PrefabItemButton(prefab, hasDescriptor);
                prefabListContainer.Add(prefabButton);
            }
        }
    }

    internal class PrefabItemButton : Button
    {
        public PrefabItemButton(PrefabInfo prefab, bool hasDescriptor)
        {
            var previewImage = new Image { image = prefab.Preview, scaleMode = ScaleMode.ScaleToFit, style = { width = 100, height = 100 } };
            Add(previewImage);

            var labelContainer = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                }
            };

            var label = new Label(Path.GetFileNameWithoutExtension(prefab.Path));
            labelContainer.Add(label);

            if (!hasDescriptor)
            {
                var warning = new VisualElement()
                {
                    style = {
                        flexDirection = FlexDirection.Row,
                    }
                };
                warning.AddToClassList("noDescriptor");
                var warningIcon = new MaterialIcon { icon = "warning" };
                var warningLabel = new Label(T7e.Get("No VRCAvatarDescriptor"));
                warning.Add(warningIcon);
                warning.Add(warningLabel);
                labelContainer.Add(warning);
                SetEnabled(false);
            }

            Add(labelContainer);

            if (EAUploaderCore.selectedPrefabPath == prefab.Path)
            {
                EnableInClassList("selected", true);
            }

            if (PrefabManager.IsPinned(prefab.Path))
            {
                EnableInClassList("pinned", true);
            }

            clicked += () =>
            {
                EAUploaderCore.selectedPrefabPath = prefab.Path;
                Main.preview.UpdatePreview(prefab.Path);
                EnableInClassList("selected", true);

                foreach (var child in parent.Children())
                {
                    if (child != this)
                    {
                        child.EnableInClassList("selected", false);
                    }
                }
            };
        }
    }
}