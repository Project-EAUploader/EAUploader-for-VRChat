using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.Windows
{
    public class Logger : EditorWindow
    {
        private static StringBuilder _stringBuilder;
        internal static string LOGFOLDER_PATH = "EAUploaderLog/";
        internal static long LOGFOLDER_MAX_SIZE_IN_BYTES = 100 * 1024 * 1024; // 100MB

        internal static IEnumerable<(string package, string version)> VpmLockedPackages()
        {
            try
            {
                var vpmManifestJson = File.ReadAllText("Packages/vpm-manifest.json");
                var manifest = JsonConvert.DeserializeObject<VpmManifest>(vpmManifestJson)
                               ?? throw new InvalidOperationException();
                return manifest.locked
                    .Where(x => x.Value.version != null)
                    .Select(x => (x.Key, x.Value.version!));
            }
            catch
            {
                return Array.Empty<(string, string)>();
            }
        }


        [Serializable]
        public class DependencyInfo
        {
            public string version;
            public Dictionary<string, string> dependencies;
        }

        [Serializable]
        public class VpmManifest
        {
            public Dictionary<string, DependencyInfo> dependencies;
            public Dictionary<string, DependencyInfo> locked;
        }

        [MenuItem("Window/Error Report")]
        public static void MakeError()
        {
            Debug.LogError("This is a test error message");
        }

        internal void OnEnable()
        {
            _stringBuilder = new StringBuilder();
        }


        public static string GetLogFolderFullPath()
        {
            return new DirectoryInfo(LOGFOLDER_PATH).FullName;
        }

        internal static void OnReceiveLog(string logText, string stackTrace, LogType logType)
        {
            // ログ出力処理
            // ログフォルダの容量がLOGFOLDER_MAX_SIZE_IN_BYTESを超えていた場合
            // 最も古いログファイルを削除する
            // ディレクトリ容量の対象となるのは*.logファイルのみである。
            DirectoryInfo di = new DirectoryInfo(LOGFOLDER_PATH);


            if (!di.Exists)
            {
                Directory.CreateDirectory(LOGFOLDER_PATH);
                // 確実にディレクトリが作成されるのを待つ
                while(!di.Exists)
                {
                    di.Refresh();
                }
            }

            // ディレクトリ容量取得
            long logFolderSizeInBytes = di.EnumerateFiles("*.log").Sum(fi => fi.Length);

            // ディレクトリ容量が規定の容量よりも大きかった場合
            if (logFolderSizeInBytes > LOGFOLDER_MAX_SIZE_IN_BYTES)
            {

                var oldestLogFile = di.EnumerateFiles("*.log")
                                      .OrderBy(fi => fi.CreationTime)
                                      .FirstOrDefault();

                if (oldestLogFile != null)
                {
                    oldestLogFile.Delete();
                }
            }

            // ログ出力
            writeLog(logText, stackTrace, logType);

            if (logType == LogType.Exception || logType == LogType.Error)
            {
                var eauWindow = GetWindow<UI.EAUploader>(null, focus: false);
                Logger wnd = GetWindow<Logger>();
                wnd.titleContent = new GUIContent(T7e.Get("Error Report"));
                wnd.position = new Rect(eauWindow.position.x + eauWindow.position.width / 2 - 400, eauWindow.position.y + eauWindow.position.height / 2 - 300, 800, 600);
                wnd.minSize = new Vector2(800, 600);

                wnd.rootVisualElement.styleSheets.Add(EAUploader.styles);
                wnd.rootVisualElement.styleSheets.Add(EAUploader.tailwind);

                wnd.rootVisualElement.Clear();
                var visualTree = Resources.Load<VisualTreeAsset>("UI/Windows/Logger");
                visualTree.CloneTree(wnd.rootVisualElement);

                LanguageUtility.Localization(wnd.rootVisualElement);

                StringBuilder errorReport = new();

                _stringBuilder.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                _stringBuilder.AppendLine(logText);
                _stringBuilder.AppendLine("Stack Trace:");
                _stringBuilder.AppendLine(stackTrace);

                errorReport.AppendLine("Error Report");
                errorReport.AppendLine("------------");
                errorReport.AppendLine();
                errorReport.AppendLine("Error Logs:");
                errorReport.AppendLine();
                errorReport.AppendLine(_stringBuilder.ToString());
                errorReport.AppendLine("Environment Details:");
                errorReport.AppendLine("- Application-Version: " + EAUploaderCore.GetVersion(true));
                errorReport.AppendLine("- Unity-Version: " + Application.unityVersion);
                errorReport.AppendLine("- Editor-Platform: " + Application.platform);
                errorReport.AppendLine("- Vpm-Dependency: \n" + String.Join("\n", VpmLockedPackages().Select(x => $"{x.package}@{x.version}")));

                wnd.rootVisualElement.Q<Label>("message").text = errorReport.ToString();

                var copyButton = wnd.rootVisualElement.Q<Button>("copy");
                var okButton = wnd.rootVisualElement.Q<Button>("ok");
                var restartButton = wnd.rootVisualElement.Q<Button>("restart");

                copyButton.clickable.clicked += () =>
                {
                    EditorGUIUtility.systemCopyBuffer = errorReport.ToString();
                };

                okButton.clickable.clicked += () =>
                {
                    wnd.Close();
                };

                restartButton.clickable.clicked += () =>
                {
                    AssetDatabase.ImportAsset("Packages/tech.uslog.eauploader", ImportAssetOptions.ImportRecursive);
                };
            }

        }

        internal static void writeLog(string logText, string stackTrace, LogType logType)
        {
            // ログレベルがErrorかExceptionのときは、フルのトレースログを出力する
            // それ以外のログレベルでは、ログの呼び出し箇所のみを表示する
            // stackTraceのパース

            string outputStackTrace = "";
            string outputTimeStamp = "";

            // ログファイルに出力するログレベル
            // LOG:Log Level
            // WNG:Warning Level
            // ERR:Error Level
            // EXP:Exception Level
            // AST:Assert Level
            string outputLogLevel = "";

            outputTimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            switch (logType)
            {
                case LogType.Error:
                    outputLogLevel = "ERR";
                    break;
                case LogType.Assert:
                    outputLogLevel = "AST";
                    break;
                case LogType.Warning:
                    outputLogLevel = "WNG";
                    break;
                case LogType.Log:
                    outputLogLevel = "LOG";
                    break;
                case LogType.Exception:
                    outputLogLevel = "EXP";
                    break;
            }

            if (logType == LogType.Exception || logType == LogType.Error)
            {
                // そのままトレースログを出力すると見づらいので
                // インデントを付ける
                string[] lines = stackTrace.Split('\n');
                for (int i = 1; i < lines.Length; i++)
                {
                    lines[i] = new string(' ', $"{outputTimeStamp} {outputLogLevel} {logText} ".Length) + lines[i];
                }
                // インデントを追加したフルのトレースログを出力
                outputStackTrace = string.Join('\n',lines);
            }
            else
            {
                string[] lines = stackTrace.Split('\n');
                if (lines.Length >= 2)
                {
                    // 二行目のみをトレースログとして出力する
                    outputStackTrace = lines[1];
                }
                else
                {
                    outputStackTrace = stackTrace;
                }
            }

            var logOutputFileName = DateTime.Now.ToString("yyyy-MM-dd") + ".log";

            // ファイルが存在しなければ作成する
            if (!File.Exists(LOGFOLDER_PATH + logOutputFileName))
            {
                File.Create(LOGFOLDER_PATH + logOutputFileName).Close();
            }
            using (var writer = new StreamWriter(LOGFOLDER_PATH + logOutputFileName, true, Encoding.GetEncoding("UTF-8")))
            {
                writer.WriteLine($"{outputTimeStamp} {outputLogLevel} {logText} {outputStackTrace}");
            }
        }

    }
}