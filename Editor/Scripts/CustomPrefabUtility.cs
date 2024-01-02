using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using VRC.SDKBase;

public static class CustomPrefabUtility
{
    private const string PrefabsInfoPath = "Packages/com.sabuworks.eauploader/Editor/PrefabManager.json";
    private const string EAUploaderScenePath = "Assets/EAUploader.unity";
    private const string PreviewSavePath = "Packages/com.sabuworks.eauploader/Editor/Resources/PrefabPreviews";
    private static Editor gameObjectEditor;
    private static GameObject currentPreviewObject;
    public static GameObject selectedPrefabInstance { get; private set; }

    public static Dictionary<string, Texture2D> prefabsWithPreview = new Dictionary<string, Texture2D>();
    public static Dictionary<string, Texture2D> vrchatAvatarsWithPreview = new Dictionary<string, Texture2D>();

    // [InitializeOnLoadMethod]
    public static void OnCustomPrefabUtility()
    {
        UpdatePrefabInfo();
        GenerateAndSaveAllPrefabPreviews();
    }

    public static void UpdatePrefabInfo()
    {
        var allPrefabs = GetAllPrefabs();
        SavePrefabsInfo(allPrefabs, PrefabsInfoPath);
    }

    public static Dictionary<string, Texture2D> GetPrefabList()
    {
        prefabsWithPreview.Clear();
        var allPrefabs = LoadPrefabsInfo(PrefabsInfoPath)
                            .OrderByDescending(p => p.Status == "editing")
                            .Where(p => p.Status != "hidden")
                            .ToList();

        foreach (var prefab in allPrefabs)
        {
            string previewImagePath = Path.Combine(PreviewSavePath, Path.GetFileNameWithoutExtension(prefab.Path) + ".png");
            if (File.Exists(previewImagePath))
            {
                Texture2D preview = LoadTextureFromFile(previewImagePath);
                prefabsWithPreview[prefab.Path] = preview;
            }
        }
        return prefabsWithPreview;
    }

    public static Dictionary<string, Texture2D> GetVrchatAvatarList()
    {
        vrchatAvatarsWithPreview.Clear();
        var vrchatAvatars = LoadPrefabsInfo(PrefabsInfoPath)
                            .Where(info => info.Type == "VRChat" && info.Status != "hidden")
                            .OrderByDescending(info => info.Status == "editing")
                            .ToList();
        foreach (var avatar in vrchatAvatars)
        {
            string previewImagePath = Path.Combine(PreviewSavePath, Path.GetFileNameWithoutExtension(avatar.Path) + ".png");
            if (File.Exists(previewImagePath))
            {
                Texture2D preview = LoadTextureFromFile(previewImagePath);
                vrchatAvatarsWithPreview[avatar.Path] = preview;
            }
        }
        return vrchatAvatarsWithPreview;
    }

