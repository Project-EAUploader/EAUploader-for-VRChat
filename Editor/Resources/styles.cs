using UnityEngine;
using UnityEditor;
using static Texture;

public class styles
{
    public static Font custumfont = Resources.Load<Font>("Font/NotoSansJP-Bold");
    public static GUIStyle MainButtonStyle;
    public static GUIStyle SubButtonStyle;
    public static GUIStyle SubButtonTransmissionStyle;
    public static GUIStyle SlotButtonStyle;
    public static GUIStyle HelpButtonStyle;
    public static GUIStyle MarketProductsButtonStyle;
    public static GUIStyle h1LabelStyle;
    public static GUIStyle h1WhiteLabelStyle;
    public static GUIStyle h2LabelStyle;
    public static GUIStyle NoMargeh2LabelStyle;
    public static GUIStyle NoMargeWhiteh2LabelStyle;
    public static GUIStyle Centerh2LabelStyle;
    public static GUIStyle h3LabelStyle;
    public static GUIStyle h4LabelStyle;
    public static GUIStyle h5LabelStyle;
    public static GUIStyle h5BlackLabelStyle;
    public static GUIStyle NoMarginh5BlackLabelStyle;
    public static GUIStyle CenteredStyle;
    public static GUIStyle MarketproductsLabelStyle;
    public static GUIStyle eLabel;
    public static GUIStyle RichTextLabelStyle;
    public static GUIStyle miniButtonStyle;
    public static GUIStyle miniButtonRedStyle;
    public static GUIStyle PopupStyle;
    public static GUIStyle largePopupStyle;
    public static GUIStyle drewborderBox;
    public static GUIStyle tabStyle;
    public static GUIStyle noBackgroundStyle;
    public static GUIStyle listBorderStyle;
    public static GUIStyle prefabButtonStyle; // 透明ボタン
    public static GUIStyle FoldoutButtonStyle;
    public static GUIStyle LinkButtonStyle;
    public static GUIStyle horizontalSeparatorStyle;
    public static GUIStyle h4CenterLabelStyle;
    public static GUIStyle LibraryButtonStyle;
    public static GUIStyle TabButtonStyle;
    public static GUIStyle TextFieldStyle;
    public static GUIStyle TextAreaStyle;
    public static GUIStyle SearchButtonStyle;
    public static GUIStyle ClearButtonStyle;

    public static void Initialize()
    {
        Texture2D borderTexture = Texture.GenerateBorderTexture(64, 64, Color.white, 2);
        RectOffset commonMargin = new RectOffset(50, 50, 10, 10);

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

        SubButtonTransmissionStyle = new GUIStyle(GUI.skin.button)
        {
            font = custumfont,
            fontSize = 20,
            normal = { 
                textColor = Color.white,
                background = null },
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
            fixedWidth = 75,
            fixedHeight = 20,
            fontSize = 14,
            normal = { textColor = Color.white},
            hover = { textColor = Color.cyan },
            margin = new RectOffset(50, 0, 0, 0)
        };

        MarketProductsButtonStyle = new GUIStyle(GUI.skin.button)
        {
            font = custumfont,
            fixedHeight = 30,
            fontSize = 18,
            normal = { textColor = Color.white},
            hover = { textColor = Color.cyan }
        };

        h1LabelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            font = custumfont,
            fontSize = 22,
            normal = { textColor = Color.black },
            margin = commonMargin
        };
        h1LabelStyle.wordWrap = true;

