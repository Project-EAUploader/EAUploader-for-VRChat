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

    public static Texture2D MakeModernButtonTex(int width, int height, Color bgColor, Color borderColor, int borderWidth, bool addShadow = false)
    {
        Texture2D texture = new Texture2D(width, height);


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {

                if (x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth)
                {
                    texture.SetPixel(x, y, borderColor);
                }

                else if (addShadow && y < borderWidth + 5)
                {
                    float alpha = shadowColor.a * (1 - (float)y / (borderWidth + 5));
                    Color currentColor = bgColor;
                    currentColor.a *= alpha;
                    texture.SetPixel(x, y, currentColor);
                }
                else
                {
                    texture.SetPixel(x, y, bgColor);
                }
            }
        }
        texture.Apply();
        return texture;
    }
}