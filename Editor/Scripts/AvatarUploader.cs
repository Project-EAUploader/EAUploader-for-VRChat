using EAUploader.CustomPrefabUtility;
using EAUploader.UI.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDK3.Validation;
using VRC.SDK3A.Editor;
using VRC.SDKBase.Editor.Api;

namespace EAUploader
{
    public class AvatarUploader
    {
        public static event EventHandler OnComplete;

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

        public static async Task BuildAndTestAsync(string prefabPath)
        {
            VRCSdkControlPanel.GetWindow<VRCSdkControlPanel>().Show();

            GameObject instantiatedPrefab = GameObject.Instantiate(PrefabManager.GetPrefab(prefabPath));

            if (instantiatedPrefab == null)
            {
                Debug.LogError($"Failed to load prefab: {EAUploaderCore.selectedPrefabPath}");
                return;
            }

            List<Component> componentsToRemove = AvatarValidation.FindIllegalComponents(instantiatedPrefab).ToList();

            if (!(componentsToRemove is List<Component> list)) return;
            for (int v = list.Count - 1; v > -1; v--)
            {
                UnityEngine.Object.DestroyImmediate(list[v]);
            }


            if (!VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder)) return;

            try
            {
                await builder.BuildAndTest(instantiatedPrefab);

                DialogPro.Show(DialogPro.DialogType.Success, "Build Succeed", T7e.Get("Avatar built and tested successfully"));
            }
            catch (Exception e)
            {
                DialogPro.Show(DialogPro.DialogType.Error, "Build Failed", T7e.Get("Failed to build avatar"));
                Debug.Log(e.Message);
            }

            GameObject.DestroyImmediate(instantiatedPrefab);
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

        public static async Task UploadAvatarAsync(string prefabPath, string contentName, string contentDescription, string releaseStatus, List<string> tags, string thumbnailPath)
        {
            GameObject originalPrefab = PrefabManager.GetPrefab(prefabPath);
            GameObject prefab = GameObject.Instantiate(originalPrefab);

            if (prefab == null)
            {
                Debug.LogError($"Failed to load prefab: {prefabPath}");
                return;
            }

            List<Component> componentsToRemove = AvatarValidation.FindIllegalComponents(prefab).ToList();

            if (!(componentsToRemove is List<Component> list)) return;
            for (int v = list.Count - 1; v > -1; v--)
            {
                UnityEngine.Object.DestroyImmediate(list[v]);
            }


            VRCAvatar avatar;
            bool isNewAvatar = false;
            var avatarData = await GetVRCAvatar(prefabPath);

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
                    var bundlePath = await builder.Build(prefab);

                    if (string.IsNullOrWhiteSpace(bundlePath) || !File.Exists(bundlePath))
                    {
                        DialogPro.Show(DialogPro.DialogType.Error, "Upload Failed", T7e.Get("Failed to build avatar"));
                        OnComplete?.Invoke(null, EventArgs.Empty);
                        return;
                    }

                    if (VRC.SDKBase.Editor.Validation.ValidationEditorHelpers.CheckIfAssetBundleFileTooLarge(ContentType.Avatar, bundlePath, out int fileSize,
                            VRC.Tools.Platform != "standalonewindows"))
                    {
                        var limit = ValidationHelpers.GetAssetBundleSizeLimit(ContentType.Avatar,
                            VRC.Tools.Platform != "standalonewindows");

                        DialogPro.Show(DialogPro.DialogType.Error, "Upload Failed",
                            T7e.Get("Avatar bundle size is too large. The maximum size is {0} MB, but the current size is {1} MB",
                                     limit, fileSize));
                        OnComplete?.Invoke(null, EventArgs.Empty);
                    }

                    if (!originalPrefab.TryGetComponent<PipelineManager>(out var pM))
                    {
                        DialogPro.Show(DialogPro.DialogType.Error, "Upload Failed", T7e.Get("Prefab does not contain a PipelineManager component"));
                        OnComplete?.Invoke(null, EventArgs.Empty);
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
                    var bundlePath = await builder.Build(prefab);
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

                    DialogPro.Show(DialogPro.DialogType.Success, "Upload Succeed", T7e.Get("Avatar updated successfully"));
                    OnComplete?.Invoke(null, EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            GameObject.DestroyImmediate(prefab);
        }

        public static async Task UpdateVRCAvatar(string prefabPath, string contentName, string contentDescription, string releaseStatus, List<string> tags, string thumbnailPath)
        {
            var avatarData = await GetVRCAvatar(prefabPath);

            if (avatarData == null)
            {
                Debug.LogError("Failed to get avatar data");
                return;
            }

            VRCAvatar avatar = (VRCAvatar)avatarData;

            if (avatar.Name != contentName || avatar.Description != contentDescription || avatar.ReleaseStatus != releaseStatus || avatar.Tags != tags)
            {
                avatar.Name = contentName;
                avatar.Description = contentDescription;
                avatar.ReleaseStatus = releaseStatus;
                avatar.Tags = tags;
                await VRCApi.UpdateAvatarInfo(avatar.ID, avatar);
            }

            if (!string.IsNullOrEmpty(thumbnailPath))
            {
                await VRCApi.UpdateAvatarImage(avatar.ID, avatar, thumbnailPath);
            }

            DialogPro.Show(DialogPro.DialogType.Success, "Update Succeed", T7e.Get("Avatar updated successfully"));
            OnComplete?.Invoke(null, EventArgs.Empty);
        }
    }
}