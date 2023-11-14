using UnityEditor;
using UnityEngine;
using static styles;
using static labels;

public class Tab0
{
    private static Settings settingsInstance = new Settings();

    // スクロール
    private static Vector2 settingsScrollPosition;
    private static Vector2 importScrollPosition;
    private static Vector2 managerScrollPosition;
    private static Vector2 guideScrollPosition;

    public static void Draw(Rect position)
    {
        // 背景
        Rect windowRect = new Rect(0, 0, position.width, position.height);
        EditorGUI.DrawRect(windowRect, Color.white);

        float mainWidth = position.width * 0.7f;
        float guideWidth = position.width * 0.3f;

        float importWidth = mainWidth * 0.3f;
        float managerWidth = mainWidth * 0.7f;

        float mainContentHeight = position.height * 0.8f;
        float settingsHeight = position.height * 0.2f;

        float currentXPosition = 0;

        GUILayout.BeginVertical();

        // Main content area
        GUILayout.BeginVertical(GUILayout.Height(mainContentHeight));

        // Import
        GUILayout.BeginArea(new Rect(currentXPosition, 0, importWidth, mainContentHeight));
        importScrollPosition = GUILayout.BeginScrollView(importScrollPosition);
        Import.Draw(new Rect(0, 0, importWidth, mainContentHeight));
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        currentXPosition += importWidth;

        // Boundary between Import and Manager
        EditorGUI.DrawRect(new Rect(currentXPosition, 0, 1, mainContentHeight), Color.black);
        currentXPosition += 1; // Move past the boundary

        // Manager
        GUILayout.BeginArea(new Rect(currentXPosition, 0, managerWidth, mainContentHeight));
        managerScrollPosition = GUILayout.BeginScrollView(managerScrollPosition);
        Manager.Draw(new Rect(0, 0, managerWidth, mainContentHeight));
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        currentXPosition += managerWidth;

        // Boundary between Manager and Guide
        EditorGUI.DrawRect(new Rect(currentXPosition, 0, 1, position.height), Color.black);

        currentXPosition += 1;  // Move past the boundary

        // Guide
        GUILayout.BeginArea(new Rect(currentXPosition, 0, guideWidth, position.height));
        guideScrollPosition = GUILayout.BeginScrollView(guideScrollPosition);
        Guide.Draw(new Rect(0, 0, guideWidth, position.height));
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        GUILayout.EndVertical();

        // Boundary above Settings
        EditorGUI.DrawRect(new Rect(0, mainContentHeight, mainWidth, 1), Color.black);

        // Settings (Horizontally aligned elements)
        GUILayout.BeginArea(new Rect(0, mainContentHeight, mainWidth, settingsHeight));
        GUILayout.BeginHorizontal();  // Begin horizontal group for Settings content
        settingsScrollPosition = GUILayout.BeginScrollView(settingsScrollPosition);
        settingsInstance.Draw();
        GUILayout.EndScrollView();
        GUILayout.EndHorizontal();    // End horizontal group
        GUILayout.EndArea();

        GUILayout.EndVertical();
    }
}
