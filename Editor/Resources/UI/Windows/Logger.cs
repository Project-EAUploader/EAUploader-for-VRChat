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


        internal static void OnReceiveLog(string logText, string stackTrace, LogType logType)
        {
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

                copyButton.clickable.clicked += () =>
                {
                    EditorGUIUtility.systemCopyBuffer = errorReport.ToString();
                };

                okButton.clickable.clicked += () =>
                {
                    wnd.Close();
                };
            }

        }

    }
}