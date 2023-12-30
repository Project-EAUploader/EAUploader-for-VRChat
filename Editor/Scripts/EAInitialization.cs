using UnityEditor;
using static EAUploader;
using static CustomPrefabUtility;
using static SpriteImportProcessor;
using static EAUploaderEditorManager;
using static ShaderChecker;

public class CombinedInitialization
{
    [InitializeOnLoadMethod]
    private static void CombinedOnLoad()
    {
        EditorUtility.DisplayProgressBar("Initialization", "Initializing CustomPrefabUtility...", 0.0f);
        CustomPrefabUtilityOnUnityLoad();
        EditorUtility.DisplayProgressBar("Initialization", "Initializing EAUploaderEditorManager...", 0.2f);
        EAUploaderEditorManagerOnLoad();
        EditorUtility.DisplayProgressBar("Initialization", "Initializing SpriteImportProcessor...", 0.4f);
        SpriteImportProcessorOnEditorLoad();
        EditorUtility.DisplayProgressBar("Initialization", "Initializing EAUploader...", 0.6f);
        EAUploaderInitializeOnLoad();
        EditorUtility.DisplayProgressBar("Initialization", "Initializing ShaderChecker...", 0.8f);
        ShaderCheckerOnLoad();
        
        UpdateManager.ShowWindow();

        EditorUtility.ClearProgressBar();
    }

    private static void CustomPrefabUtilityOnUnityLoad()
    {
        OnCustomPrefabUtility();
    }

    private static void SpriteImportProcessorOnEditorLoad()
    {
        OnEditorLoad();
    }

    private static void EAUploaderInitializeOnLoad()
    {
        OnEAUploader();
    }

    private static void EAUploaderEditorManagerOnLoad()
    {
        OnEditorManagerLoad();
    }

    private static void ShaderCheckerOnLoad()
    {
        OnShaderChecker();
    }
}