        h1WhiteLabelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            font = custumfont,
            fontSize = 22,
            normal = { textColor = Color.white },
            margin = commonMargin
        };
        h1WhiteLabelStyle.wordWrap = true;

        h2LabelStyle = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 16,
            normal = { textColor = Color.black },
            margin = commonMargin
        };
        h2LabelStyle.wordWrap = true;

        NoMargeh2LabelStyle = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 16,
            normal = { textColor = Color.black },
        };
        NoMargeh2LabelStyle.wordWrap = true;

        NoMargeWhiteh2LabelStyle = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 20,
            normal = { textColor = Color.white },
        };
        NoMargeWhiteh2LabelStyle.wordWrap = true;

        Centerh2LabelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            font = custumfont,
            fontSize = 16,
            normal = { textColor = Color.white },
        };
        Centerh2LabelStyle.wordWrap = true;

        h3LabelStyle = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 16,
            normal = { textColor = Color.cyan},
            margin = commonMargin
        };
        h3LabelStyle.wordWrap = true;

        h4LabelStyle = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 14,
            normal = { textColor = Color.black },
            margin = new RectOffset(30, 30, 0, 0)
        };
        h4LabelStyle.wordWrap = true;

        h5LabelStyle = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 14,
            normal = { textColor = Color.cyan},
            margin = new RectOffset(30, 30, 0, 0)
        };
        h5LabelStyle.wordWrap = true;

        h5BlackLabelStyle = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 14,
            normal = { textColor = Color.black},
            margin = new RectOffset(30, 30, 0, 0)
        };
        h5BlackLabelStyle.wordWrap = true;

        NoMarginh5BlackLabelStyle = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 14,
            normal = { textColor = Color.black}
        };

        MarketproductsLabelStyle = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 14,
            normal = { textColor = Color.white}
        };
        MarketproductsLabelStyle.wordWrap = true;

        CenteredStyle = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 18,
            normal = { textColor = Color.black },
            margin = commonMargin,
            alignment = TextAnchor.MiddleCenter
        };
        CenteredStyle.wordWrap = true;

        eLabel = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 12,
            normal = { textColor = Color.red},
            margin = commonMargin
        };

        RichTextLabelStyle = new GUIStyle(GUI.skin.label)
        {
            font = custumfont,
            fontSize = 14,
            normal = { textColor = Color.black},
            hover = { textColor = Color.black }
        };
        RichTextLabelStyle.richText = true;

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
            normal = { textColor = Color.white },
            fontSize = 14,
        };

        largePopupStyle = new GUIStyle(EditorStyles.popup)
        {
            font = custumfont,
            fontSize = 18,
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
            font = h4LabelStyle.font,
            fontSize = h4LabelStyle.fontSize,
            wordWrap = true
        };

        FoldoutButtonStyle = new GUIStyle(GUI.skin.button)
        {
            font = custumfont,
            fontSize = 14,
            normal = { background = Texture.MakeTex(2, 2, Color.white), textColor = Color.black },
            hover = { textColor = Color.cyan },
            active = { textColor = Color.yellow },
            padding = new RectOffset(0, 0, 10, 10),
            margin = new RectOffset(10, 10, 0, 0)
        };

        LinkButtonStyle = new GUIStyle(GUI.skin.button)
        {
            font = custumfont,
            fontSize = 14,
            normal = { background = Texture.MakeTex(2, 2, Color.white), textColor = Color.blue},
            margin = commonMargin
        };

        horizontalSeparatorStyle = new GUIStyle()
        {
            normal = { background = Texture2D.blackTexture },
            margin = new RectOffset(0, 0, 5, 5),
            fixedHeight = 1 // 線の高さ
        };

        h4CenterLabelStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            font = custumfont,
            fontSize = 14,
            normal = { textColor = Color.black },
            margin = new RectOffset(30, 30, 0, 0)
        };

        LibraryButtonStyle = new GUIStyle(GUI.skin.button)
        {
            font = custumfont,
            fontSize = 18,
            normal = {
                background = Texture.MakeTex(2, 2, Color.white),
                textColor = Color.black
            },
            hover = {
                background = Texture.MakeTex(2, 2, new Color(0.9f, 0.9f, 0.9f)),
                textColor = Color.black
            },
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true,
            margin = new RectOffset(10, 10, 10, 10),
            padding = new RectOffset(10, 10, 10, 10)
        };

        TabButtonStyle = new GUIStyle(GUI.skin.button)
        {
            font = custumfont,
            fontSize = 26,
            normal = { textColor = Color.white },
            hover = { textColor = Color.cyan },
            active = { textColor = Color.yellow }
        };

        TextFieldStyle = new GUIStyle(GUI.skin.textField)
        {
            normal = { 
                background = Texture.MakeTex(2, 2, new Color(0.9f, 0.9f, 0.9f)), // 白に近い背景色
                textColor = Color.black
            },
            alignment = TextAnchor.MiddleLeft,
            font = custumfont,
            fontSize = 12,
        };
        TextFieldStyle.wordWrap = true;

        TextAreaStyle = new GUIStyle(GUI.skin.textArea)
        {
            normal = { 
                background = Texture.MakeTex(2, 2, new Color(0.9f, 0.9f, 0.9f)), // 白に近い背景色
                textColor = Color.black
            },
            alignment = TextAnchor.UpperLeft,
            font = custumfont,
            fontSize = 12,
        };
        TextFieldStyle.wordWrap = true;

        SearchButtonStyle = new GUIStyle(GUI.skin.button)
        {
            font = custumfont,
            fontSize = 26,
            normal = { textColor = Color.white },
            hover = { textColor = Color.cyan },
            active = { textColor = Color.yellow },
        };

        ClearButtonStyle = new GUIStyle(GUI.skin.button)
        {
            font = custumfont,
            fontSize = 26,
            normal = { background = Texture.MakeTex(2, 2, new Color(0, 0, 0, 0)),　textColor = Color.white },
            hover = { textColor = Color.cyan },
            active = { textColor = Color.yellow },
        };
    }
}
