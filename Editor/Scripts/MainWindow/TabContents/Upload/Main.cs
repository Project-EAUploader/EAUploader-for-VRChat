using UnityEngine;
using UnityEditor;

public static class UploadTabDrawer
{
    private static readonly float BorderWidth = 2f; // 境界線の幅

    public static void Draw(Rect position)
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), Color.white);
        
        GUILayout.BeginHorizontal();
        
        // Build
        Rect buildArea = new Rect(0, 0, position.width * 0.5f - BorderWidth, position.height); // - BorderWidthを追加して境界線の幅を考慮します。
        GUILayout.BeginArea(buildArea);
        EABuilder.Draw(buildArea);
        GUILayout.EndArea();

        EditorGUI.DrawRect(new Rect(buildArea.x + buildArea.width, 0, BorderWidth, position.height), Color.gray);

        // Guide
        Rect guideArea = new Rect(position.width * 0.5f, 0, position.width * 0.5f, position.height);
        GUILayout.BeginArea(guideArea);
        EAUploaderGuide.Draw(new Rect(0, 0, guideArea.width, guideArea.height));
        GUILayout.EndArea();

        GUILayout.EndHorizontal();
    }
}
