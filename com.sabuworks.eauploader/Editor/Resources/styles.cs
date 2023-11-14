using UnityEngine;
using UnityEditor;
using static Texture;

public class styles
{
    public static Font custumfont = Resources.Load<Font>(@"Font\NotoSansJP-Bold");
    public static GUIStyle MainButtonStyle;
    public static GUIStyle SubButtonStyle;
    public static GUIStyle SlotButtonStyle;
    public static GUIStyle HelpButtonStyle;
    public static GUIStyle h1Style;
    public static GUIStyle h2Style;
    public static GUIStyle h3Style; //ライトブルー
    public static GUIStyle h4Style; //margin0
    public static GUIStyle h5Style; //margin0ライトブルー
    public static GUIStyle h5BlackStyle;
    public static GUIStyle eStyle; // red text
    public static GUIStyle RichTextStyle;
    public static GUIStyle miniButtonStyle;
    public static GUIStyle miniButtonRedStyle;
    public static GUIStyle PopupStyle;
    public static GUIStyle largePopupStyle;
    public static GUIStyle drewborderBox;
    public static GUIStyle tabStyle;
    public static GUIStyle noBackgroundStyle;
    public static GUIStyle listBorderStyle; // list
    public static GUIStyle horizontalSeparatorStyle; // list
    public static GUIStyle prefabButtonStyle;

    public static void Initialize()
    {
        Texture2D borderTexture = Texture.GenerateBorderTexture(64, 64, Color.white, 2);
        RectOffset commonMargin = new RectOffset(50, 50, 10, 10); // Corrected line

        MainButtonStyle = new GUIStyle(GUI.skin.button)
        {
            font = custumfont,
            fontSize = 26,
            normal = { textColor = Color.white },
            hover = { textColor = Color.cyan },
            active = { textColor = Color.yellow },
            padding = new RectOffset(0, 0, 10, 10),
            margin = commonMargin
        };

        SubButtonStyle = new GUIStyle(GUI.skin.button)
        {
            font = custumfont,
            fontSize = 20,
            normal = { textColor = Color.white },
            hover = { textColor = Color.cyan },
            active = { textColor = Color.yellow },
            margin = commonMargin
        };

        SlotButtonStyle = new GUIStyle(GUI.skin.button)
        {
            font = custumfont,
            fontSize = 20,
            normal = { textColor = Color.white },
            hover = { textColor = Color.cyan },
            active = { textColor = Color.yellow },
            margin = commonMargin,
            fixedWidth = 80,
            fixedHeight = 30
        };

        HelpButtonStyle = new GUIStyle(GUI.skin.button)
        {
            font = custumfont,
            fixedWidth = 15,
            fixedHeight = 15,
            fontSize = 8,
            normal = { textColor = Color.white},
            hover = { textColor = Color.cyan },
            margin = new RectOffset(30, 0, 0, 0)
        };

        h1Style = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 22,
            normal = { textColor = Color.black },
            margin = commonMargin
        };
        h1Style.wordWrap = true;

        h2Style = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 16,
            normal = { textColor = Color.black },
            margin = commonMargin
        };
        h2Style.wordWrap = true;

        h3Style = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 16,
            normal = { textColor = Color.cyan},
            margin = commonMargin
        };
        h3Style.wordWrap = true;

        h4Style = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 14,
            normal = { textColor = Color.black },
            margin = new RectOffset(30, 30, 0, 0)
        };
        h4Style.wordWrap = true;

        h5Style = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 14,
            normal = { textColor = Color.cyan},
            margin = new RectOffset(30, 30, 0, 0)
        };
        h5Style.wordWrap = true;

        h5BlackStyle = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 14,
            normal = { textColor = Color.black},
            margin = new RectOffset(30, 30, 0, 0)
        };

        eStyle = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 12,
            normal = { textColor = Color.red},
            margin = commonMargin
        };

        RichTextStyle = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 14,
            normal = { textColor = Color.black},
            hover = { textColor = Color.black },
            padding = new RectOffset(10, 10, 30, 10),
            margin = commonMargin
        };
        RichTextStyle.wordWrap = true;
        RichTextStyle.richText = true;

        miniButtonStyle = new GUIStyle(GUI.skin.button)
        {
            font = custumfont,
            fontSize = 14,
            normal = { textColor = Color.white },
            hover = { textColor = Color.cyan },
            active = { textColor = Color.yellow },
            padding = new RectOffset(0, 0, 10, 10),
            margin = new RectOffset(10, 10, 0, 0)
        };

        miniButtonRedStyle = new GUIStyle(GUI.skin.button)
        {
            font = custumfont,
            fontSize = 14,
            normal = { textColor = Color.white },
            hover = { textColor = Color.red },
            active = { textColor = Color.yellow },
            padding = new RectOffset(0, 0, 10, 10),
            margin = new RectOffset(10, 10, 0, 0)
        };

        PopupStyle = new GUIStyle(EditorStyles.popup)
        {
            font = custumfont,
            fontSize = 14,
            margin = commonMargin
        };

        largePopupStyle = new GUIStyle(EditorStyles.popup)
        {
            font = custumfont,
            fontSize = 18,
            margin = commonMargin
        };

        drewborderBox = new GUIStyle(GUI.skin.box)
        {
            border = new RectOffset(2, 2, 2, 2),
            margin = commonMargin
        };
        drewborderBox.normal.background = borderTexture;

        tabStyle = new GUIStyle(EditorStyles.toolbarButton)
        {
            font = custumfont,
            fontSize = 16,
        };

        noBackgroundStyle = new GUIStyle()
        {
            normal = { background = null}
        };

        listBorderStyle = new GUIStyle()
        {
            normal = { background = Texture2D.blackTexture },
            padding = new RectOffset(1, 1, 1, 1)
        };

        horizontalSeparatorStyle = new GUIStyle()
        {
            normal = { background = Texture2D.blackTexture },
            margin = new RectOffset(0, 0, 5, 5)
        };

        prefabButtonStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { 
                background = Texture.MakeTex(2, 2, Color.white),
                textColor = Color.black
            },
            hover = {
                background = Texture.MakeTex(2, 2, Color.black),
                textColor = Color.white
                },
            alignment = TextAnchor.MiddleLeft,
            font = h4Style.font,
            fontSize = h4Style.fontSize,
            margin = h4Style.margin
        };
    }
}
