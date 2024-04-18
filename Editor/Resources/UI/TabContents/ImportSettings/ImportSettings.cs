using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.ImportSettings
{
    internal class Main
    {
        private static VisualElement root;
        internal static bool isLibraryOpen = true;

        public static void ShowContent(VisualElement rootContainer)
        {
            Main.root = rootContainer;
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/ImportSettings/ImportSettings");
            visualTree.CloneTree(root);

            var settings = Config.LoadSettings();
            isLibraryOpen = settings.isLibraryOpen;

            Import.ShowContent(root.Q("import_settings"));
            ManageModels.ShowContent(root.Q("manage_models"));
            EALibrary.ShowContent(root.Q("library_container"));

            root.Q("library_container").style.display = isLibraryOpen ? DisplayStyle.Flex : DisplayStyle.None;
        }

        internal static void ToggleLibrary()
        {
            isLibraryOpen = !isLibraryOpen;
            root.Q("library_container").style.display = isLibraryOpen ? DisplayStyle.Flex : DisplayStyle.None;

            var settings = Config.LoadSettings();
            settings.isLibraryOpen = isLibraryOpen;
            Config.SaveSettings(settings);

            var manageModelsContainer = root.Q("manage_models");
            manageModelsContainer.Clear();

            ManageModels.ShowContent(manageModelsContainer);
        }
    }
}