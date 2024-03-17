using EAUploader.CustomPrefabUtility;
using EAUploader.UI.Components;
using System;
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
        private static bool isLoggedin;
        private static bool isFormNull = true;
        private static bool isConfirmTerm = false;

        private static readonly Dictionary<string, string> BUILD_TARGET_ICONS = new Dictionary<string, string>
        {
            { "Windows", "fab fa-windows" },
            { "Android", "fab fa-android" },
            { "iOS", "fab fa-apple" }
        };

        public static void ShowContent(VisualElement rootContainer)
        {
            isCloned = false;
            isLoggedin = APIUser.IsLoggedIn;
            root = rootContainer;

            EAUploaderCore.SelectedPrefabPathChanged += OnSelectedPrefabPathChanged;

            root.schedule.Execute(() =>
            {
                if (APIUser.IsLoggedIn != isLoggedin)
                {
                    isLoggedin = APIUser.IsLoggedIn;
                    UpdateStatus();
                }
            }).Every(500);


            if (EAUploaderCore.selectedPrefabPath == null)
            {
                var label = new Label(T7e.Get("Select a prefab to upload"))
                {
                    style =
                    {
                        flexGrow = 1,
                        unityTextAlign = TextAnchor.MiddleCenter,
                        fontSize = 24,
                        unityFontStyleAndWeight = FontStyle.Bold
                    }
                };
                root.Add(label);
                return;
            }

            var hasDescriptor = Utility.CheckAvatarHasVRCAvatarDescriptor(PrefabManager.GetPrefab(EAUploaderCore.selectedPrefabPath));

            if (!hasDescriptor)
            {
                var label = new Label(T7e.Get("The avatar cannot be uploaded because it does not have a VRCAvatarDescriptor."))
                {
                    style =
                    {
                        flexGrow = 1,
                        unityTextAlign = TextAnchor.MiddleCenter,
                        fontSize = 24,
                        unityFontStyleAndWeight = FontStyle.Bold
                    }
                };
                root.Q("upload_form").Add(label);
                return;
            }

            CloneVisualTree();
            checkCanUpload();

            Validate();
            UpdateStatus();

            OnSelectedPrefabPathChanged(EAUploaderCore.selectedPrefabPath);
        }

        private static void CloneVisualTree()
        {
            if (!isCloned)
            {
                root.Clear();
                var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Upload/Contents/UploadForm");
                visualTree.CloneTree(root);
                isCloned = true;

                BindEvents();
            }
        }

        private static void BindEvents()
        {
            root.Q<ShadowButton>("buildandtest").clicked += async () => await BuildAndTestAsync();
            root.Q<ShadowButton>("upload").clicked += async () => await UploadAsync();
            root.Q<ShadowButton>("add_thumbnail").clicked += AddThumbnail;
            root.Q<ShadowButton>("remove_thumbnail").clicked += RemoveThumbnail;
            root.Q<TextFieldPro>("content-name").RegisterValueChangedCallback(evt =>
            {
                if (string.IsNullOrEmpty(root.Q<TextFieldPro>("content-name").GetValue()))
                {
                    isFormNull = true;
                }
                else
                {
                    isFormNull = false;
                }
                checkCanUpload();
            });
            root.Q<Toggle>("confirm_term").RegisterValueChangedCallback(evt =>
            {
                isConfirmTerm = evt.newValue;
                checkCanUpload();
            });
        }

        private static void checkCanUpload()
        {
            var uploadButton = root.Q<ShadowButton>("upload");
            if (isFormNull || !isConfirmTerm)
            {
                uploadButton.SetEnabled(false);
            }
            else
            {
                uploadButton.SetEnabled(true);
            }
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
                UpdateStatus();

                var switcherBlock = root.Q("platform-switcher");
                if (switcherBlock.Q("platform-switcher-popup") == null)
                {
                    var options = GetBuildTargetOptions();
                    var currentTarget = GetCurrentBuildTarget();
                    var selectedIndex = options.IndexOf(currentTarget);

                    var popup = new PopupField<string>(T7e.Get("Selected Platform"), options, selectedIndex)
                    {
                        name = "platform-switcher-popup"
                    };
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
                                    if (EditorUtility.DisplayDialog(T7e.Get("Build Target Switcher"), T7e.Get("Are you sure you want to switch your build target to Windows? This could take a while."), T7e.Get("Confirm"), T7e.Get("Cancel")))
                                    {
                                        EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Standalone;
                                        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                                    }

                                    break;
                                }
                            case "Android":
                                {
                                    if (EditorUtility.DisplayDialog(T7e.Get("Build Target Switcher"), T7e.Get("Are you sure you want to switch your build target to Android? This could take a while."), T7e.Get("Confirm"), T7e.Get("Cancel")))
                                    {
                                        EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
                                        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);
                                    }

                                    break;
                                }
                            case "iOS":
                                {
                                    if (ApiUserPlatforms.CurrentUserPlatforms?.SupportsiOS != true) return;
                                    if (EditorUtility.DisplayDialog(T7e.Get("Build Target Switcher"), T7e.Get("Are you sure you want to switch your build target to iOS? This could take a while."), T7e.Get("Confirm"), T7e.Get("Cancel")))
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
            var prefab = PrefabManager.GetPrefab(EAUploaderCore.selectedPrefabPath);
            var performanceInfos = PerformanceInfoComputer.ComputePerformanceInfos(prefab, false);

            var performanceInfoList = root.Q<VisualElement>($"performance_info_list");
            performanceInfoList.Clear();

            foreach (var info in performanceInfos)
            {
                var item = new VisualElement()
                {
                    name = "performance_info_item"
                };
                item.AddToClassList("flex-row");
                item.AddToClassList("flex-1");
                item.AddToClassList("align-items-center");

                var icon_texture = PerformanceIcons.GetIconForPerformance(info.rating);
                var icon = new Image()
                {
                    image = icon_texture
                };
                item.Add(icon);

                string categoryName = T7e.Get(info.categoryName);
                string rating = T7e.Get(info.rating.ToString());
                string data = info.data;

                var label = new Label($"{categoryName}: {rating} ({data})");
                item.Add(label);

                performanceInfoList.Add(item);
            }
        }

        private static void UpdateStatus()
        {
            var loginStatus = root.Q<VisualElement>("login_status");
            var permissionStatusContainer = root.Q<VisualElement>("permission_status_container");
            var permissionStatus = root.Q<VisualElement>("permission_status");
            var uploadMain = root.Q<VisualElement>("upload_main");
            var uploadAction = root.Q<VisualElement>("upload_action");

            loginStatus.Clear();
            permissionStatus.Clear();

            if (VRC.Core.APIUser.IsLoggedIn)
            {
                loginStatus.Add(new Label(T7e.Get("Logged in as ") + VRC.Core.APIUser.CurrentUser.displayName));
                permissionStatusContainer.style.display = DisplayStyle.Flex;
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
                permissionStatusContainer.style.display = DisplayStyle.None;
            }

            if (APIUser.CurrentUser != null && APIUser.CurrentUser.canPublishAvatars)
            {
                permissionStatus.Add(new Label(T7e.Get("You have permission to upload avatar")));
                uploadMain.style.display = DisplayStyle.Flex;
                uploadAction.style.display = DisplayStyle.Flex;
            }
            else
            {
                permissionStatus.Add(new Label(T7e.Get("You cannot upload an avatar with your current trust rank. You can immediately increase your rank by spending time on VRChat and adding friends. Or you can join VRChat+ (for a fee) to raise your rank immediately.")));
                uploadMain.style.display = DisplayStyle.None;
                uploadAction.style.display = DisplayStyle.None;
            }
        }

        private static async Task BuildAndTestAsync()
        {
            var selectedPrefabPath = EAUploaderCore.selectedPrefabPath;

            // If platform is not supported, return
            if (GetCurrentBuildTarget() == "Windows")
            {
                await AvatarUploader.BuildAndTestAsync();
            }
            else // If platform is not "Windows"
            {
                EditorUtility.DisplayDialog(T7e.Get("Unsupported Platform"), T7e.Get("Avatar testing is only supported on Windows."), "OK");
                return;
            }

            if (selectedPrefabPath != null)
            {
                await AvatarUploader.BuildAndTestAsync();
            }
        }

        private static async Task UploadAsync()
        {
            var selectedPrefabPath = EAUploaderCore.selectedPrefabPath;
            if (string.IsNullOrEmpty(selectedPrefabPath) || !root.Q<Toggle>("confirm_term").value)
            {
                Debug.LogError("Required fields are missing or terms not confirmed.");
                return;
            }

            var contentName = root.Q<TextFieldPro>("content-name").GetValue();
            var contentDescription = root.Q<TextFieldPro>("content-description").GetValue();
            var releaseStatus = root.Q<DropdownField>("release-status").value.ToLower();
            var tags = root.Q<UI.Components.ContentWarningsField>("content-warnings").Tags;

            string thumbnailPath = thumbnailUrl ?? PrefabPreview.GetPreviewImagePath(selectedPrefabPath);

            VRCAvatar avatar = default;
            bool isNewAvatar = false;

            if (string.IsNullOrEmpty(contentName))
            {
                return;
            }
            
            if (string.IsNullOrEmpty(contentDescription))
            {
                contentDescription = string.Empty;
            }

            var pipelineManager = PrefabManager.GetPrefab(selectedPrefabPath).GetComponent<PipelineManager>();
            if (!string.IsNullOrEmpty(pipelineManager.blueprintId))
            {
                try
                {
                    avatar = await VRCApi.GetAvatar(pipelineManager.blueprintId, true);
                }
                catch (Exception)
                {
                    isNewAvatar = true;
                    avatar = new VRCAvatar { Name = contentName, Description = contentDescription, ReleaseStatus = releaseStatus, Tags = tags};
                }
            }
            else
            {
                isNewAvatar = true;
                avatar = new VRCAvatar { Name = contentName, Description = contentDescription, ReleaseStatus = releaseStatus, Tags = tags };
            }

            var uploadStatus = root.Q<VisualElement>("upload_status");
            uploadStatus.Clear();
            uploadStatus.Add(new Label(T7e.Get("Uploading avatar...")));
            var progress = new ProgressBar()
            {
                lowValue = 0,
                highValue = 1,
            };
            uploadStatus.Add(progress);

            IVisualElementScheduledItem cancelUploadSchedule = null;

            cancelUploadSchedule = uploadStatus.schedule.Execute(() =>
            {
                if (AvatarUploader.IsUploading)
                {
                    progress.value = AvatarUploader.Percentage;
                    progress.title = AvatarUploader.Status;
                }
                else
                {
                    uploadStatus.Clear();
                    if (cancelUploadSchedule != null) // nullチェック
                    {
                        cancelUploadSchedule.Pause(); // ここでスケジュールを一時停止
                    }
                }
            }).Every(1000);

            try
            {
                if (isNewAvatar)
                {
                    await AvatarUploader.UploadAvatarAsync(avatar, thumbnailPath);
                }
                else
                {
                    if (!string.IsNullOrEmpty(thumbnailPath))
                    {
                        await VRCApi.UpdateAvatarImage(avatar.ID, avatar, thumbnailPath);
                    }
                    await VRCApi.UpdateAvatarInfo(avatar.ID, avatar);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred during avatar upload: {ex.Message}");
            }
            finally
            {
                uploadStatus.Clear();
                if (cancelUploadSchedule != null)
                {
                    cancelUploadSchedule.Pause();
                }
            }
        }
    }
}