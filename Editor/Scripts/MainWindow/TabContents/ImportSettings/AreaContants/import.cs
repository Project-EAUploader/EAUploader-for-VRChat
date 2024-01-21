#if !EA_ONBUILD
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using static labels;
using static styles;
using static Texture;

public class Import
{
    private static string lastImportedFile;
    public static bool isVRMAvailable = EAUploader.HasVRM;

    public static void Draw(Rect position)
    {
        GUILayout.BeginArea(position);
        GUILayout.Label(Get(102), h1LabelStyle);

        if (GUILayout.Button(Getc("help", 500), HelpButtonStyle))
        {
            EAUploaderMessageWindow.ShowMsg(100);
        }
        GUILayout.Label(Get(105), h2LabelStyle);
        if (GUILayout.Button(Getc("import", 130), MainButtonStyle))
        {
            string filePath = EditorUtility.OpenFilePanelWithFilters(
                Get(200),
                "",
                new string[] { Get(201), "prefab,unitypackage", "All files", "*" }
            );
            if (!string.IsNullOrEmpty(filePath))
            {
                ImportSingleAsset(filePath);
            }
        }
        // フォルダ選択用
        if (GUILayout.Button(Getc("import", 1301), SubButtonStyle))
        {
            string folderPath = EditorUtility.OpenFolderPanel( Get(202), "", "");
            if (!string.IsNullOrEmpty(folderPath))
            {
                ImportAllAssetsFromFolder(folderPath);
            }
        }

        static void ImportAllAssetsFromFolder(string folderPath)
        {
            var allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                                    .Where(s => s.EndsWith(".prefab") || s.EndsWith(".unitypackage"))
                                    .ToList();

            if (allFiles.Count == 0)
            {
                EditorUtility.DisplayDialog(Get(306), Get(307), Get(136));
                return;
            }
            // ファイルの一覧を文字列に変換
            string fileList = string.Join("\n", allFiles.Select(Path.GetFileName));
            bool confirmImport = EditorUtility.DisplayDialog(Get(308), $""+Get(309) + "\n{fileList}", Get(136), Get(137));

            if (!confirmImport)
            {
                return;
            }

            bool applyToAll = false;
            string applyToAllChoice = "";

            foreach (var file in allFiles)
            {
                string fileExtension = Path.GetExtension(file).ToLower();
                string fileName = Path.GetFileName(file);

                if (!applyToAll && AssetDatabase.LoadAssetAtPath<GameObject>(fileName) != null)
                {
                    int choice = EditorUtility.DisplayDialogComplex(Get(300),
                        $"{fileName}" + Get(301),
                        Get(302),
                        Get(303),
                        Get(304));

                    switch (choice)
                    {
                        case 0:
                            break;
                        case 1:
                            continue;
                        case 2:
                            applyToAll = true;
                            applyToAllChoice = Get(305);
                            break;
                    }
                }

                if (applyToAll && applyToAllChoice == Get(304))
                {
                    continue;
                }

                switch (fileExtension)
                {
                    case ".prefab":
                        AssetDatabase.ImportAsset(file);
                        break;
                    case ".unitypackage":
                        AssetDatabase.ImportPackage(file, true);
                        break;
                }
            }
            AssetDatabase.Refresh();
        }

        static void ImportSingleAsset(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath).ToLower();
            string fileName = Path.GetFileName(filePath);

            if (AssetDatabase.LoadAssetAtPath<GameObject>(fileName) != null)
            {
                bool overwrite = EditorUtility.DisplayDialog(Get(300),
                    $"{fileName}" + Get(301),
                    Get(305),
                    Get(303));

                if (!overwrite)
                {
                    return;
                }
            }

            switch (fileExtension)
            {
                case ".prefab":
                    AssetDatabase.ImportAsset(filePath, ImportAssetOptions.Default);
                    break;
                case ".unitypackage":
                    AssetDatabase.ImportPackage(filePath, true);
                    break;
            }
            AssetDatabase.Refresh();   
        }

        if (isVRMAvailable == true)
        {
            GUILayout.Label(Get(104), h1LabelStyle);
            if (GUILayout.Button(Getc("help", 500), HelpButtonStyle))
            {
                EAUploaderMessageWindow.ShowMsg(101);
            }
            GUILayout.Label(Get(108), h2LabelStyle);
            if (GUILayout.Button(Get(104), MainButtonStyle))
            {
                EditorApplication.ExecuteMenuItem("VRM0/Import from VRM 0.x");
            }
        }
        else
        {
            GUILayout.Label(Get(109), h2LabelStyle);
            if(GUILayout.Button(Get(139), MainButtonStyle))
            {
                Application.OpenURL("https://github.com/vrm-c/UniVRM/releases");
            }
        }

        DrawHorizontalDottedCenterLine(Color.black, 12, position.width);

        GUILayout.Label(Get(142), h2LabelStyle);
        if(GUILayout.Button(Getc("travel_explor", 143), MainButtonStyle))
        {
            EAUploader.ChangeTab(3);
        }

        GUILayout.EndArea();
    }
}
#endif