using UnityEngine;
using UnityEditor;

/*
    stylesで使用してる
    同じことしてる関数が二つあるけど、
    どっちも使ってる
*/

internal static class Texture
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
}