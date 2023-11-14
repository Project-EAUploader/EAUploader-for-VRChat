using UnityEngine;
using System.IO;

public class ShaderEditorlabels
{
    private static string language;

    public static void UpdateLanguage()
    {
        language = LanguageUtility.GetCurrentLanguage();
        Initialize();
    }

    static ShaderEditorlabels()
    {
        language = LanguageUtility.GetCurrentLanguage();
    }

    // Here, define the labels specific to the ShaderEditor window
    public static string CloseButtonLabel;
    public static string Windowname;


    public static void Initialize()
    {
        switch (language)
        {
            case "English":
                CloseButtonLabel = "Close";
                Windowname = "Shader Edit";
                break;

            case "日本語":
                CloseButtonLabel = "閉じる";
                Windowname = "シェーダー変更";
                break;
        }
    }
}
