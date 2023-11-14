using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using static styles;
using static labels;

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
    private int selectedTabIndex = 1;
    private string[] tabNames;
    int currentWindowWidth;
    int currentWindowHeight;
    public static string[] prefabPaths; // Array to hold prefab paths and names
    public static string[] prefabNames;
    int selectedPrefabIndex; // Variable to hold the index of the selected prefab
    public static GameObject selectedPrefabInstance; // Variable to hold an instance of the selected Prefab
    public static string selectedPrefabName;
    private bool showDropdown;
    private Rect dropdownRect;
    public static string overlayLabel; //help用
    private Texture2D overlayBackgroundImage; // help背景
    private Vector2 _scrollPosition = Vector2.zero;
    private Vector2 PrefablistscrollPosition = Vector2.zero;
    private static string _message;
    private static string _previousMessage;
    private string currentLanguage;
    private string previousLanguage;
    private int overlayLabelIndex;
    public static bool overlayLabelVisible = false;
    public static bool HasVRM;
    public static bool HasVRMConverter;

    public static EAUploader ShowWindow()
    {
        var window = GetWindow<EAUploader>();
        window.minSize = new Vector2(900, 600);  // 最小サイズを設定
        window.maximized = true;  // ウィンドウを最大化
        window.Show();
        return window;
    }

    // Method called when the EAUploader window is opened
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

        // Update Prefab list
        CustomPrefabUtility.UpdatePrefabList();
        Prefabop.Refresh(out prefabPaths, out prefabNames);

        CheckForPackages();

    }

    // Method called when the EAUploader window is closed
    private void OnDisable()
    {

    }

    void Update()
    {
        // Check if the language has changed
        currentLanguage = LanguageUtility.GetCurrentLanguage(); // Update current language
        if (currentLanguage != previousLanguage)
        {
            ReloadResourcesForLanguage();
            previousLanguage = currentLanguage; // Store the current language for comparison in the next frame
        }

        CustomPrefabUtility.UpdatePrefabList();
    }

    void ReloadResourcesForLanguage()
    {
        Repaint();
    }

    // GUIを描画するメソッド
    private void OnGUI()
    {
        styles.Initialize();
        labels.Initialize();

        this.minSize = new Vector2(900, 600);

        // タブの描画
        tabNames = new string[] { labels.Tab0, labels.Tab1, labels.Tab2 };
        selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabNames, styles.tabStyle);

        // 指定された位置からの範囲を開始
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
        }

        // 範囲を終了
        GUILayout.EndArea();
    }

    private void DrawImportSettingsTab()
    {
        Rect currentRect = new Rect(0, 0, this.position.width, this.position.height);
        Tab0.Draw(currentRect);
    }

    private void DrawSetUpTab()
    {
        SetUpTabDrawer.Draw(position, _scrollPosition, prefabPaths, prefabNames, selectedPrefabName, selectedPrefabInstance);
    }

    private void DrawUploadTab()
    {
        UploadTabDrawer.Draw(position);
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

                Debug.Log($"HasVRM: {HasVRM}, HasVRMConverter: {HasVRMConverter}");
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


}