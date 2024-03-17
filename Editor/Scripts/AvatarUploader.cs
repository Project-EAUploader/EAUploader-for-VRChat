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
        public static bool IsUploading { get; set; } = false;
        public static string Status { get; private set; }
        public static float Percentage { get; private set; }

        public static async Task BuildAndTestAsync()
        {
            VRCSdkControlPanel.GetWindow<VRCSdkControlPanel>().Show();

            var selectedPrefab = PrefabManager.GetPrefab(EAUploaderCore.selectedPrefabPath);


            if (!VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder)) return;

            try
            {
                await builder.BuildAndTest(selectedPrefab);

                EditorUtility.DisplayDialog(T7e.Get("Build Succeed"), T7e.Get("Avatar built and tested successfully"), "OK");
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
                    Status = status;
                    Percentage = percentage;
                    Debug.Log("Uploading Avatar.." + status + ":" + percentage * 100 + "%");
                };
                if (isNewAvatar)
                {
                    var bundlePath = await builder.Build(selectedPrefab);

                    if (string.IsNullOrWhiteSpace(bundlePath) || !File.Exists(bundlePath))
                    {
                        EditorUtility.DisplayDialog(T7e.Get("Upload Failed"), T7e.Get("Failed to build avatar"), "OK");
                        return;
                    }

                    if (ValidationHelpers.CheckIfAssetBundleFileTooLarge(ContentType.Avatar, bundlePath, out int fileSize,
                            VRC.Tools.Platform != "standalonewindows"))
                    {
                        var limit = ValidationHelpers.GetAssetBundleSizeLimit(ContentType.Avatar,
                            VRC.Tools.Platform != "standalonewindows");

                        EditorUtility.DisplayDialog(T7e.Get("Upload Failed"),
                                                       T7e.Get("Avatar bundle size is too large. The maximum size is {0} MB, but the current size is {1} MB",
                                                                                      limit, fileSize), "OK");
                    }

                    if (!selectedPrefab.TryGetComponent<PipelineManager>(out var pM))
                    {
                        EditorUtility.DisplayDialog(T7e.Get("Upload Failed"), T7e.Get("Prefab does not contain a PipelineManager component"), "OK");
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

                    EditorUtility.DisplayDialog(T7e.Get("Upload Succeed"), T7e.Get("Avatar updated successfully"), "OK");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}