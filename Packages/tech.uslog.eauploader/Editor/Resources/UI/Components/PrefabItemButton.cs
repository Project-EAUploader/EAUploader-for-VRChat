using EAUploader.CustomPrefabUtility;
using EAUploader.UI.Components;
using EAUploader;
using System.IO;
using UnityEngine.UIElements;
using UnityEngine;
using System;

namespace EAUploader.UI.Components
{
    public class PrefabItemButton : Button
    {

        public PrefabItemButton(PrefabInfo prefab, Action clicked_action, bool disabled = false)
        {
            var hasDescriptor = Utility.CheckAvatarHasVRCAvatarDescriptor(PrefabManager.GetPrefab(prefab.Path));
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

                if (disabled)
                {
                    SetEnabled(false);
                }
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
                EnableInClassList("selected", true);
                foreach (var child in parent.Children())
                {
                    if (child != this)
                    {
                        child.EnableInClassList("selected", false);
                    }
                }

                clicked_action();
            };
        }
    }
}