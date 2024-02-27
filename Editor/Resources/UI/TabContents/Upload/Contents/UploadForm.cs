using EAUploader.CustomPrefabUtility;
using EAUploader.UI.Components;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

        private static readonly Dictionary<string, string> BUILD_TARGET_ICONS = new Dictionary<string, string>
        {
            { "Windows", "fab fa-windows" },
            { "Android", "fab fa-android" },
            { "iOS", "fab fa-apple" }
        };

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
            root.Q<ShadowButton>("buildandtest").clicked += async () => await BuildAndTestAsync();
            root.Q<ShadowButton>("upload").clicked += async () => await UploadAsync();
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

                var switcherBlock = root.Q("platform-switcher");
                if (switcherBlock.Q("platform-switcher-popup") == null)
                {
                    var options = GetBuildTargetOptions();
                    var currentTarget = GetCurrentBuildTarget();
                    var selectedIndex = options.IndexOf(currentTarget);
                    if (!BUILD_TARGET_ICONS.TryGetValue(currentTarget, out var iconClass))
                    {
                        iconClass = "";
                    }
                    if (selectedIndex == -1)
                    {
                        selectedIndex = 0;
                    }
                    var popup = new PopupField<string>("Selected Platform", options, selectedIndex)
                    {
                        name = "platform-switcher-popup"
                    };
                    var icon = new VisualElement();
                    icon.AddToClassList("icon");
                    icon.AddToClassList(iconClass);

                    popup.hierarchy.Insert(0, icon);
                    popup.schedule.Execute(() =>
                    {
                        currentTarget = GetCurrentBuildTarget();
                        popup.SetValueWithoutNotify(currentTarget);
                    }).Every(500);
                    popup.RegisterValueChangedCallback(evt =>
                    {
                        switch (evt.newValue)
                        {
                            case "Windows":
                                {
                                    if (EditorUtility.DisplayDialog("Build Target Switcher", "Are you sure you want to switch your build target to Windows? This could take a while.", "Confirm", "Cancel"))
                                    {
                                        EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Standalone;
                                        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                                    }

                                    break;
                                }
                            case "Android":
                                {
                                    if (EditorUtility.DisplayDialog("Build Target Switcher", "Are you sure you want to switch your build target to Android? This could take a while.", "Confirm", "Cancel"))
                                    {
                                        EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
                                        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);
                                    }

                                    break;
                                }
                            case "iOS":
                                {
                                    if (ApiUserPlatforms.CurrentUserPlatforms?.SupportsiOS != true) return;
                                    if (EditorUtility.DisplayDialog("Build Target Switcher", "Are you sure you want to switch your build target to iOS? This could take a while.", "Confirm", "Cancel"))
                                    {
                                        EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.iOS;
                                        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.iOS, BuildTarget.iOS);
                                    }

                                    break;
                                }
                            default:
                                {
                                    // Unsupported platform
                                    break;
                                }
                        }
                    });
                    popup.AddToClassList("flex-1");
                    switcherBlock.Add(popup);
                }
            }
        }

        private static List<string> GetBuildTargetOptions()
        {
            var options = new List<string>
        {
            "Windows",
            "Android"
        };
            if (ApiUserPlatforms.CurrentUserPlatforms?.SupportsiOS == true)
            {
                options.Add("iOS");
            }

            return options;
        }

        private static string GetCurrentBuildTarget()
        {
            string currentTarget = "Unsupported Target Platform";
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    currentTarget = "Windows";
                    break;
                case BuildTarget.Android:
                    currentTarget = "Android";
                    break;
                case BuildTarget.iOS:
                    currentTarget = "iOS";
                    break;
                default:
                    currentTarget = "Unsupported Target Platform";
                    break;
            }

            return currentTarget;
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

                loginButton.clicked += () => VRCSdkControlPanel.GetWindow<VRCSdkControlPanel>().Show();
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

        private static async Task BuildAndTestAsync()
        {
            var selectedPrefabPath = EAUploaderCore.selectedPrefabPath;

            if (selectedPrefabPath != null)
            {
                await AvatarUploader.BuildAndTestAsync();
            }
        }

        private static async Task UploadAsync()
        {
            var selectedPrefabPath = EAUploaderCore.selectedPrefabPath;

            var contentName = root.Q<TextFieldPro>("content-name").GetValue();
            var contentDescription = root.Q<TextFieldPro>("content-description").GetValue();
            var releaseStatus = root.Q<DropdownField>("release-status").value;

            if (root.Q<Toggle>("confirm_term").value == false)
            {
                return;
            }

            if (string.IsNullOrEmpty(contentName) || string.IsNullOrEmpty(contentDescription))
            {
                return;
            }

            if (selectedPrefabPath != null)
            {
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

                await AvatarUploader.UploadAvatarAsync(avatar, thumbnailPath);

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
                default:
                    // Unsupported type
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

                button.clicked += () => Application.OpenURL(validateResult.Link);
                Add(button);
            }
        }
    }
}
