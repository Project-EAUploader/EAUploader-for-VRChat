using EAUploader.CustomPrefabUtility;
using EAUploader.UI.Components;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.Core;
using VRC.SDK3A.Editor;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.Api;

namespace EAUploader.UI.Upload
{
    internal class UploadForm
    {
        public static VisualElement root;
        public static Components.Preview preview;

        public static void ShowContent(VisualElement rootContainer)
        {
            root = rootContainer;
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Upload/Contents/UploadForm");
            visualTree.CloneTree(root);

            root.schedule.Execute(() =>
            {
                var loginStatus = root.Q<VisualElement>("login_status");
                loginStatus.Clear();

                var permissionStatus = root.Q<VisualElement>("permission_status");
                permissionStatus.Clear();

                if (VRC.Core.APIUser.IsLoggedIn)
                {
                    loginStatus.Add(new Label("Logged in as " + VRC.Core.APIUser.CurrentUser.displayName));
                    var uploadMain = root.Q<VisualElement>("upload_main");
                    permissionStatus.style.display = DisplayStyle.Flex;
                }
                else
                {
                    loginStatus.Add(new Label("You need to login to upload avatar"));
                    loginStatus.Add(new Button(() =>
                    {
                        VRCSdkControlPanel.GetWindow<VRCSdkControlPanel>().Show();
                    })
                    {
                        text = "Login"
                    });

                    var uploadMain = root.Q<VisualElement>("upload_main");
                    uploadMain.style.display = DisplayStyle.None;
                    permissionStatus.style.display = DisplayStyle.None;
                }

                if (APIUser.CurrentUser != null && APIUser.CurrentUser.canPublishAvatars)
                {
                    permissionStatus.Add(new Label("You have permission to upload avatar"));
                    var uploadMain = root.Q<VisualElement>("upload_main");
                    uploadMain.style.display = DisplayStyle.Flex;
                }
                else
                {
                    permissionStatus.Add(new Label("You don't have permission to upload avatar"));
                    var uploadMain = root.Q<VisualElement>("upload_main");
                    uploadMain.style.display = DisplayStyle.None;
                }

                if (EAUploaderCore.selectedPrefabPath != null)
                {
                    OnSelectedPrefabPathChanged(EAUploaderCore.selectedPrefabPath);
                }
                
            }).Every(1000);

            root.Q<Button>("build").clicked += Build;
            root.Q<Button>("upload").clicked += Upload;
        }

        private static void OnSelectedPrefabPathChanged(string path)
        {
            if (path != null)
            {
                var avatarThumbnail = root.Q<Image>("thumbnail-image");
                var previewImage = PrefabPreview.GetPrefabPreview(path);
                avatarThumbnail.image = previewImage;
            }
        }

        private static void Build()
        {
            Debug.Log("Build button clicked");
            var selectedPrefabPath = EAUploaderCore.selectedPrefabPath;

            if (selectedPrefabPath != null)
            {
                Debug.Log("Building avatar");

                AvatarUploader.BuildAvatar();

            }
        }

        private static void Upload()
        {
            Debug.Log("Upload button clicked");
            var selectedPrefabPath = EAUploaderCore.selectedPrefabPath;

            var contentName = root.Q<TextFieldPro>("content-name").GetValue();
            var contentDescription = root.Q<TextFieldPro>("content-description").GetValue();
            var releaseStatus = root.Q<DropdownField>("release-status").value;

            if (contentName == null || contentDescription == null)
            {
                return;
            }

            if (selectedPrefabPath != null)
            {
                Debug.Log("Uploading avatar");

                VRCAvatar avatar = new VRCAvatar()
                {
                    Name = contentName,
                    Description = contentDescription,
                    ReleaseStatus = releaseStatus.ToLower(),
                    Tags = new System.Collections.Generic.List<string>()
                };

                var previewImage = PrefabPreview.GetPreviewImagePath(selectedPrefabPath);

                AvatarUploader.UploadAvatar(avatar, previewImage);

                var uploadStatus = root.Q<VisualElement>("upload_status");
                uploadStatus.Clear();

                uploadStatus.Add(new Label("Uploading avatar..."));
                var progress = new ProgressBar()
                {
                    lowValue = 0,
                    highValue = 1,
                };
                uploadStatus.Add(progress);

                uploadStatus.schedule.Execute(() =>
                {
                    if (AvatarUploader.Status != null)
                    {
                        progress.value = AvatarUploader.Percentage;
                        progress.title = AvatarUploader.Status;
                    } 
                }).Every(1000);
            }
        }
    }
}