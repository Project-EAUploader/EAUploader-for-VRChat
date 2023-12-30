using UnityEditor;
using UnityEngine;
using static styles;
using static labels;

public class Tab0
{
    private static Settings settingsInstance = new Settings();

    public static void Draw(Rect position)
    {
        // 背景
        Rect windowRect = new Rect(0, 0, position.width, position.height);
        EditorGUI.DrawRect(windowRect, Color.white);

        // 水平方向の分割比率
        float mainAreaWidth = position.width * 0.7f;
        float guideAreaWidth = position.width * 0.3f;

        // 垂直方向の分割比率
        float upperAreaHeight = position.height * 0.7f;
        float lowerAreaHeight = position.height * 0.3f;

        // 境界線の幅
        float borderWidth = 1;
        Color borderColor = Color.black;

        GUILayout.BeginHorizontal(); // 全体の水平分割

        // 左側のエリア（Main Area）
        GUILayout.BeginVertical(GUILayout.Width(mainAreaWidth));

        // 上部エリア（ImportとManager）
        GUILayout.BeginHorizontal(GUILayout.Height(upperAreaHeight));

        // Importエリア
        float importAreaWidth = mainAreaWidth * 0.3f;
        GUILayout.BeginArea(new Rect(0, 0, importAreaWidth, upperAreaHeight));
        Import.Draw(new Rect(0, 0, importAreaWidth, upperAreaHeight));
        GUILayout.EndArea();

        // ImportとManagerの境界線
        EditorGUI.DrawRect(new Rect(importAreaWidth, 0, borderWidth, position.height), borderColor);

        // Managerエリア
        float managerAreaWidth = mainAreaWidth * 0.7f - borderWidth;
        GUILayout.BeginArea(new Rect(importAreaWidth + borderWidth, 0, managerAreaWidth, position.height));
        Manager.Draw(new Rect(0, 0, managerAreaWidth, position.height));
        GUILayout.EndArea();

        GUILayout.EndHorizontal(); // 上部エリア終了

        // 上部と下部の境界線
        EditorGUI.DrawRect(new Rect(0, upperAreaHeight, importAreaWidth, borderWidth), borderColor);

        // 下部エリア（Settingsのみに変更）
        GUILayout.BeginHorizontal(GUILayout.Height(lowerAreaHeight));

        // Settingsエリア
        float settingsAreaWidth = importAreaWidth; // (元) mainAreaWidth * 0.2f;
        GUILayout.BeginArea(new Rect(0, upperAreaHeight, settingsAreaWidth, lowerAreaHeight - borderWidth));
        settingsInstance.Draw();
        GUILayout.EndArea();

        /*
        // SettingsとEAInfoの境界線
        EditorGUI.DrawRect(new Rect(settingsAreaWidth, upperAreaHeight + borderWidth, borderWidth, lowerAreaHeight - borderWidth), borderColor);

        // EAInfoエリア
        float eaInfoAreaWidth = mainAreaWidth * 0.8f - borderWidth;
        GUILayout.BeginArea(new Rect(settingsAreaWidth + borderWidth, upperAreaHeight + borderWidth, eaInfoAreaWidth, lowerAreaHeight - borderWidth));
        EAInfo.Draw(new Rect(0, 0, eaInfoAreaWidth, lowerAreaHeight - borderWidth));
        GUILayout.EndArea();
        */

        GUILayout.EndHorizontal(); // 下部エリア終了

        GUILayout.EndVertical(); // 左側のエリア終了

        // 左側と右側の境界線
        EditorGUI.DrawRect(new Rect(mainAreaWidth, 0, borderWidth, position.height), borderColor);

        // 右側のエリア（Guide Area）
        GUILayout.BeginArea(new Rect(mainAreaWidth + borderWidth, 0, guideAreaWidth - borderWidth, position.height));
        Guide.Draw(new Rect(0, 0, guideAreaWidth - borderWidth, position.height));
        GUILayout.EndArea();

        GUILayout.EndHorizontal(); // 全体の水平分割終了
    }
}
