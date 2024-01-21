using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using VRC.SDK3A.Editor;

[InitializeOnLoad]
public class BuildScriptManager
{
    // 複数のスクリプトファイルパスを管理する配列
    private static string[] scriptPaths = new string[]
    {
        "Packages/tech.uslog.eauploader/Editor/Scripts/EAInitialization.cs",
        "Packages/tech.uslog.eauploader/Editor/Scripts/MainWindow/EAUploader.cs",
        "Packages/tech.uslog.eauploader/Editor/Scripts/CustomPrefabUtility.cs"
    };

    static BuildScriptManager()
    {
        VRCSdkControlPanel.OnSdkPanelEnable += AddBuildHook;
    }

    private static void AddBuildHook(object sender, EventArgs e)
    {
        if (VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder))
        {
            builder.OnSdkBuildStart += OnBuildStart;
            builder.OnSdkBuildFinish += OnBuildFinish;
        }
    }

    private static void OnBuildStart(object sender, object target)
    {
        EAUploader.CloseWindow();
        foreach (var scriptPath in scriptPaths)
        {
            if (File.Exists(scriptPath))
            {
                File.Move(scriptPath, scriptPath + ".disabled");
                Debug.Log("Build started. Script disabled: " + scriptPath);
            }
        }
        AssetDatabase.Refresh();
    }

    private static void OnBuildFinish(object sender, object target)
    {
        foreach (var scriptPath in scriptPaths)
        {
            string disabledScriptPath = scriptPath + ".disabled";
            if (File.Exists(disabledScriptPath))
            {
                File.Move(disabledScriptPath, scriptPath);
                Debug.Log("Build finished. Script enabled: " + scriptPath);
            }
        }
        AssetDatabase.Refresh();
    }
}
