using UnityEngine;
using UnityEditor;

public static class Texture
{
    public static Texture2D GenerateBorderTexture(int width, int height, Color borderColor, int borderWidth)
    {
        Texture2D texture = new Texture2D(width, height);
        
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                if (x < borderWidth || x > width - borderWidth || y < borderWidth || y > height - borderWidth)
                {
                    texture.SetPixel(x, y, borderColor);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();
        
        return texture;
    }

    public static Texture2D LoadTextureFromPath(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    public static Texture2D CreateBorderTexture(int width, int height, Color borderColor, int borderWidth)
    {
        Texture2D texture = new Texture2D(width, height);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                if (x < borderWidth || x > width - borderWidth || y < borderWidth || y > height - borderWidth)
                {
                    texture.SetPixel(x, y, borderColor);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();

        return texture;
    }

    public static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    /// <summary>
    /// color色でfontSizeサイズの横線をwidth長さ描画
    /// </summary>
    /// <param name="color"></param>
    /// <param name="fontSize"></param>
    /// <param name="width"></param>
    public static void DrawHorizontalLine(Color color, int fontSize, float width)
    {
        GUIStyle lineLabel = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = color },
            fontSize = fontSize,
            alignment = TextAnchor.MiddleLeft
        };

        string lineText = new string('―', Mathf.FloorToInt(width / fontSize));

        GUILayout.Label(lineText, lineLabel, GUILayout.Width(width));
    }

    /// <summary>
    /// DrawHorizontalLineの点線版
    /// </summary>
    /// <param name="color"></param>
    /// <param name="fontSize"></param>
    /// <param name="width"></param>
    public static void DrawHorizontalDottedLine(Color color, int fontSize, float width)
    {
        GUIStyle lineLabel = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = color },
            fontSize = fontSize,
            alignment = TextAnchor.MiddleLeft
        };

        string lineText = new string('-', Mathf.FloorToInt(width / fontSize));

        GUILayout.Label(lineText, lineLabel, GUILayout.Width(width));
    }

    /// <summary>
    /// DrawHorizontalDottedLineを中心にそろえて描画
    /// </summary>
    /// <param name="color"></param>
    /// <param name="fontSize"></param>
    /// <param name="width"></param>
    public static void DrawHorizontalDottedCenterLine(Color color, int fontSize, float width)
    {
        GUIStyle lineLabel = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = color },
            fontSize = fontSize,
            alignment = TextAnchor.MiddleCenter
        };

        string lineText = new string('-', Mathf.FloorToInt(width / fontSize));

        GUILayout.Label(lineText, lineLabel, GUILayout.Width(width));
    }
}