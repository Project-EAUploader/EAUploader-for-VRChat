using UnityEngine;
using UnityEditor;
using static styles;
using static labels;

// 参考用保存

/*
internal static class AvatarInfoWindowUtility
{
    private static GameObject selectedPrefabInstance; // クラスの中で使用する変数
    private static Vector2 scrollPostion = Vector2.zero;

    public static void AvatarInfoWindow(GameObject selectedPrefab)
    {
        selectedPrefabInstance = selectedPrefab;

        if (selectedPrefabInstance == null)
        {
            GUILayout.Label(labels.C5, styles.h2Style);
        }
        else
        {
            VRMMeta vrmmeta;
            if (selectedPrefabInstance.TryGetComponent<VRMMeta>(out vrmmeta))
            {
                scrollPostion = EditorGUILayout.BeginScrollView(scrollPostion);
                var h4 = styles.h4Style;
                var h5 = styles.h5Style;
                EditorGUILayout.LabelField(labels.C12 + vrmmeta.Meta.Title, styles.h2Style);
                EditorGUILayout.LabelField(labels.C13 + vrmmeta.Meta.Version, styles.h2Style);
                EditorGUILayout.LabelField(labels.C14 + vrmmeta.Meta.Author, styles.h2Style);
                EditorGUILayout.LabelField(labels.C15 + vrmmeta.Meta.ContactInformation, h4);
                EditorGUILayout.LabelField(labels.C16 + vrmmeta.Meta.Reference, h4);

                DisplayWithColorBasedOnDisallow(labels.C17 + vrmmeta.Meta.AllowedUser, h5);
                DisplayWithColorBasedOnDisallow(labels.C18 + vrmmeta.Meta.ViolentUssage, h5);
                DisplayWithColorBasedOnDisallow(labels.C19 + vrmmeta.Meta.SexualUssage, h5);
                DisplayWithColorBasedOnDisallow(labels.C20 + vrmmeta.Meta.CommercialUssage, h5);
                EditorGUILayout.LabelField(labels.C21 + vrmmeta.Meta.OtherPermissionUrl, h5);

                string distributionLicense = labels.C22 + vrmmeta.Meta.LicenseType;
                if (!string.IsNullOrEmpty(vrmmeta.Meta.OtherLicenseUrl))
                {
                    distributionLicense += labels.C23 + vrmmeta.Meta.OtherLicenseUrl;
                }
                EditorGUILayout.LabelField(distributionLicense, styles.h5Style);
                EditorGUILayout.EndScrollView();
                if (GUILayout.Button("?", styles.HelpButtonStyle))
                {
                    OverlayLabelUtility.ShowOverlayLabel(5); //help5
                }
                if (GUILayout.Button(labels.C24, styles.MainButtonStyle))
                {
                    Selection.activeGameObject = selectedPrefabInstance;
                    EditorApplication.ExecuteMenuItem("VRM0/Duplicate and Convert for VRChat" ); // メニュー呼び出せるらしい。もっと早く知りたかった。
                    ConverterHelp.Open();
                }
            }
            else
            {
                GUILayout.Label(labels.C6, styles.h2Style);
            }
        }
    }

    private static void DisplayWithColorBasedOnDisallow(string content, GUIStyle style)
    {
        Color originalColor = EditorGUIUtility.isProSkin ? Color.white : Color.black; // Check for Unity Pro skin
        if (content.Contains("Disallow"))
        {
            EditorGUIUtility.labelWidth = 400;
            EditorGUI.LabelField(GUILayoutUtility.GetRect(GUIContent.none, style), new GUIContent(content), new GUIStyle(style) { normal = new GUIStyleState() { textColor = Color.red } });
        }
        else
        {
            EditorGUILayout.LabelField(content, style);
        }
        EditorGUIUtility.labelWidth = 0;
    }
}
*/