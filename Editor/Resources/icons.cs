using UnityEditor;
using UnityEngine;

public static class icons
{

    private static readonly string iconFolderPath = "Packages/com.sabuworks.eauploader/Editor/Resources/icons/";


    public static Texture2D GetIcon(string fileName)
    {
        string filePath = iconFolderPath + fileName + ".png";
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
        if (icon == null)
        {
            Debug.LogError("Icon not found: " + filePath);
        }
        return icon;
    }
}
