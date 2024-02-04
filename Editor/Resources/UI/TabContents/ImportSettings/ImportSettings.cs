using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader_beta
{
    public class ImportSettings
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
