using EAUploader.CustomPrefabUtility;
using EAUploader.UI.Components;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.Core;
using VRC.SDKBase.Editor.Api;

namespace EAUploader.UI.Upload
{
    internal class UploadForm
    {
        private static VisualElement root;
        private static bool isCloned = false;
        private static string thumbnailUrl = null;

        public static void ShowContent(VisualElement rootContainer)
        {
            isCloned = false;
            root = rootContainer;

            EAUploaderCore.SelectedPrefabPathChanged += OnSelectedPrefabPathChanged;

            if (EAUploaderCore.selectedPrefabPath == null)
            {
                root.Add(new Label(T7e.Get("Select a prefab to upload")));
                return;
            }

            var hasDescriptor = Utility.CheckAvatarHasVRCAvatarDescriptor(PrefabManager.GetPrefab(EAUploaderCore.selectedPrefabPath));

            if (!hasDescriptor)
            {
                root.Q("upload_form").Add(new Label(T7e.Get("No VRCAvatarDescriptor")));
                return;
            }

            CloneVisualTree();

            Validate();
            UpdateStatus();
        }

        private static void CloneVisualTree()
        {
            if (!isCloned)
            {
                root.Clear();
                var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Upload/Contents/UploadForm");
                visualTree.CloneTree(root);
                isCloned = true;

                BindButtons();
            }
        }

        private static void BindButtons()
        {
            root.Q<ShadowButton>("buildandtest").clicked += BuildAndTest;
            root.Q<ShadowButton>("upload").clicked += Upload;
            root.Q<ShadowButton>("add_thumbnail").clicked += AddThumbnail;
            root.Q<ShadowButton>("remove_thumbnail").clicked += RemoveThumbnail;
        }

        private static void AddThumbnail()
        {
            var path = EditorUtility.OpenFilePanel("Select Thumbnail", "", "png,jpg,jpeg");
            if (path.Length != 0)
            {
                var avatarThumbnail = root.Q<Image>("thumbnail-image");
                var texture = new Texture2D(2, 2);
                texture.LoadImage(File.ReadAllBytes(path));
                avatarThumbnail.image = texture;
                thumbnailUrl = path;
            }
        }

        private static void RemoveThumbnail()
        {
            var avatarThumbnail = root.Q<Image>("thumbnail-image");
            avatarThumbnail.image = PrefabPreview.GetPrefabPreview(EAUploaderCore.selectedPrefabPath);
            thumbnailUrl = null;
        }

        private static void OnSelectedPrefabPathChanged(string path)
        {
            if (path != null)
            {
                CloneVisualTree();

                var avatarThumbnail = root.Q<Image>("thumbnail-image");
                var previewImage = PrefabPreview.GetPrefabPreview(path);
                avatarThumbnail.image = previewImage;

                Validate();
            }
        }

        private static void Validate()
        {
            var avatarDescriptor = Utility.GetAvatarDescriptor(PrefabManager.GetPrefab(EAUploaderCore.selectedPrefabPath));
            var avatarUploader = new AvatarUploader();
            var validations = avatarUploader.CheckAvatarForValidationIssues(avatarDescriptor);
            var validationList = root.Q<ScrollView>("validation_list");
            validationList.Clear();
            foreach (var validation in validations)
            {
                var validationItem = new ValidationItem(validation);
                validationList.Add(validationItem);
            }
        }

        private static void UpdateStatus()
        {
            var loginStatus = root.Q<VisualElement>("login_status");
            var permissionStatus = root.Q<VisualElement>("permission_status");
            var uploadMain = root.Q<VisualElement>("upload_main");

            loginStatus.Clear();
            permissionStatus.Clear();

            if (VRC.Core.APIUser.IsLoggedIn)
            {
                loginStatus.Add(new Label(T7e.Get("Logged in as ") + VRC.Core.APIUser.CurrentUser.displayName));
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

                uploadMain.style.display = DisplayStyle.None;
                permissionStatus.style.display = DisplayStyle.None;
            }

            if (APIUser.CurrentUser != null && APIUser.CurrentUser.canPublishAvatars)
            {
                permissionStatus.Add(new Label(T7e.Get("You have permission to upload avatar")));
                uploadMain.style.display = DisplayStyle.Flex;
            }
            else
            {
                permissionStatus.Add(new Label(T7e.Get("You cannot upload an avatar with your current trust rank. You can immediately increase your rank by spending time on VRChat and adding friends. Or you can join VRChat+ (for a fee) to raise your rank immediately.")));
                uploadMain.style.display = DisplayStyle.None;
            }

            if (EAUploaderCore.selectedPrefabPath != null)
            {
                OnSelectedPrefabPathChanged(EAUploaderCore.selectedPrefabPath);
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

            if (string.IsNullOrEmpty(contentName) || string.IsNullOrEmpty(contentDescription))
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

                string thumbnailPath = PrefabPreview.GetPreviewImagePath(selectedPrefabPath);

                if (thumbnailUrl != null)
                {
                    thumbnailPath = thumbnailUrl;
                }

                AvatarUploader.UploadAvatar(avatar, thumbnailPath);

                /*
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
                */
            }
        }
    }

    public class ValidationItem : VisualElement
    {
        public ValidationItem(AvatarUploader.ValidateResult validateResult)
        {
            switch (validateResult.ResultType)
            {
                case AvatarUploader.ValidateResult.ValidateResultType.Error:
                    AddToClassList("error");
                    break;
                case AvatarUploader.ValidateResult.ValidateResultType.Warning:
                    AddToClassList("warning");
                    break;
                case AvatarUploader.ValidateResult.ValidateResultType.Info:
                    AddToClassList("info");
                    break;
                case AvatarUploader.ValidateResult.ValidateResultType.Success:
                    AddToClassList("success");
                    break;
                case AvatarUploader.ValidateResult.ValidateResultType.Link:
                    AddToClassList("link");
                    break;
            }
            var message = validateResult.ResultMessage;
            Add(new Label(message));

            if (validateResult.ResultType == AvatarUploader.ValidateResult.ValidateResultType.Link)
            {
                var button = new ShadowButton()
                {
                    text = "Open Link"
                };

                button.clicked += () =>
                {
                    Application.OpenURL(validateResult.Link);
                };
                Add(button);
            }
        }
    }
}
