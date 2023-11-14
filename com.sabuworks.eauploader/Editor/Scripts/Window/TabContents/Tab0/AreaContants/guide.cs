using UnityEditor;
using UnityEngine;

public class Guide
{
    public static void Draw(Rect position)
    {
        EditorGUI.LabelField(new Rect(position.width * 0.5f, position.height * 0.5f, 100, 20), "Guide");
    }
}
