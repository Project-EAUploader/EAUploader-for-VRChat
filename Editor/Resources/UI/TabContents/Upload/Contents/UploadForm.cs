using EAUploader.CustomPrefabUtility;
using EAUploader.UI.Components;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.Core;
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

            root.Q<Label>("loginSDKLabel").text = T7e.Get("Login to VRChat");
            root.Q<Label>("checkPermissionLabel").text = T7e.Get("Check you are allowed to upload avatar");
            root.Q<Label>("modelInfoLabel").text = T7e.Get("Model Info");
            root.Q<Label>("addThumbnailLabel").text = T7e.Get("Set thumbnail");
            root.Q<Label>("buildLabel").text = T7e.Get("Build");
            root.Q<Label>("buildandtestLabel").text = T7e.Get("Build as Test");
            root.Q<Label>("uploadLabel").text = T7e.Get("Upload");

            root.Q<TextFieldPro>("content-name").label = T7e.Get("Name");
            root.Q<TextFieldPro>("content-name").placeholder = T7e.Get("Give your avatar a name");
            root.Q<TextFieldPro>("content-description").label = T7e.Get("Description");
            root.Q<TextFieldPro>("content-description").placeholder = T7e.Get("Describe your avatar so it is easier to remember!");
            root.Q<ContentWarningsField>("content-warnings").label = T7e.Get("Content Warnings");
            root.Q<DropdownField>("release-status").label = T7e.Get("Visibility");

            root.schedule.Execute(() =>
            {
                var loginStatus = root.Q<VisualElement>("login_status");
                loginStatus.Clear();

                var permissionStatus = root.Q<VisualElement>("permission_status");
                permissionStatus.Clear();

                if (VRC.Core.APIUser.IsLoggedIn)
                {
                    loginStatus.Add(new Label(T7e.Get("Logged in as ") + VRC.Core.APIUser.CurrentUser.displayName));
                    var uploadMain = root.Q<VisualElement>("upload_main");
                    permissionStatus.style.display = DisplayStyle.Flex;
                }
                else
                {
                    loginStatus.Add(new Label(T7e.Get("You need to login to upload avatar")));

                    var loginButton = new ShadowButton()
                    {
                        text = T7e.Get("Login")
                    };

                    loginButton.clicked += () =>
                    {
                        VRCSdkControlPanel.GetWindow<VRCSdkControlPanel>().Show();
                    };
                    loginStatus.Add(loginButton);

                    var uploadMain = root.Q<VisualElement>("upload_main");
                    uploadMain.style.display = DisplayStyle.None;
                    permissionStatus.style.display = DisplayStyle.None;
                }

                if (APIUser.CurrentUser != null && APIUser.CurrentUser.canPublishAvatars)
                {
                    permissionStatus.Add(new Label(T7e.Get("You have permission to upload avatar")));
                    var uploadMain = root.Q<VisualElement>("upload_main");
                    uploadMain.style.display = DisplayStyle.Flex;
                }
                else
                {
                    permissionStatus.Add(new Label(T7e.Get("You cannot upload an avatar with your current trust rank. You can immediately increase your rank by spending time on VRChat and adding friends. Or you can join VRChat+ (for a fee) to raise your rank immediately.")));
                    var uploadMain = root.Q<VisualElement>("upload_main");
                    uploadMain.style.display = DisplayStyle.None;
                }

                if (EAUploaderCore.selectedPrefabPath != null)
                {
                    OnSelectedPrefabPathChanged(EAUploaderCore.selectedPrefabPath);
                }
            }).Every(1000);

            root.Q<ShadowButton>("build").clicked += Build;
            root.Q<ShadowButton>("buildandtest").clicked += BuildAndTest;
            root.Q<ShadowButton>("upload").clicked += Upload;
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

        private static void BuildAndTest()
        {
            Debug.Log("BuildandTest button clicked");
            var selectedPrefabPath = EAUploaderCore.selectedPrefabPath;

            if (selectedPrefabPath != null)
            {
                Debug.Log("Building avatar as Test");

                AvatarUploader.BuildAndTest();
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

                var tags = root.Q<UI.Components.ContentWarningsField>("content-warnings").Tags;

                VRCAvatar avatar = new VRCAvatar()
                {
                    Name = contentName,
                    Description = contentDescription,
                    ReleaseStatus = releaseStatus.ToLower(),
                    Tags = tags,
                };

                var previewImage = PrefabPreview.GetPreviewImagePath(selectedPrefabPath);

                AvatarUploader.UploadAvatar(avatar, previewImage);

                var uploadStatus = root.Q<VisualElement>("upload_status");
                uploadStatus.Clear();

                uploadStatus.Add(new Label(T7e.Get("Uploading avatar...")));
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