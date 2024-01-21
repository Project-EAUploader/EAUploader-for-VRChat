using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using VRC.SDK3A.Editor;

[InitializeOnLoad]
public class BuildScriptManager
{
    private static string scriptPath = "Packages/tech.uslog.eauploader/Editor/Scripts/EAInitialization.cs";

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
        if (File.Exists(scriptPath))
        {
            File.Move(scriptPath, scriptPath + ".disabled");
            AssetDatabase.Refresh();
            Debug.Log("Build started. Script disabled.");
        }
    }

    private static void OnBuildFinish(object sender, object target)
    {
        string disabledScriptPath = scriptPath + ".disabled";
        if (File.Exists(disabledScriptPath))
        {
            File.Move(disabledScriptPath, scriptPath);
            AssetDatabase.Refresh();
            Debug.Log("Build finished. Script enabled.");
        }
    }
}
