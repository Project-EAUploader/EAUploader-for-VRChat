#if !EA_ONBUILD
using UnityEditor;
using UnityEngine;
using System.IO;
using static styles;
using static labels;
using EAUploaderEditors;

public class SetUpTabDrawer
{
    public static void Draw(Rect position, string[] prefabPaths, string[] prefabNames, string selectedPrefabName, GameObject selectedPrefabInstance)
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), Color.white);

        GUILayout.BeginHorizontal();

        GUILayout.BeginArea(new Rect(0, 0, position.width * 0.3f, position.height));

        var localPrefabPaths = CustomPrefabUtility.GetPrefabPaths();
        var localPrefabNames = CustomPrefabUtility.GetPrefabNames();

        Setup.Draw(localPrefabPaths.ToArray(), localPrefabNames.ToArray());
        GUILayout.EndArea();

        EditorGUI.DrawRect(new Rect(position.width * 0.3f - 1, 0, 2, position.height), Color.black); 

        GUILayout.BeginArea(new Rect(position.width * 0.3f + 1, 0, position.width * 0.4f - 2, position.height));
        Rect previewRect = new Rect(0, 0, position.width * 0.4f - 2, position.height);
        Preview.Draw(CustomPrefabUtility.selectedPrefabInstance, previewRect);
        GUILayout.EndArea();

        EditorGUI.DrawRect(new Rect(position.width * 0.7f - 1, 0, 2, position.height), Color.black);

        GUILayout.BeginArea(new Rect(position.width * 0.7f + 1, 0, position.width * 0.3f - 2, position.height));
        EAUploaderEditors.Editors.Draw(new Rect(0, 0, position.width * 0.3f - 2, position.height));
        GUILayout.EndArea();

        GUILayout.EndHorizontal();
    }
}
#endif