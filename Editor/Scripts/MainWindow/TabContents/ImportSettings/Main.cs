using UnityEditor;
using UnityEngine;
using static styles;
using static labels;

public class Tab0
{
    private static Settings settingsInstance = new Settings();

    public static void Draw(Rect position)
    {

        Rect windowRect = new Rect(0, 0, position.width, position.height);
        EditorGUI.DrawRect(windowRect, Color.white);


        float mainAreaWidth = position.width * 0.7f;
        float guideAreaWidth = position.width * 0.3f;


        float upperAreaHeight = position.height * 0.7f;
        float lowerAreaHeight = position.height * 0.3f;


        float borderWidth = 1;
        Color borderColor = Color.black;




        GUILayout.BeginVertical(GUILayout.Width(mainAreaWidth));


        GUILayout.BeginHorizontal(GUILayout.Height(upperAreaHeight));


        float importAreaWidth = mainAreaWidth * 0.3f;
        GUILayout.BeginArea(new Rect(0, 0, importAreaWidth, upperAreaHeight));
        Import.Draw(new Rect(0, 0, importAreaWidth, upperAreaHeight));
        GUILayout.EndArea();


        EditorGUI.DrawRect(new Rect(importAreaWidth, 0, borderWidth, position.height), borderColor);


        float managerAreaWidth = mainAreaWidth * 0.7f - borderWidth;
        GUILayout.BeginArea(new Rect(importAreaWidth + borderWidth, 0, managerAreaWidth, position.height));
        Manager.Draw(new Rect(0, 0, managerAreaWidth, position.height));
        GUILayout.EndArea();




        EditorGUI.DrawRect(new Rect(0, upperAreaHeight, importAreaWidth, borderWidth), borderColor);


        GUILayout.BeginHorizontal(GUILayout.Height(lowerAreaHeight));



        GUILayout.BeginArea(new Rect(0, upperAreaHeight, settingsAreaWidth, lowerAreaHeight - borderWidth));
        settingsInstance.Draw();
        GUILayout.EndArea();

        






        EditorGUI.DrawRect(new Rect(mainAreaWidth, 0, borderWidth, position.height), borderColor);


        GUILayout.BeginArea(new Rect(mainAreaWidth + borderWidth, 0, guideAreaWidth - borderWidth, position.height));
        Guide.Draw(new Rect(0, 0, guideAreaWidth - borderWidth, position.height));
        GUILayout.EndArea();


    }
}
