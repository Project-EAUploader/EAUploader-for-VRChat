using EAUploader.CustomPrefabUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDK3A.Editor;
using VRC.SDKBase.Editor.Api;

namespace EAUploader
{
    public class AvatarUploader
    {
        public static event EventHandler<DialogEventArgs> DialogRequired;

        public class DialogEventArgs : EventArgs
        {
            public string DialogType { get; }
            public string Message { get; }

            public DialogEventArgs(string dialogType, string message)
            {
                DialogType = dialogType;
                Message = message;
            }
        }

        public static event EventHandler<ProgressEventArgs> ProgressChanged;

        public class ProgressEventArgs : EventArgs
        {
            public string Status { get; }
            public float Percentage { get; }

            public ProgressEventArgs(string status, float percentage)
            {
                Status = status;
                Percentage = percentage;
            }
        }

        public static async Task BuildAndTestAsync()
        {
            VRCSdkControlPanel.GetWindow<VRCSdkControlPanel>().Show();

            var selectedPrefab = PrefabManager.GetPrefab(EAUploaderCore.selectedPrefabPath);


            if (!VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder)) return;

            try
            {
                await builder.BuildAndTest(selectedPrefab);

                OnDialogRequired("Build Succeed", T7e.Get("Avatar built and tested successfully"));
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public static async Task<VRCAvatar?> GetVRCAvatar(string selectedPrefabPath)
        {
            var selectedPrefab = PrefabManager.GetPrefab(selectedPrefabPath);
            if (selectedPrefab == null)
            {
                Debug.LogError($"Failed to load prefab: {selectedPrefabPath}");
                return null;
            }

            var pipelineManager = selectedPrefab.GetComponent<PipelineManager>();
            if (pipelineManager == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(pipelineManager.blueprintId))
            {
                try
                {
                    return await VRCApi.GetAvatar(pipelineManager.blueprintId, true);
                }
                catch (ApiErrorException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static async Task UploadAvatarAsync(string selectedPrefabPath, string contentName, string contentDescription, string releaseStatus, List<string> tags, string thumbnailPath)
        {
            var selectedPrefab = PrefabManager.GetPrefab(EAUploaderCore.selectedPrefabPath);
            if (selectedPrefab == null)
            {
                Debug.LogError($"Failed to load prefab: {selectedPrefabPath}");
                return;
            }

            VRCAvatar avatar;
            bool isNewAvatar = false;
            var avatarData = await GetVRCAvatar(selectedPrefabPath);

            if (avatarData == null)
            {
                Debug.Log("Creating new avatar..");
                isNewAvatar = true;
                avatar = new VRCAvatar
                {
                    Name = contentName,
                    Description = contentDescription,
                    ReleaseStatus = releaseStatus,
                    Tags = tags,
                };
            }
            else
            {
                avatar = (VRCAvatar)avatarData;
            }

            VRCSdkControlPanel.GetWindow<VRCSdkControlPanel>().Show();
            if (!VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder)) return;

            try
            {
                Action<string, float> action = (status, percentage) =>
                {
                    try
                    {
                        ProgressChanged?.Invoke(null, new ProgressEventArgs(status, percentage));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                    Debug.Log("Uploading Avatar.." + status + ":" + percentage * 100 + "%");
                };
                if (isNewAvatar)
                {
                    var bundlePath = await builder.Build(selectedPrefab);

                    if (string.IsNullOrWhiteSpace(bundlePath) || !File.Exists(bundlePath))
                    {
                        OnDialogRequired("Upload Failed", T7e.Get("Failed to build avatar"));
                        return;
                    }

                    if (ValidationHelpers.CheckIfAssetBundleFileTooLarge(ContentType.Avatar, bundlePath, out int fileSize,
                            VRC.Tools.Platform != "standalonewindows"))
                    {
                        var limit = ValidationHelpers.GetAssetBundleSizeLimit(ContentType.Avatar,
                            VRC.Tools.Platform != "standalonewindows");

                        OnDialogRequired("Upload Failed",
                            T7e.Get("Avatar bundle size is too large. The maximum size is {0} MB, but the current size is {1} MB",
                                     limit, fileSize));
                    }

                    if (!selectedPrefab.TryGetComponent<PipelineManager>(out var pM))
                    {
                        OnDialogRequired("Upload Failed", T7e.Get("Prefab does not contain a PipelineManager component"));
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(pM.blueprintId))
                    {
                        Undo.RecordObject(pM, "Assigning a new ID");
                        pM.AssignId();
                    }

                    await VRCApi.CreateNewAvatar(pM.blueprintId, avatar, bundlePath, thumbnailPath, action);
                }
                else
                {
                    var bundlePath = await builder.Build(selectedPrefab);
                    await VRCApi.UpdateAvatarBundle(avatar.ID, avatar, bundlePath, action);

                    if (!string.IsNullOrEmpty(thumbnailPath))
                    {
                        await VRCApi.UpdateAvatarImage(avatar.ID, avatar, thumbnailPath, action);
                    }

                    if (avatar.Name != contentName || avatar.Description != contentDescription || avatar.ReleaseStatus != releaseStatus || avatar.Tags != tags)
                    {
                        avatar.Name = contentName;
                        avatar.Description = contentDescription;
                        avatar.ReleaseStatus = releaseStatus;
                        avatar.Tags = tags;
                        await VRCApi.UpdateAvatarInfo(avatar.ID, avatar);
                    }

                    OnDialogRequired("Upload Succeed", T7e.Get("Avatar updated successfully"));
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private static void OnDialogRequired(string dialogType, string message)
        {
            DialogRequired?.Invoke(null, new DialogEventArgs(dialogType, message));
        }
    }
}