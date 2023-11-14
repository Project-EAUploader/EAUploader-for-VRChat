using UnityEngine;
using System.IO;

public class labels
{
    Font myJapaneseFont = Resources.Load<Font>(@"Packages\com.sabuworks.eauploader\Editor\Resources\Font\NotoSansJP-Bold.ttf");

    private static string language;

    public static void UpdateLanguage()
    {
        language = LanguageUtility.GetCurrentLanguage();
        Initialize();
    }

    static labels()
    {
        language = LanguageUtility.GetCurrentLanguage();
    }

    // Tab0
    public static string Changelng;
    public static string T1; // Tan0 Settings Title
    public static string T2; // インポート
    public static string T3; // モデル管理
    public static string T4; // VRMインポート
    public static string C1; // Prefabインポート
    public static string C2; // Unityパッケージインポート
    public static string C3; // プロジェクト内のプレハブ
    public static string C4; // VRMインポート
    public static string C4no; // UniVRMなし
    public static string C5; // Select setup
    public static string C5no; 
    public static string C6; // 選択中のプレファブ
    public static string C6no;
    public static string C7; // VRChat用アバターなし
    public static string C8; //
    public static string C9; //
    public static string C10; //
    public static string C11; //
    public static string C12; //
    public static string C13; //
    public static string C14; //
    public static string C15; //
    public static string C16; //
    public static string C17; // 
    public static string C18; // 
    public static string C19; //
    public static string C20; //
    public static string C21; //
    public static string C22; //
    public static string C23; //
    public static string C24; //
    public static string C25; //
    public static string C26; //
    public static string C27; //

    public static string B1; // Import Prefab
    public static string B2; // Import UnityPackage
    public static string B3; // update prefab list
    public static string B4; // delete prefab
    public static string B4C1;
    public static string B4C2;
    public static string B4B1;
    public static string B4B2;
    public static string B5; // VRMインポート
    public static string B6; // UniVRMインポート
    public static string B7; // string select
    public static string B8; // build
    public static string B9;
    public static string H1; // Editors
    public static string H2; // アップロード可能
    public static string H3; // 
    public static string H4; // 
    public static string H5; // 
    public static string SC1;
    public static string SC2;
    public static string SC3;
    public static string GC1;
    public static string Tab0;
    public static string Tab1;
    public static string Tab2;

    public static void Initialize()
    {
        switch (language)
        {
            case "English":
                Changelng = "Language: ";
                T1 = "Settings";
                T2 = "Import";
                T3 = "Manage models";
                T4 = "Import VRM -using UniVRM";
                C1 = "Import the .prefab file.";
                C2 = "Import prefab. This may contain various settings such as animation as well as the 3D model.";
                C3 = "Prefabs List";
                C4 = "Import the .vrm(ver. 0.x) file.";
                C4no = "To use VRM files in Unity, you must import the UniVRM package.";
                C5 = "Select an avatar to set up.";
                C5no = "There are no instances available for upload.\n Please import your avatar and press Update List.";
                C6 = "Now selecting: ";
                C6no = "";
                C7 = "There are no avatars available for upload. \nIf you want to use a VRM avatar, you will need to convert it.";
                C8 = "";
                C9 = "";
                C10 = "";
                C11 = "";
                C12 = "";
                C13 = "";
                C14 = "";
                C15 = "";
                C16 = "";
                C17 = "";
                C18 = "";
                C19 = "";
                C20 = "";
                C21 = "";
                C22 = "";
                C23 = "";
                C24 = "";
                C25 = "";
                C26 = "";
                C27 = "";
                B1 = "Import Prefab";
                B2 = "Import Unitypackage";
                B3 = "Update List";
                B4 = "Delete";
                B4C1 = "Warning";
                B4C2 = "You are trying to remove this altogether.";
                B4B1 = "OK";
                B4B2 = "Cancel";
                B5 = "Import VRM";
                B6 = "Get UniVRM";
                B7 = "Select";
                B8 = "Build";
                B9 = "";
                H1 = "Editor / Tools";
                H2 = "Uploadable avatars";
                H3 = "";
                H4 = "";
                H5 = "";
                SC1 = "";
                SC2 = "";
                SC3 = "";
                GC1 = "";
                Tab0 = "Import / Settings";
                Tab1 = "Set Up";
                Tab2 = "Upload";
                break;
            case "日本語":
                Changelng = "言語: ";
                T1 = "設定";
                T2 = "インポート";
                T3 = "モデルの管理";
                T4 = "VRMのインポート(UniVRM)";
                C1 = "Prefab をインポートします。";
                C2 = "Unitypackage をインポートします。";
                C3 = "プロジェクト内のプレハブ";
                C4 = "バージョン0.xのVRMファイルをインポートします。";
                C4no = "UnityでVRMファイルを使用するためには、\nUniVRMパッケージをインポートする必要があります。";
                C5 = "アップロード・編集するアバターを選択";
                C5no = "アップロードできるインスタンスがありません。\nアバターをインポートして、「更新する」を押してください。";
                C6 = "選択中のプレハブ: ";
                C6no = "選択中のプレハブはありません。";
                C7 = "アップロード可能なアバターがありません。\nVRMアバターを使用したい場合は、VRChat用に変換する必要があります。";
                C8 = "";
                C9 = "";
                C10 = "";
                C11 = "";
                C12 = "";
                C13 = "";
                C14 = "";
                C15 = "";
                C16 = "";
                C17 = "";
                C18 = "";
                C19 = "";
                C20 = "";
                C21 = "";
                C22 = "";
                C23 = "";
                C24 = "";
                C25 = "";
                C26 = "";
                C27 = "";
                B1 = "Prefabを開く";
                B2 = "unitypackageを開く";
                B3 = "更新する";
                B4 = "削除";
                B4C1 = "警告";
                B4C2 = "をプロジェクトから完全に削除しますか？";
                B4B1 = "削除";
                B4B2 = "キャンセル";
                B5 = "VRMをインポート";
                B6 = "UniVRMを入手する";
                B7 = "選択";
                B8 = "ビルド";
                B9 = "";
                H1 = "編集ツール";
                H2 = "アップロード可能なアバター";
                H3 = "";
                H4 = "";
                H5 = "";
                SC1 = "";
                SC2 = "";
                SC3 = "";
                GC1 = "";
                Tab0 = "インポート / 設定";
                Tab1 = "セットアップ";
                Tab2 = "アップロード";
                break;
        }
    }

}

public static class LanguageUtility
{
    public static string GetCurrentLanguage()
    {
        string settingsPath = @"Packages\com.sabuworks.eauploader\settings.json";
        if (File.Exists(settingsPath))
        {
            string jsonContent = File.ReadAllText(settingsPath);
            var settings = JsonUtility.FromJson<LanguageSetting>(jsonContent);
            return settings.language;
        }
        return "English"; // default language
    }
}

[System.Serializable]
public class LanguageSetting
{
    public string language;
}