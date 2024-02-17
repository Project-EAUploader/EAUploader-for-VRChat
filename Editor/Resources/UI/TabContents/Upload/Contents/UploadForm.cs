using EAUploader.CustomPrefabUtility;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase.Editor;

namespace EAUploader.UI.Upload
{
    internal class UploadForm
    {
        public static VisualElement root;
        public static Components.Preview preview;

        public static void ShowContent(VisualElement rootContainer)
        {
            root = rootContainer;
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Upload/Contents/UploadForm");
            visualTree.CloneTree(root);
        }
    }
}