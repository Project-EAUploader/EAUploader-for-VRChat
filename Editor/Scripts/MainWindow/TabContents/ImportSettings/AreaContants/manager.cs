using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static labels;
using static styles;

public class Manager
{
    private static Dictionary<string, Texture2D> prefabsWithPreview = new Dictionary<string, Texture2D>();
    private static string lastRemovedPrefab;
    private static Vector2 scrollposition;
    private static string searchQuery = "";
    private static bool showAll = true;
    private static bool searchPerformed = false;

    public static void Draw(Rect position)
    {
        if (prefabsWithPreview.Count == 0 || showAll)
        {
            prefabsWithPreview = CustomPrefabUtility.GetPrefabList();
            showAll = false;
        }

        List<string> duplicateRequests = new List<string>();
        List<string> deleteRequests = new List<string>();
        List<string> convertRequests = new List<string>();
        List<string> renameRequests = new List<string>();

        GUILayout.BeginArea(position);
        GUILayout.Label(Get(103), styles.h1LabelStyle);

        // 検索機能
        EditorGUILayout.BeginHorizontal();
        searchQuery = EditorGUILayout.TextField(searchQuery, styles.TextFieldStyle, GUILayout.Height(40));
        searchPerformed = false;
        if (GUILayout.Button(Getc("search", 144), styles.SearchButtonStyle, GUILayout.Height(40)))
        {
            searchPerformed = true;
            if (!string.IsNullOrEmpty(searchQuery))
            {
                prefabsWithPreview = prefabsWithPreview
                    .Where(kvp => kvp.Key.ToLower().Contains(searchQuery.ToLower()))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            else
            {
                prefabsWithPreview = CustomPrefabUtility.GetPrefabList();
            }
        }
        if(searchPerformed)
        {
            if (GUILayout.Button(Getc("search", 145), styles.SearchButtonStyle, GUILayout.Height(40)))
            {
                searchQuery = "";
                searchPerformed = false;
                prefabsWithPreview = CustomPrefabUtility.GetPrefabList();
            }
        }
        EditorGUILayout.EndHorizontal();
        float windowWidth = EditorGUIUtility.currentViewWidth;
        Texture.DrawHorizontalLine(Color.black, 12, windowWidth);

        scrollposition = GUILayout.BeginScrollView(scrollposition, GUILayout.Width(position.width), GUILayout.Height(position.height));
        GUILayout.BeginVertical(styles.listBorderStyle);

        foreach (var kvp in prefabsWithPreview)
        {
            string prefab = kvp.Key;
            string prefabName = Path.GetFileNameWithoutExtension(prefab);
            Texture2D preview = kvp.Value;

            GUILayout.Space(10); // 項目間のスペース

            EditorGUILayout.BeginHorizontal();
            // Preview画像
            if (preview != null)
            {
                if (GUILayout.Button(preview, prefabButtonStyle, GUILayout.Width(150), GUILayout.Height(150)))
                {
                    AvatarPreviewerWindow.ShowWindow(prefabName);
                }
            }
            if (CustomPrefabUtility.IsVRMPrefab(prefab))
            {
                GUILayout.Label(prefabName, styles.h2LabelStyle, GUILayout.Width(310));

                if (GUILayout.Button(Getc("help", 500), HelpButtonStyle))
                {
                    EAUploaderMessageWindow.ShowMsg(102);
                }
                if (GUILayout.Button(Get(146), miniButtonStyle, GUILayout.Width(150)))
                {
                    convertRequests.Add(prefab);
                }
            }else{
                GUILayout.Label(prefabName, styles.h2LabelStyle, GUILayout.Width(555));

            }
            EditorGUILayout.BeginVertical();
            /* アイコンのみに変更 採用検討中
            if (GUILayout.Button(Getc("create", 147), miniButtonStyle, GUILayout.Width(80)))
            {
                renameRequests.Add(prefab);
            }
            if (GUILayout.Button(Getc("copy", 148), miniButtonStyle, GUILayout.Width(80)))
            {
                duplicateRequests.Add(prefab);
            }
            if (GUILayout.Button(Getc("delete", 133), miniButtonRedStyle, GUILayout.Width(80)))
            {
                if (EditorUtility.DisplayDialog(Get(134), prefab + Get(135), Get(136), Get(137)))
                {
                    deleteRequests.Add(prefab);
                }
            }
            ※ 0 はアイコンのみ
            */
            if (GUILayout.Button(Getc("create", 0), miniButtonStyle, GUILayout.Width(60)))
            {
                renameRequests.Add(prefab);
            }
            if (GUILayout.Button(Getc("copy", 0), miniButtonStyle, GUILayout.Width(60)))
            {
                duplicateRequests.Add(prefab);
            }
            if (GUILayout.Button(Getc("delete", 0), miniButtonRedStyle, GUILayout.Width(60)))
            {
                deleteRequests.Add(prefab);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            Texture.DrawHorizontalDottedLine(Color.black, 12, windowWidth);
        }
        GUILayout.Space(200);
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        ProcessPrefabRequests(renameRequests, duplicateRequests, deleteRequests, convertRequests);
    }

    private static void ProcessPrefabRequests(List<string> renameRequests, List<string> duplicateRequests, List<string> deleteRequests, List<string> convertRequests)
    {
        foreach (var prefabPath in renameRequests)
        {
            RenamePrefab(prefabPath);
        }
        foreach (var prefabPath in duplicateRequests)
        {
            DuplicatePrefab(prefabPath);
        }

        foreach (var prefabPath in deleteRequests)
        {
            DeletePrefab(prefabPath);
        }

        foreach (var prefabPath in convertRequests)
        {
            ConvertToVRChat(prefabPath);
        }
    }

    private static void RenamePrefab(string prefabpath)
    {
        RenameFileWindow.ShowWindow(prefabpath);
    }

    private static void ConvertToVRChat(string prefabPath)
    {
        GameObject selectedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (selectedPrefab != null && CustomPrefabUtility.IsVRMPrefab(prefabPath))
        {
            Selection.activeObject = selectedPrefab;
            EditorApplication.ExecuteMenuItem("VRM0/Duplicate and Convert for VRChat");
        }
    }

    private static void DuplicatePrefab(string originalPrefabPath)
    {
        string assetName = Path.GetFileNameWithoutExtension(originalPrefabPath);
        string directoryPath = Path.GetDirectoryName(originalPrefabPath);
        string newAssetName = assetName + "_Copy";
        string newPrefabPath = AssetDatabase.GenerateUniqueAssetPath(directoryPath + "/" + newAssetName + ".prefab");

        Object originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(originalPrefabPath);
        if (originalPrefab != null)
        {
            Object prefabCopy = Object.Instantiate(originalPrefab);
            PrefabUtility.SaveAsPrefabAsset((GameObject)prefabCopy, newPrefabPath);
            Object.DestroyImmediate(prefabCopy);

            RenameFileWindow.ShowWindow(newPrefabPath);

            prefabsWithPreview = CustomPrefabUtility.GetPrefabList();
        }
    }

    private static void DeletePrefab(string prefabPath)
    {
        ConfirmationPopup.ShowWindow(prefabPath);
    }

    public static void RefreshPrefabList()
    {
        prefabsWithPreview = CustomPrefabUtility.GetPrefabList();
    }

}


public class ConfirmationPopup : EditorWindow
{
    private string prefabPath;
    private static readonly Vector2 windowSize = new Vector2(300, 120);

    public static void ShowWindow(string path)
    {
        var window = GetWindow<ConfirmationPopup>("Confirm Deletion");
        window.prefabPath = path;
        window.minSize = windowSize;
        window.maxSize = windowSize;
        var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
        window.ShowAsDropDown(new Rect(mousePos, Vector2.zero), windowSize);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField(Get(134), EditorStyles.boldLabel);
        EditorGUILayout.LabelField(prefabPath + Get(135), EditorStyles.wordWrappedLabel); // Message
        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(Get(136)))
        {
            CustomPrefabUtility.RemovePrefabFromScene(prefabPath);
            AssetDatabase.DeleteAsset(prefabPath);
            Manager.RefreshPrefabList();
            this.Close();
        }

        if (GUILayout.Button(Get(137)))
        {
            this.Close();
        }
        EditorGUILayout.EndHorizontal();
    }
}