    private static List<PrefabInfo> GetAllPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        return guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => new PrefabInfo
                    {
                        Path = path,
                        Name = Path.GetFileNameWithoutExtension(path),
                        LastModified = File.GetLastWriteTime(path),
                        Type = GetPrefabType(path),
                        Status = GetPrefabStatus(path)
                    })
                    .OrderBy(p => p.LastModified)
                    .ToList();
    }

    private static string GetPrefabType(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null)
        {
            if (prefab.GetComponent("VRC_AvatarDescriptor") != null)
                return "VRChat";
            if (prefab.GetComponent("VRMMeta") != null)
                return "VRM";
        }
        return "Other";
    }

    private static bool IsVrchatAvatar(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        return prefab != null && prefab.GetComponent("VRC_AvatarDescriptor") != null;
    }

    public static void SavePrefabsInfo(List<PrefabInfo> prefabs, string filePath)
    {
        string directory = Path.GetDirectoryName(filePath);

        // ディレクトリが存在しない場合は作成
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // JSONデータを生成
        PrefabInfoList prefabList = new PrefabInfoList { Prefabs = prefabs };
        string json = JsonUtility.ToJson(prefabList, true);

        // ファイルに書き込む
        File.WriteAllText(filePath, json);
    }

    public static List<PrefabInfo> LoadPrefabsInfo(string filePath)
    {
        if (!File.Exists(filePath)) return new List<PrefabInfo>();

        string json = File.ReadAllText(filePath);
        PrefabInfoList prefabList = JsonUtility.FromJson<PrefabInfoList>(json);
        return prefabList.Prefabs;
    }

    // JSONファイルからPrefabの情報を取得するメソッド群
    public static List<string> GetPrefabPaths()
    {
        return LoadPrefabsInfo(PrefabsInfoPath).Select(p => p.Path).ToList();
    }

    public static List<string> GetPrefabNames()
    {
        return LoadPrefabsInfo(PrefabsInfoPath).Select(p => p.Name).ToList();
    }

    public static DateTime? GetPrefabLastModified(string path)
    {
        return LoadPrefabsInfo(PrefabsInfoPath).FirstOrDefault(p => p.Path == path)?.LastModified;
    }

    public static List<string> GetVrchatAvatarPaths()
    {
        return LoadPrefabsInfo(PrefabsInfoPath)
            .Where(info => info.Type == "VRChat")
            .Select(info => info.Path)
            .ToList();
    }

    public static string GetVrchatAvatarName(string path)
    {
        return LoadPrefabsInfo(PrefabsInfoPath)
            .FirstOrDefault(info => info.Path == path && info.Type == "VRChat")?.Name;
    }

    public static DateTime? GetVrchatAvatarLastModified(string path)
    {
        return LoadPrefabsInfo(PrefabsInfoPath)
            .FirstOrDefault(info => info.Path == path && info.Type == "VRChat")?.LastModified;
    }

    public static void SelectPrefabAndSetupScene(string prefabPath)
    {
        // EAUploader シーンをロード
        if (EditorSceneManager.GetActiveScene().path != EAUploaderScenePath)
        {
            EditorSceneManager.OpenScene(EAUploaderScenePath, OpenSceneMode.Single);
        }

        Scene currentScene = SceneManager.GetSceneByPath(EAUploaderScenePath);
        GameObject existingInstance = null;

        // シーン内のすべてのPrefabインスタンスを検索し、非表示に
        foreach (GameObject obj in currentScene.GetRootGameObjects())
        {
            if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj) == prefabPath)
            {
                existingInstance = obj;  // 選択されたPrefabが見つかった場合
            }
            else
            {
                obj.SetActive(false);  // 他のPrefabは非表示にする
            }
        }

        if (existingInstance != null)
        {
            // 既に存在するPrefabインスタンスをアクティブに
            selectedPrefabInstance = existingInstance;
            selectedPrefabInstance.SetActive(true);
        }
        else
        {
            // 新しいPrefabインスタンスをロードしてインスタンス化
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("Failed to load prefab from path: " + prefabPath);
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = Vector3.zero;
            SceneManager.MoveGameObjectToScene(instance, currentScene);
            selectedPrefabInstance = instance;
        }
    }

    public static void RemovePrefabFromScene(string prefabPath)
    {
        // EAUploader シーンをロード
        if (EditorSceneManager.GetActiveScene().path != EAUploaderScenePath)
        {
            EditorSceneManager.OpenScene(EAUploaderScenePath, OpenSceneMode.Single);
        }

        // シーンを検索
        Scene currentScene = SceneManager.GetSceneByPath(EAUploaderScenePath);
        List<GameObject> instancesToRemove = new List<GameObject>();
        foreach (GameObject obj in currentScene.GetRootGameObjects())
        {
            if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj) == prefabPath)
            {
                instancesToRemove.Add(obj);
            }
        }

        // インスタンスをシーンから削除
        foreach (var instance in instancesToRemove)
        {
            UnityEngine.Object.DestroyImmediate(instance);
        }

        // 必要に応じてシーンの変更を保存
        EditorSceneManager.MarkSceneDirty(currentScene);
        EditorSceneManager.SaveScene(currentScene);
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

            float aspectRatio = 1.0f;
            float previewSize = Mathf.Min(position.width, position.height * aspectRatio);
            Rect r = new Rect(position.x, position.y, previewSize, previewSize / aspectRatio); 
            gameObjectEditor.OnInteractivePreviewGUI(r, bgColor);
        }
    }

    public static bool IsVRMPrefab(string prefabPath)
    {
        var allPrefabsInfo = LoadPrefabsInfo(PrefabsInfoPath);
        var prefabInfo = allPrefabsInfo.FirstOrDefault(info => info.Path == prefabPath);

        return prefabInfo != null && prefabInfo.Type == "VRM";
    }

    public static void SetPrefabStatus(string prefabPath, string status)
    {
        var allPrefabs = LoadPrefabsInfo(PrefabsInfoPath);
        var prefab = allPrefabs.FirstOrDefault(p => p.Path == prefabPath);
        if (prefab != null)
        {
            prefab.Status = status;
            SavePrefabsInfo(allPrefabs, PrefabsInfoPath);
            // Debug.Log($"Save PrefabInfo as {allPrefabs}-{PrefabsInfoPath}");
        }
    }

    public static Dictionary<string, Texture2D> GetHiddenPrefabList()
    {
        var hiddenPrefabsWithPreview = new Dictionary<string, Texture2D>();
        var hiddenPrefabs = LoadPrefabsInfo(PrefabsInfoPath)
                            .Where(p => p.Status == "hidden")
                            .ToList();

        foreach (var prefab in hiddenPrefabs)
        {
            string previewImagePath = Path.Combine(PreviewSavePath, Path.GetFileNameWithoutExtension(prefab.Path) + ".png");
            if (File.Exists(previewImagePath))
            {
                Texture2D preview = LoadTextureFromFile(previewImagePath);
                hiddenPrefabsWithPreview[prefab.Path] = preview;
            }
        }
        return hiddenPrefabsWithPreview;
    }

    public static string GetPrefabStatus(string path)
    {
        // JSONファイルからプレハブのステータスを読み込む
        var allPrefabsInfo = LoadPrefabsInfo(PrefabsInfoPath);
        var prefabInfo = allPrefabsInfo.FirstOrDefault(info => info.Path == path);

        return prefabInfo != null && !string.IsNullOrEmpty(prefabInfo.Status) ? prefabInfo.Status : "show";
    }

    private static void GenerateAndSaveAllPrefabPreviews()
    {
        var allPrefabs = GetAllPrefabs();
        List<string> failedPrefabs = new List<string>(); // 失敗したプレファブを保持するリスト

        foreach (var prefabInfo in allPrefabs)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabInfo.Path);
            if (prefab != null)
            {
                Texture2D preview = GeneratePreview(prefab);
                SavePrefabPreview(prefabInfo.Path, preview);
            }
            
        }

        // 処理終了後に失敗リストを確認
        if (failedPrefabs.Count > 0)
        {
            string failedPaths = string.Join("/n", failedPrefabs);
            EditorUtility.DisplayDialog("Prefab Preview Generation Failed", $"Failed to generate previews for the following prefabs:/n{failedPaths}", "OK");
        }
    }

    private static void SavePrefabPreview(string prefabPath, Texture2D preview)
    {
        string fileName = Path.GetFileNameWithoutExtension(prefabPath);
        string savePath = Path.Combine(PreviewSavePath, $"{fileName}.png");

        byte[] pngData = preview.EncodeToPNG();
        File.WriteAllBytes(savePath, pngData);
    }

    public static Texture2D GeneratePreview(GameObject prefab)
    {
        Scene tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        GameObject cameraObject = null;
        GameObject lightObject = null;
        GameObject instance = null;

        try
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = Vector3.zero;
            SceneManager.MoveGameObjectToScene(instance, tempScene);

            // EAUploader シーンをロード
            if (EditorSceneManager.GetActiveScene().path != EAUploaderScenePath)
            {
                EditorSceneManager.OpenScene(EAUploaderScenePath, OpenSceneMode.Single);
            }

            // シーン内の他のオブジェクトを非表示にする
            Scene currentScene = SceneManager.GetSceneByPath(EAUploaderScenePath);
            foreach (GameObject obj in currentScene.GetRootGameObjects())
            {
                obj.SetActive(false);
            }

            // プレファブをインスタンス化してシーンに設置
            instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = Vector3.zero;
            SceneManager.MoveGameObjectToScene(instance, currentScene);

            // バウンディングボックスを計算
            Bounds bounds = CalculateBounds(instance);

            // プレビュー用のカメラを設定
            cameraObject = new GameObject("EAUploader Preview Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.orthographic = true;

            // カメラのアスペクト比を設定
            camera.aspect = bounds.size.x / bounds.size.y;
            camera.orthographicSize = bounds.size.y / 2;

            camera.transform.position = bounds.center + camera.transform.forward * bounds.extents.magnitude * 2;
            camera.transform.LookAt(bounds.center);

            // プレビュー用のライトを設定
            lightObject = new GameObject("EAUploader Preview Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.color = Color.white;

            // ライトの位置と向きをカメラに合わせる
            light.transform.position = camera.transform.position;
            light.transform.rotation = camera.transform.rotation;

            // レンダリング解像度を設定
            int imageHeight = 540;
            int imageWidth = (int)(imageHeight * camera.aspect);

            // プレビュー画像をレンダリング
            RenderTexture renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            camera.Render();

            // 画像をTexture2Dに変換
            Texture2D preview = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);
            preview.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
            preview.Apply();

            // クリーンアップ
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(instance);
            UnityEngine.Object.DestroyImmediate(lightObject);

            return preview;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error generating preview for prefab: {prefab.name}. Error: {e.Message}");
            return null;
        }
        finally
        {
            // クリーンアップ
            if (cameraObject != null) UnityEngine.Object.DestroyImmediate(cameraObject);
            if (lightObject != null) UnityEngine.Object.DestroyImmediate(lightObject);
            if (instance != null) UnityEngine.Object.DestroyImmediate(instance);
            EditorSceneManager.CloseScene(tempScene, true);
        }
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

    private static Texture2D LoadTextureFromFile(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    public static float GetAvatarHeight(GameObject avatar)
    {
        var avatarDescriptor = avatar.GetComponent<VRC_AvatarDescriptor>();
        if (avatarDescriptor != null)
        {
            // ViewPosition.y がアバターの目線の高さ
            return avatarDescriptor.ViewPosition.y;
        }

        // デフォルト
        return 0f;
    }

    public static void Processor(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        // Prefabが追加・削除・移動された場合、Prefab情報を更新
        UpdatePrefabInfo();

        foreach (string path in importedAssets.Concat(deletedAssets).Concat(movedAssets))
        {
            if (Path.GetExtension(path) == ".prefab")
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    Texture2D preview = GeneratePreview(prefab);
                    SavePrefabPreview(path, preview);
                }
            }
        }
    }

    [System.Serializable]
    public class PrefabInfo
    {
        public string Path;
        public string Name;
        public DateTime LastModified;
        public string Type;
        public string Status;
    }

    [System.Serializable]
    public class PrefabInfoList
    {
        public List<PrefabInfo> Prefabs;
    }

    public static void EnsureEAUploaderSceneExists()
    {
        if (!File.Exists(EAUploaderScenePath))
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, EAUploaderScenePath);
        }
    }
}
