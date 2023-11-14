using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;

namespace EAUploaderEditors
{
    public static class EAUploaderEditorManager
    {
        [System.Serializable]
        public class EditorInfo
        {
            public string directory;
            public string display_name;
            public string description;
            public string author;
            public string version;
            public string url;
        }

        public static List<EditorInfo> LoadEditorInfos()
        {
            // 拡張エディタの登録方式はAPIを提供することで変更予定
            string path = @"Packages\com.sabuworks.eauploader\Editor\Scripts\Window\Editors\manage.json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var editorsWrapper = JsonUtility.FromJson<EditorsWrapper>(json);
                
                return editorsWrapper?.editors ?? new List<EditorInfo>();
            }
            else
            {
                return new List<EditorInfo>();
            }
        }

        private class EditorsWrapper
        {
            public List<EditorInfo> editors;
        }

        public static void OpenEditor(string directory)
        {
            // Assume the class is in the namespace "EAUploaderEditors"
            string editorPath = "EAUploaderEditors." + directory;

            // Load the assembly that contains the editor
            Assembly assembly = Assembly.GetExecutingAssembly();
            
            // Get the type of the editor class
            System.Type type = assembly.GetType(editorPath);
            
            // Check if the type was found
            if (type != null)
            {
                // Try to get the "Open" method from the type
                MethodInfo method = type.GetMethod("Open", BindingFlags.Static | BindingFlags.Public);
                
                // Check if the method was found
                if (method != null)
                {
                    try
                    {
                        // Invoke the method
                        method.Invoke(null, null);
                    }
                    catch (Exception e)
                    {
                        // Log any exceptions that occurred during the invoke
                        Debug.LogError("Error invoking method: " + e.Message);
                    }
                }
                else
                {
                    // Log an error if the method was not found
                    Debug.LogError("No Open method found in " + directory);
                }
            }
            else
            {
                // Log an error if the type was not found
                Debug.LogError("Editor class " + directory + " not found!");
            }
        }
    }
}
