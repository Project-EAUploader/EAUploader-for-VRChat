using EAUploader.CustomPrefabUtility;
using EAUploader.UI.Components;
using System.IO;
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
        public static bool isCloned = false;

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
            else
            {
                var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Upload/Contents/UploadForm");
                visualTree.CloneTree(root);

                isCloned = true;

                root.Q<ShadowButton>("buildandtest").clicked += BuildAndTest;
                root.Q<ShadowButton>("upload").clicked += Upload;

                Validate();
            }

            var loginStatus = root.Q<VisualElement>("login_status");
            var permissionStatus = root.Q<VisualElement>("permission_status");
            var uploadMain = root.Q<VisualElement>("upload_main");

            UpdateStatus(loginStatus, permissionStatus, uploadMain);

            var isLoggined = APIUser.IsLoggedIn;
            root.schedule.Execute(() =>
            {
                if (isLoggined != APIUser.IsLoggedIn)
                {
                    isLoggined = APIUser.IsLoggedIn;
                    UpdateStatus(loginStatus, permissionStatus, uploadMain);
                }
            }).Every(1000);
        }

        private static void OnSelectedPrefabPathChanged(string path)
        {
            if (path != null)
            {
                if (!isCloned)
                {
                    root.Clear();
                    var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Upload/Contents/UploadForm");
                    visualTree.CloneTree(root);
                    isCloned = true;
                }

                var avatarThumbnail = root.Q<Image>("thumbnail-image");
                var previewImage = PrefabPreview.GetPrefabPreview(path);
                avatarThumbnail.image = previewImage;

                root.Q<ShadowButton>("buildandtest").clicked += BuildAndTest;
                root.Q<ShadowButton>("upload").clicked += Upload;

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

        private static void UpdateStatus(VisualElement loginStatus, VisualElement permissionStatus, VisualElement uploadMain)
        {
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

            if (validateResult.Open != null)
            {
                var button = new ShadowButton()
                {
                    text = "Select"
                };

                button.clicked += validateResult.Open;
                Add(button);
            }
        }
    }
}