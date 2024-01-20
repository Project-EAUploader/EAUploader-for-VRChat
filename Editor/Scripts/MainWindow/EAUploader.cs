using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using VRCWMarketPlace;
using static labels;
using static styles;

[InitializeOnLoad]
public static class EasyUploaderMenu
{
    [MenuItem("EAUploader/MainWindow _%#e")]
    public static void ShowEasyUploaderWindow()
    {
        EAUploader.ShowWindow();
    }
}

internal class EAUploader : EditorWindow
{
    private static int selectedTabIndex = 0;
    private string[] tabNames;
    int currentWindowWidth;
    int currentWindowHeight;
    public static string[] prefabPaths;
    public static string[] prefabNames;
    public static GameObject selectedPrefabInstance;
    public static string selectedPrefabName;
    private string currentLanguage;
    private string previousLanguage;
    public static bool HasVRM;
    public static bool HasVRMConverter;
    public static bool skipOnDisable = false;
    private static int selectedAvatarWorldTabIndex = 0;

    // [InitializeOnLoadMethod] EAInitialization.cs参照
    public static void OnEAUploader()
    {
        EditorApplication.update += OpenWindowOnce;
    }

    // 一度だけウィンドウを開く
    private static void OpenWindowOnce()
    {
        EditorApplication.update -= OpenWindowOnce;
        ShowWindow();
    }

    public static EAUploader ShowWindow()
    {
        var window = GetWindow<EAUploader>();
        new Vector2(900, 600);  // 最小サイズ
        window.maximized = true;  // ウィンドウを最大化
        window.Show();
        return window;
    }

    public static void CloseWindow()
    {
        var window = GetWindow<EAUploader>();
        if (window != null)
        {
            window.Close();
        }
    }

    private void OnEnable()
    {
        Type vrcSdkControlPanelType = Assembly.GetAssembly(typeof(VRCSdkControlPanel)).GetType("VRC.SDKBase.Editor.VRCSdkControlPanel");
        if (vrcSdkControlPanelType != null)
        {
            MethodInfo showWindowMethod = vrcSdkControlPanelType.GetMethod("ShowWindow", BindingFlags.Public | BindingFlags.Static);
            if (showWindowMethod != null)
            {
                showWindowMethod.Invoke(null, null);
            }
        }

        LibraryIndexer.CreateIndex();

        CustomPrefabUtility.UpdatePrefabInfo();
        CustomPrefabUtility.GetPrefabList();

        CheckForPackages();

        VRCWMarket.ClearProductsAndFetchNew();

        // WelcomeWindow.ShowWindow();
    }

    private void OnDisable()
    {
        // WelcomeWindow.CloseWindow();
        VRCWMarket.DeleteAllImagesInFolder();
        if (skipOnDisable)
        {
            return; // OnDisable処理をスキップ
        }

        /*
            【 SDK翻訳機能要 】
        // ユーザーにUnityを終了するかどうか尋ねる
        bool shouldCloseUnity = EditorUtility.DisplayDialog(
            Get(310),
            Get(311),
            Get(312),
            Get(313)
        );

        if (shouldCloseUnity)
        {
            // Unityエディタの現在のシーンを保存して終了
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorApplication.Exit(0);
            }
        }
        */
    }

    void Update()
    {
        currentLanguage = LanguageUtility.GetCurrentLanguage();
        if (currentLanguage != previousLanguage)
        {
            ReloadResourcesForLanguage();
            previousLanguage = currentLanguage;
        }

        CustomPrefabUtility.UpdatePrefabInfo();;
    }

    void ReloadResourcesForLanguage()
    {
        Repaint();
    }

    private void OnGUI()
    {
        styles.Initialize();
        this.minSize = new Vector2(900, 600);

        tabNames = new string[] { Get(160), Get(161), Get(162), Get(163) };

        string[] tabIcons = new string[] { "settings", "create", "upload", "travel_explor" };

        float buttonWidth = this.position.width / tabNames.Length;

        Color selectedTabColor = new Color(0.7f, 0.8f, 1.0f);
        
        GUILayout.BeginArea(new Rect(0, 0, this.position.width, this.position.height * 0.05f));
        GUILayout.BeginHorizontal();
        for (int i = 0; i < tabNames.Length; i++)
        {
            Texture2D icon = icons.GetIcon(tabIcons[i]);
            GUIContent content = new GUIContent(tabNames[i], icon);

            if (selectedTabIndex == i)
            {
                Color originalColor = GUI.backgroundColor;

                GUI.backgroundColor = selectedTabColor;

                if (GUILayout.Button(content, styles.TabButtonStyle, GUILayout.Width(buttonWidth)))
                {
                    ChangeTab(i);
                }

                GUI.backgroundColor = originalColor;
            }
            else
            {
                if (GUILayout.Button(content, styles.TabButtonStyle, GUILayout.Width(buttonWidth)))
                {
                    ChangeTab(i);
                }
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(0, this.position.height * 0.05f, this.position.width, this.position.height * 0.95f));
        switch (selectedTabIndex)
        {
            case 0:
                DrawImportSettingsTab();
                break;
            case 1:
                DrawSetUpTab();
                break;
            case 2:
                DrawUploadTab();
                break;
            case 3:
                DrawAvatarWorldTab();
                break;
        }
        GUILayout.EndArea();
    }

    private void DrawImportSettingsTab()
    {
        Rect currentRect = new Rect(0, 0, this.position.width, this.position.height);
        Tab0.Draw(currentRect);
    }

    private void DrawSetUpTab()
    {
        SetUpTabDrawer.Draw(position, prefabPaths, prefabNames, selectedPrefabName, selectedPrefabInstance);
    }

    private void DrawUploadTab()
    {
        UploadTabDrawer.Draw(position);
    }

    private void DrawAvatarWorldTab()
    {
        float tabAreaHeight = position.height * 0.05f;
        float contentAreaHeight = position.height * 0.95f;

        // タブエリア
        GUILayout.BeginArea(new Rect(0, 0, position.width, tabAreaHeight));
        GUILayout.BeginHorizontal();

        GUILayout.Space(20);

        // Market
        if (GUILayout.Button(Getc("badge", 121), SubButtonStyle))
        {
            selectedAvatarWorldTabIndex = 0;
        }

        // Other Market Place
        if (GUILayout.Button(Getc("store", 122), SubButtonStyle))
        {
            selectedAvatarWorldTabIndex = 1;
        }

        GUILayout.Space(20);

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(0, tabAreaHeight, position.width, contentAreaHeight));

        switch (selectedAvatarWorldTabIndex)
        {
            case 0:
                VRCWMarket.Draw(new Rect(0, 0, position.width, contentAreaHeight)); // Market内容
                break;
            case 1:
                AvatarWorldTabDrawer.Draw(new Rect(0, 0, position.width, contentAreaHeight)); // Other Market Place内容
                break;
        }

        GUILayout.EndArea();
    }

    private void CheckForPackages()
    {
        try
        {
            string manifestPath = "Packages/packages-lock.json";
            if(File.Exists(manifestPath))
            {
                string manifestContent = File.ReadAllText(manifestPath);
                HasVRM = manifestContent.Contains("\"com.vrmc.univrm\"");
                HasVRMConverter = manifestContent.Contains("\"jp.pokemori.vrm-converter-for-vrchat\"");
            }
            else
            {
                Debug.LogError("Manifest file not found.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to check for packages: " + e.Message);
        }
    }

    public static void ChangeTab(int Tabnum)
    {
        selectedTabIndex = Tabnum;
    }
}