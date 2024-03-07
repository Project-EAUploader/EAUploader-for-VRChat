using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.ImportSettings
{
    internal class Main
    {
        private static VisualElement root;

        public static void ShowContent(VisualElement rootContainer)
        {
            Main.root = rootContainer;
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/ImportSettings/ImportSettings");
            visualTree.CloneTree(root);

            Import.ShowContent(root.Q("import_settings"));
            ManageModels.ShowContent(root.Q("manage_models"));
            EALibrary.ShowContent(root.Q("library_container"));
        }
    }
}
