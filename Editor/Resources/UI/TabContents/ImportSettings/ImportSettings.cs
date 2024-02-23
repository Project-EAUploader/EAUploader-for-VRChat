using EAUploader.UI.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.ImportSettings
{
    internal class Main
    {
        private static EALibrary EALibrary = new EALibrary();
        private static IMGUIContainer iMGUIContainer;
        private static VisualElement root;
        private static float libraryWidth;
        private static float libraryHeight;

        public static void ShowContent(VisualElement rootContainer)
        {
            Main.root = rootContainer;
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/ImportSettings/ImportSettings");
            visualTree.CloneTree(root);

            Import.ShowContent(root.Q("import_settings"));
            ManageModels.ShowContent(root.Q("manage_models"));


            ShowEALibrary();
        }

        private static void ShowEALibrary()
        {
            var libraryContainer = root.Q<VisualElement>("library_container");
            ClearLibrary();

            var library = new VisualElement()
            {
                name = "library",
                style = { flexGrow = 1 }
            };

            libraryContainer.Add(library);

            // Subscribe to the GeometryChangeEvent with a local method
            library.RegisterCallback<GeometryChangedEvent>(OnPreviewGeometryChanged);

            void OnPreviewGeometryChanged(GeometryChangedEvent evt)
            {
                library.UnregisterCallback<GeometryChangedEvent>(OnPreviewGeometryChanged);
                libraryWidth = evt.newRect.width;
                libraryHeight = evt.newRect.height;
                CreateIMGUIContainer();
            }

            library.RegisterCallback<GeometryChangedEvent>((evt) =>
            {
                libraryWidth = evt.newRect.width;
                libraryHeight = evt.newRect.height;
            });
        }

        private static void CreateIMGUIContainer()
        {
            iMGUIContainer = new IMGUIContainer(() =>
            {
                var libraryRectArea = new Rect(0, 0, libraryWidth, libraryHeight);
                GUILayout.BeginArea(libraryRectArea);
                {
                    EALibrary.Draw(libraryRectArea);
                }
                GUILayout.EndArea();
            });

            iMGUIContainer.style.width = libraryWidth;
            iMGUIContainer.style.height = libraryHeight;

            var library = root.Q("library");
            library.Add(iMGUIContainer);
        }

        private static void ClearLibrary()
        {
            iMGUIContainer = null;
        }
    }
}
