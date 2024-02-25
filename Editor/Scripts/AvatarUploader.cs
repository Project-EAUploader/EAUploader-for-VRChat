using EAUploader.CustomPrefabUtility;
using System;
using UnityEditor;
using UnityEngine;
using VRC.SDK3A.Editor;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.Api;

namespace EAUploader
{
    public class AvatarUploader
    {
        public static bool IsUploading { get; private set; }
        public static string Status { get; private set; }
        public static float Percentage { get; private set; }

        [InitializeOnLoadMethod]
        public static void RegisterSDKCallback()
        {
            VRCSdkControlPanel.OnSdkPanelEnable += AddBuildHook;
        }

        private static void AddBuildHook(object sender, EventArgs e)
        {
            if (VRCSdkControlPanel.TryGetBuilder<IVRCSdkBuilderApi>(out var builder))
            {
                builder.OnSdkUploadProgress += ShowProgress;
                builder.OnSdkUploadSuccess += NoticeUploadSuccess;
                builder.OnSdkUploadError += NoticeUploadError;
            }
        }

        private static void ShowProgress(object sender, (string status, float percentage) e)
        {
            Status = e.status;
            Percentage = e.percentage;
            Debug.Log("Uploading Avatar.." + e.status + ":" + e.percentage * 100 + "%");
        }

        private static void NoticeUploadSuccess(object sender, string message)
        {
            EditorUtility.DisplayDialog("Upload Succeed", message, "OK");
            Status = null;
            Percentage = 0;
        }
        private static void NoticeUploadError(object sender, string message)
        {
            EditorUtility.DisplayDialog("Upload Failed", message, "OK");
            Status = null;
            Percentage = 0;
        }


        public static async void BuildAvatar()
        {
            // Open the SDK Control Panel
            VRCSdkControlPanel.GetWindow<VRCSdkControlPanel>().Show();

            var selectedPrefab = PrefabManager.GetPrefab(EAUploaderCore.selectedPrefabPath);


            if (!VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder)) return;

            try
            {
                await builder.Build(selectedPrefab);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public static async void BuildAndTest()
        {
            // Open the SDK Control Panel
            VRCSdkControlPanel.GetWindow<VRCSdkControlPanel>().Show();

            var selectedPrefab = PrefabManager.GetPrefab(EAUploaderCore.selectedPrefabPath);


            if (!VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder)) return;

            try
            {
                await builder.BuildAndTest(selectedPrefab);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public static async void UploadAvatar(VRCAvatar avatar, string previewImagePath)
        {
            var selectedPrefab = PrefabManager.GetPrefab(EAUploaderCore.selectedPrefabPath);

            if (!VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder)) return;

            try
            {
                await builder.BuildAndUpload(selectedPrefab, avatar, previewImagePath);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}