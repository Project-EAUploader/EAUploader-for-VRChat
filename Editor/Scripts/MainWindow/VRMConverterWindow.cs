/* 制作中
using UnityEngine;
using UnityEditor;
using Esperecyan.Unity.VRMConverterForVRChat;
using System.Collections.Generic;
using System.Linq;

public class VRMConverterWindow : EditorWindow
{
    private GameObject vrmPrefabInstance;
    private string vrmFilePath;
    private Converter.SwayingObjectsConverterSetting swayingObjectsSetting;
    private bool forQuest;
    private bool takingOverSwayingParameters;
    private bool useShapeKeyNormalsAndTangents;
    private Converter.OSCComponents oscComponents;

    // VRMファイルパスを渡してウィンドウを表示する
    public static void ShowModal(string vrmPath)
    {
        var window = ScriptableObject.CreateInstance<VRMConverterWindow>();
        window.vrmFilePath = vrmPath;
        window.titleContent = new GUIContent("VRM Converter for VRChat");
        window.minSize = new Vector2(400, 300);
        window.ShowModal();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("VRM Converter for VRChat", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // VRMファイルパス表示
        EditorGUILayout.LabelField("VRM File Path:", vrmFilePath);

        // 揺れ物の設定
        swayingObjectsSetting = (Converter.SwayingObjectsConverterSetting)EditorGUILayout.EnumPopup("Swaying Objects Setting", swayingObjectsSetting);

        // Quest対応
        forQuest = EditorGUILayout.Toggle("Convert for Quest", forQuest);

        // 揺れ物のパラメータ引継ぎ
        takingOverSwayingParameters = EditorGUILayout.Toggle("Take Over Swaying Parameters", takingOverSwayingParameters);

        // シェイプキーの法線・接線の使用
        useShapeKeyNormalsAndTangents = EditorGUILayout.Toggle("Use Shape Key Normals and Tangents", useShapeKeyNormalsAndTangents);

        // OSCコンポーネント
        oscComponents = (Converter.OSCComponents)EditorGUILayout.EnumFlagsField("OSC Components", oscComponents);

        GUILayout.Space(20);

        // 変換ボタン
        if (GUILayout.Button("Convert", GUILayout.Height(40)))
        {
            ConvertVRM();
        }
    }

    private void ConvertVRM()
    {
        vrmPrefabInstance = AssetDatabase.LoadAssetAtPath<GameObject>(vrmFilePath);

        if (vrmPrefabInstance == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to load VRM file.", "OK");
            return;
        }

        var clips = VRMUtility.GetAllVRMBlendShapeClips(vrmPrefabInstance);

        if (!AreRequiredBlendShapesPresent(clips))
        {
            EditorUtility.DisplayDialog("Error", "Required blend shape clips are missing.", "OK");
            return;
        }

        // 変換オプションを指定して変換を実行
        IEnumerable<(string message, MessageType type)> messages = Converter.Convert(
            vrmPrefabInstance,
            clips,
            forQuest,
            swayingObjectsSetting,
            takingOverSwayingParameters,
            useShapeKeyNormalsAndTangents: useShapeKeyNormalsAndTangents,
            oscComponents: oscComponents
        );

        // 変換結果のメッセージを表示
        ShowConversionResult(messages);
    }

    private void ShowConversionResult(IEnumerable<(string message, MessageType type)> messages)
    {
        foreach (var (message, type) in messages)
        {
            switch (type)
            {
                case MessageType.Error:
                    Debug.LogError(message);
                    break;
                case MessageType.Warning:
                    Debug.LogWarning(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }

        EditorUtility.DisplayDialog("Conversion Completed", "VRM conversion completed successfully.", "OK");
    }

    private bool AreRequiredBlendShapesPresent(IEnumerable<VRMBlendShapeClip> clips)
    {
        // 必要なブレンドシェイプ名のリスト
        var requiredBlendShapes = new List<string> { 必要なブレンドシェイプ名 };

        foreach (var requiredShape in requiredBlendShapes)
        {
            if (!clips.Any(clip => clip.BlendShapeName == requiredShape))
            {
                return false; // 必要なブレンドシェイプが見つからない
            }
        }

        return true; // すべての必要なブレンドシェイプが存在する
    }
}
*/