using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using static ShaderEditorlabels;
using static styles;

namespace EAUploaderEditors
{
    public class ShaderEditor : EditorWindow
    {
        private Shader newShader;
        private List<Material> materials = new List<Material>();
        private int selectedShaderIndex = 0;
        private bool shaderChanged = false;

        public static void Open()
        {
            ShaderEditorlabels.UpdateLanguage();
            ShaderEditor window = (ShaderEditor)EditorWindow.GetWindow(typeof(ShaderEditor), false, Windowname);
            window.Show();
            window.maximized = true; // Automatically maximize the window when opened
        }

        private void OnEnable()
        {
            if (CustomPrefabUtility.selectedPrefabInstance != null)
            {
                Renderer[] renderers = CustomPrefabUtility.selectedPrefabInstance.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    foreach (Material mat in renderer.sharedMaterials)
                    {
                        if (mat != null && !materials.Contains(mat) && !mat.name.Contains("EyeIris"))
                        {
                            materials.Add(mat);
                        }
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (CustomPrefabUtility.selectedPrefabInstance == null)
            {
                Close();
            }

            // Set background color to white
            GUI.backgroundColor = Color.white;
            GUI.Box(new Rect(0, 0, position.width, position.height), "");

            float totalWidth = this.position.width;
            float totalHeight = this.position.height;

            // Vertical split: 9:1
            float upperPartHeight = totalHeight * 0.9f;
            float lowerPartHeight = totalHeight * 0.1f;

            // Horizontal split for the upper part: 5:3:2
            float leftWidth = totalWidth * 0.5f;
            float middleWidth = totalWidth * 0.3f;
            float rightWidth = totalWidth * 0.2f;

            // Draw the preview in the left-most area
            Rect previewRect = new Rect(0, 0, leftWidth, upperPartHeight);
            CustomPrefabUtility.DrawPrefabPreview(previewRect);
            GUILayout.Label(Windowname, h1Style);

            // Middle section for shader dropdown
            GUILayout.BeginArea(new Rect(leftWidth, 0, middleWidth, upperPartHeight));
            GUILayout.Label("Edit Shader", EditorStyles.boldLabel);
            string[] shaderGuids = AssetDatabase.FindAssets("t:Shader");
            List<string> shaderOptions = new List<string>();
            foreach (var guid in shaderGuids)
            {
                string shaderPath = AssetDatabase.GUIDToAssetPath(guid);
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
                shaderOptions.Add(shader.name);
            }

            // Add a dropdown for bulk change at the beginning of the list
            int prevSelectedShaderIndex = selectedShaderIndex;
            selectedShaderIndex = EditorGUILayout.Popup("Change all materials", selectedShaderIndex, shaderOptions.ToArray());
            if (GUILayout.Button("Apply Shader to All"))
            {
                Shader selectedShader = Shader.Find(shaderOptions[selectedShaderIndex]);
                foreach (var mat in materials)
                {
                    mat.shader = selectedShader;
                }
                shaderChanged = true;
            }

            foreach (var mat in materials)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Box(AssetPreview.GetAssetPreview(mat), GUILayout.Width(40), GUILayout.Height(40));
                EditorGUILayout.LabelField(mat.name, EditorStyles.boldLabel);
                string currentShaderName = mat.shader.name;
                int currentIndex = shaderOptions.IndexOf(currentShaderName);
                int newIndex = EditorGUILayout.Popup("Shader", currentIndex, shaderOptions.ToArray());
                if (newIndex != currentIndex)
                {
                    mat.shader = Shader.Find(shaderOptions[newIndex]);
                    shaderChanged = true;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (shaderChanged)
            {
                Repaint();
                shaderChanged = false;
            }

            GUILayout.EndArea();

            // Right section (for future use)
            GUILayout.BeginArea(new Rect(leftWidth + middleWidth, 0, rightWidth, upperPartHeight));
            // Set the background color to transparent
            GUI.backgroundColor = Color.clear;

            // Create a GUIStyle for centered text
            GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
            centeredStyle.alignment = TextAnchor.MiddleCenter;

            // Get the size of the text
            Vector2 textSize = centeredStyle.CalcSize(new GUIContent("Guide"));

            // Draw the text in the center of the area
            GUI.Label(new Rect((rightWidth - textSize.x) / 2, (upperPartHeight - textSize.y) / 2, textSize.x, textSize.y), "Guide", centeredStyle);

            GUILayout.EndArea();

            // Positioning the Close button at the bottom
            GUILayout.BeginArea(new Rect(0, upperPartHeight, totalWidth, lowerPartHeight));
            if (GUILayout.Button(CloseButtonLabel, MainButtonStyle))
            {
                Close();
            }
            GUILayout.EndArea();
        }
    }
}
