using UnityEngine;
using UnityEditor;

public static class UploadTabDrawer
{
    private static readonly float BorderWidth = 2f;

    public static void Draw(Rect position)
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), Color.white);
        
        GUILayout.BeginHorizontal();
        
        Rect buildArea = new Rect(0, 0, position.width * 0.7f - BorderWidth, position.height);
        GUILayout.BeginArea(buildArea);
        EABuilder.Draw(buildArea);
        GUILayout.EndArea();

        EditorGUI.DrawRect(new Rect(buildArea.x + buildArea.width, 0, BorderWidth, position.height), Color.gray);

        Rect guideArea = new Rect(position.width * 0.7f, 0, position.width * 0.3f, position.height);
        GUILayout.BeginArea(guideArea);
        EAUploaderGuide.Draw(new Rect(0, 0, guideArea.width, guideArea.height));
        GUILayout.EndArea();

        GUILayout.EndHorizontal();
    }
}
