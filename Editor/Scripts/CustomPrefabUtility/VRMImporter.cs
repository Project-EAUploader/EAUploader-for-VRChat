using EAUploader.UI.Components;
using Esperecyan.Unity.VRMConverterForVRChat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UIElements;
using VRC.Core;
using VRM;
using static Esperecyan.Unity.VRMConverterForVRChat.Converter;

namespace EAUploader.CustomPrefabUtility
{
    public class VRMImporter
    {
        public static void ImportVRM()
        {
            var path = EditorUtility.OpenFilePanel("Open .vrm", "", "vrm");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            if (path.StartsWithUnityAssetPath())
            {
                Debug.LogWarningFormat("disallow import from folder under the Assets");
                return;
            }
            string vrmFolder = "Assets/VRM";
            if (!AssetDatabase.IsValidFolder(vrmFolder))
            {
                AssetDatabase.CreateFolder("Assets", "VRM");
            }

            var prefabPath = Path.Combine(vrmFolder, Path.GetFileNameWithoutExtension(path) + ".prefab");
            vrmAssetPostprocessor.ImportVrmAndCreatePrefab(path, UnityPath.FromUnityPath(prefabPath));

            WaitForPrefabGeneration(prefabPath, () => VRMImporterWindow.ShowWindow(prefabPath));
        }

        private static void WaitForPrefabGeneration(string prefabPath, Action callback)
        {
            EditorApplication.update += WaitForPrefabGenerationInternal;

            void WaitForPrefabGenerationInternal()
            {
                if (File.Exists(prefabPath))
                {
                    EditorApplication.update -= WaitForPrefabGenerationInternal;
                    callback();
                }
            }
        }
    }

    internal class VRMImporterWindow : EditorWindow
    {
        private string prefabPath = "";
        SwayingObjectsConverterSetting swayingObjectsConverterSetting;

        [Serializable]
        public struct GameSwitch
        {
            public string name;
        }

        public static void ShowWindow(string prefabPath)
        {
            VRMImporterWindow wnd = GetWindow<VRMImporterWindow>();
            wnd.titleContent = new GUIContent("VRM Importer");
            wnd.minSize = new Vector2(480, 640);
            wnd.prefabPath = prefabPath;
            wnd.ShowUtility();
        }

        public void CreateGUI()
        {
            rootVisualElement.schedule.Execute(() => LanguageUtility.Localization(rootVisualElement)).Every(100);
        }

        private void ImportVRMButtonClicked()
        {
            var prefabBlueprintId = "";
            var blueprintIds = new Dictionary<int, string>();
            var previousPrefab = PrefabManager.GetPrefab(prefabPath);
            if (previousPrefab != null)
            {
                var pipelineManager = previousPrefab.GetComponent<PipelineManager>();
                prefabBlueprintId = pipelineManager ? pipelineManager.blueprintId : "";

                GameObject[] previousRootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
                blueprintIds = previousRootGameObjects
                    .Where(root => PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root) == prefabPath)
                    .Select(root =>
                    {
                        var manager = root.GetComponent<PipelineManager>();
                        var blueprintId = manager ? manager.blueprintId : "";
                        return new
                        {
                            index = Array.IndexOf(previousRootGameObjects, root),
                            blueprintId = blueprintId != prefabBlueprintId ? blueprintId : "",
                        };
                    }).ToDictionary(
                        keySelector: indexAndBlueprintId => indexAndBlueprintId.index,
                        elementSelector: indexAndBlueprintId => indexAndBlueprintId.blueprintId
                    );
            }
            GameObject prefabInstance = Duplicator.Duplicate(previousPrefab, prefabPath, new List<string> { "" }, true);

            var clips = VRMUtility.GetAllVRMBlendShapeClips(prefabInstance);
            bool forQuest = rootVisualElement.Q<Toggle>("forQuest").value;
            bool takingOverSwayingParameters = rootVisualElement.Q<Toggle>("takingOverSwayingParameters").value;
            float addedShouldersPositionY = rootVisualElement.Q<Slider>("addedShouldersPositionY").value;
            float addedArmaturePositionY = rootVisualElement.Q<Slider>("addedArmaturePositionY").value;
            bool useShapeKeyNormalsAndTangents = rootVisualElement.Q<Toggle>("useShapeKeyNormalsAndTangents").value;
            Convert(
                prefabInstance: prefabInstance,
                clips: clips,
                forQuest: forQuest,
                swayingObjectsConverterSetting: swayingObjectsConverterSetting,
                takingOverSwayingParameters: takingOverSwayingParameters,
                addedShouldersPositionY: addedShouldersPositionY,
                addedArmaturePositionY: addedArmaturePositionY,
                useShapeKeyNormalsAndTangents: useShapeKeyNormalsAndTangents
            );

            if (!string.IsNullOrEmpty(prefabBlueprintId))
            {
                prefabInstance.GetComponent<PipelineManager>().blueprintId = prefabBlueprintId;
            }

            PrefabUtility.ApplyPrefabInstance(prefabInstance, InteractionMode.AutomatedAction);

            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var (avatarIndex, blueprintId) in blueprintIds)
            {
                if (string.IsNullOrEmpty(blueprintId))
                {
                    continue;
                }
                rootGameObjects[avatarIndex].GetComponent<PipelineManager>().blueprintId = blueprintId;
            }

            if (blueprintIds.Count > 0)
            {
                DestroyImmediate(prefabInstance);
            }

            Close();
        }
    }
}