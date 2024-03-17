using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EAUploader.CustomPrefabUtility
{
    public class PrefabPreview
    {
        private const string PREVIEW_SAVE_PATH = "Assets/EAUploader/PrefabPreviews";

        public static Texture2D GeneratePreview(GameObject prefab)
        {
            var previewRenderUtility = new PreviewRenderUtility();
            var rect = new Rect(0, 0, 1080, 1080);

            var gameObject = previewRenderUtility.InstantiatePrefabInScene(prefab);

            Bounds bounds = CalculateBounds(gameObject);

            previewRenderUtility.BeginStaticPreview(rect);
            previewRenderUtility.AddSingleGO(gameObject);

            previewRenderUtility.camera.backgroundColor = new UnityEngine.Color(0.9f, 0.9f, 0.9f, 1);
            previewRenderUtility.camera.clearFlags = CameraClearFlags.SolidColor;
            previewRenderUtility.camera.orthographic = true;
            previewRenderUtility.camera.orthographicSize = bounds.size.y / 2;

            float size = bounds.size.magnitude;
            float distance = size / (4 * Mathf.Tan(previewRenderUtility.camera.fieldOfView * 0.5f * Mathf.Deg2Rad));
            previewRenderUtility.camera.transform.position = bounds.center + previewRenderUtility.camera.transform.forward * distance;
            previewRenderUtility.camera.transform.LookAt(bounds.center);

            previewRenderUtility.Render();

            Texture2D texture = previewRenderUtility.EndStaticPreview();

            previewRenderUtility.Cleanup();

            return texture;
        }

        private static Bounds CalculateBounds(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

        public static void GenerateAndSaveAllPrefabPreviews()
        {
            var allPrefabs = PrefabManager.GetAllPrefabs();

            foreach (var prefabInfo in allPrefabs)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabInfo.Path);
                if (prefab != null)
                {
                    Texture2D preview = GeneratePreview(prefab);
                    SavePrefabPreview(prefabInfo.Path, preview);
                }
            }
        }

        internal static void SavePrefabPreview(string prefabPath, Texture2D preview)
        {
            string fileName = Path.GetFileNameWithoutExtension(prefabPath);
            string savePath = Path.Combine(PREVIEW_SAVE_PATH, $"{fileName}.png");

            string directoryPath = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                byte[] pngData = preview.EncodeToPNG();
                File.WriteAllBytes(savePath, pngData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving preview for prefab: {prefabPath}. \nError: {e.Message}");
                return;
            }
        }

        internal static Texture2D LoadTextureFromFile(string filePath)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            return texture;
        }

        public static void SaveTextureToFile(Texture2D texture, string filePath)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
        }

        public static string GetPreviewImagePath(string prefabPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(prefabPath);
            return Path.Combine(PREVIEW_SAVE_PATH, $"{fileName}.png");
        }


        public static Texture2D GetPrefabPreview(string prefabPath)
        {
            string previewImagePath = PrefabPreview.GetPreviewImagePath(prefabPath);
            if (File.Exists(previewImagePath))
            {
                return PrefabPreview.LoadTextureFromFile(previewImagePath);
            }
            return null;
        }
    }
}