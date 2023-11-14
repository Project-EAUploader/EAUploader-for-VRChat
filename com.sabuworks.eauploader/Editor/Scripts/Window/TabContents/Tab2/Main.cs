using UnityEngine;
using UnityEditor;

public static class UploadTabDrawer
{
    private static readonly float BorderWidth = 2f; // 境界線の幅を定義します。

    public static void Draw(Rect position)
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), Color.white);
        
        GUILayout.BeginHorizontal();
        
        // Build
        Rect buildArea = new Rect(0, 0, position.width * 0.7f - BorderWidth, position.height); // - BorderWidthを追加して境界線の幅を考慮します。
        GUILayout.BeginArea(buildArea);
        EABuilder.Draw(buildArea); // Rectを渡しています。
        GUILayout.EndArea();

        // 境界線を描画します。
        EditorGUI.DrawRect(new Rect(buildArea.x + buildArea.width, 0, BorderWidth, position.height), Color.gray);

        // Guide
        Rect guideArea = new Rect(position.width * 0.7f, 0, position.width * 0.3f, position.height);
        GUILayout.BeginArea(guideArea);
        EAUploaderGuide.Draw(); // このメソッドはRectを必要としないと仮定しています。
        GUILayout.EndArea();

        GUILayout.EndHorizontal();
    }
}
