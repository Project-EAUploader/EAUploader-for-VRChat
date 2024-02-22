using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.ImportSettings
{
    internal class Main
    {
        public static void ShowContent(VisualElement root)
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/ImportSettings/ImportSettings");
            visualTree.CloneTree(root);

            Import.ShowContent(root.Q("import_settings"));
            ManageModels.ShowContent(root.Q("manage_models")); 
        }
    }
}
