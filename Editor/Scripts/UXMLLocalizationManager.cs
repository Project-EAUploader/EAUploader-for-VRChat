using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class UXMLLocalizationManager
{
    public UXMLLocalizationManager()
    {
        Debug.Log("UXMLLocalizationManager constructor called");
    }

    public void UpdateUXMLFiles(List<string> uxmlFilePaths)
    {
        string currentLanguage = LanguageUtility.GetCurrentLanguage();
        Debug.Log($"Current Language: {currentLanguage}");

        foreach (string uxmlFilePath in uxmlFilePaths)
        {
            Debug.Log($"Processing UXML file: {uxmlFilePath}");

            string uxmlFileName = Path.GetFileNameWithoutExtension(uxmlFilePath);
            string searchPattern = $"{uxmlFileName}_{currentLanguage}.uxml";
            Debug.Log($"Search pattern for localized UXML file: {searchPattern}");

            string basePath = @"Packages\com.sabuworks.eauploader\Editor\Resources\Translation\EAUploader\VRCSDK";
            Debug.Log($"Searching for localized UXML files in: {basePath}");

            string[] foundFiles = Directory.GetFiles(basePath, searchPattern, SearchOption.AllDirectories);
            Debug.Log($"Number of matching files found: {foundFiles.Length}");

            if (foundFiles.Length > 0)
            {
                foreach (var foundFile in foundFiles)
                {
                    Debug.Log($"Found localized UXML file: {foundFile}");
                }

                string localizedContent = File.ReadAllText(foundFiles[0]);
                Debug.Log($"Updating UXML file: {uxmlFilePath} with content from: {foundFiles[0]}");
                File.WriteAllText(uxmlFilePath, localizedContent);
                Debug.Log($"UXML file updated successfully: {uxmlFilePath}");
            }
            else
            {
                Debug.LogError($"Localized UXML file not found for: {uxmlFilePath}");
            }
        }

        EditorApplication.ExecuteMenuItem("VRChat SDK/Reload SDK");
    }
}

