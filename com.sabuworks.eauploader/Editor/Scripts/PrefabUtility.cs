using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class CustomPrefabUtility
{
    private static Editor gameObjectEditor;
    private static GameObject currentPreviewObject;

    public static GameObject selectedPrefabInstance { get; private set; }
    public static Dictionary<string, Texture2D> prefabsWithPreview = new Dictionary<string, Texture2D>();
    public static Dictionary<string, Texture2D> vrchatAvatarsWithPreview = new Dictionary<string, Texture2D>();

    public static Dictionary<string, Texture2D> GetPrefabList()
    {
        prefabsWithPreview.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            Texture2D preview = AssetPreview.GetAssetPreview(prefab);
            prefabsWithPreview[assetPath] = preview;
        }
        return prefabsWithPreview;
    }

    public static void UpdatePrefabList()
    {
        GetPrefabList();
        GetVrchatAvatarList();
    }

    private class AssetChangeDetector : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            UpdatePrefabList();
        }
    }

    public static Dictionary<string, Texture2D> GetVrchatAvatarList()
    {
        vrchatAvatarsWithPreview.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            // Check if the prefab has a VRC Avatar Descriptor component
            if (prefab.GetComponent("VRC_AvatarDescriptor") != null)
            {
                Texture2D preview = AssetPreview.GetAssetPreview(prefab);
                vrchatAvatarsWithPreview[assetPath] = preview;
            }
        }
        return vrchatAvatarsWithPreview;
    }

    public static void UpdateVrchatAvatarList()
    {
        GetVrchatAvatarList();
    }

    public static void SelectPrefabAndSetupScene(string prefabPath)
    {
        // Load the selected prefab
        selectedPrefabInstance = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        // Ensure selectedPrefabInstance is not null before processing
        if (selectedPrefabInstance == null)
        {
            Debug.LogError("Failed to load prefab from path: " + prefabPath);
            return;
        }

        // Find all GameObjects in the Scene
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();

        // List to keep track of roots to be removed
        List<GameObject> rootsToRemove = new List<GameObject>();

        // Identify all prefab roots in the scene
        foreach (var go in allGameObjects)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(go))
            {
                GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                if (root != null && !rootsToRemove.Contains(root))
                {
                    rootsToRemove.Add(root);
                }
            }
        }

        // Unpack and delete all identified prefab roots
        foreach (var root in rootsToRemove)
        {
            PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            GameObject.DestroyImmediate(root);
        }

        // Instantiate the selected prefab at position (0, 0, 0)
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefabInstance);
        instance.transform.position = Vector3.zero;
    }

    public static void DrawPrefabPreview(Rect position)
    {
        if (selectedPrefabInstance == null)
        {
            GUI.Label(position, "No prefab selected", EditorStyles.boldLabel);
            return;
        }

        if (currentPreviewObject != selectedPrefabInstance)
        {
            currentPreviewObject = selectedPrefabInstance;
            gameObjectEditor = Editor.CreateEditor(selectedPrefabInstance);
        }

        if (gameObjectEditor != null)
        {
            GUIStyle bgColor = new GUIStyle();
            bgColor.normal.background = EditorGUIUtility.whiteTexture;

            float aspectRatio = 1.0f;  // Replace this with the aspect ratio of your 3D object if needed
            float previewSize = Mathf.Min(position.width, position.height * aspectRatio);
            Rect r = new Rect(position.x, position.y, previewSize, previewSize / aspectRatio); 
            gameObjectEditor.OnInteractivePreviewGUI(r, bgColor);
        }
    }

    public static float GetAvatarHeight(GameObject prefabInstance)
    {
        Collider collider = prefabInstance.GetComponent<Collider>();
        if (collider != null)
        {
            return collider.bounds.size.y;
        }
        
        // デフォルトの値
        return 999f;  //
    }

}
