using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using EAUploader.CustomPrefabUtility;
using System;

namespace EAUploader.UI.Setup
{
    public class ViewpointPositionEditor : EditorWindow
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            var editorRegistration = new EditorRegistration
            {
                MenuName = "EAUploader/ViewpointPositionEditor",
                EditorName = "Viewpoint Position Editor",
                Description = "A powerful viewpoint position editor for Unity.",
                Version = "0.0.1",
                Author = "USLOG",
                Url = "https://uslog.tech/eauploader",
                Requirement = (string prefabPath) =>
                {
                    GameObject gameObject = PrefabManager.GetPrefab(prefabPath);
                    return Utility.CheckAvatarHasVRCAvatarDescriptor(gameObject);
                },
                RequirementDescription = "The selected model must have a VRCAvatarDescriptor component.",
            };

            EAUploaderEditorManager.RegisterEditor(editorRegistration);
        }

        [MenuItem("EAUploader/ViewpointPositionEditor")]
        public static void Popup()
        {
            ViewpointPositionEditor editor = GetWindow<ViewpointPositionEditor>();
            editor.titleContent = new GUIContent("Viewpoint Position Editor");
        }

        private PreviewRenderUtility _previewUtility;
        private PreviewImage _image;
        public static Bounds bounds;
        public static GameObject gameObject;
        public static GameObject newPoint;
        private GameObject _oldPoint;

        public void OnEnable()
        {
            gameObject = PrefabManager.GetPrefab(EAUploaderCore.selectedPrefabPath);

            if (gameObject is null)
            {
                return;
            }

            var avatarDescriptor = Utility.GetAvatarDescriptor(gameObject);

            bounds = CalculateBounds(gameObject);

            _previewUtility ??= new PreviewRenderUtility();
            _previewUtility.camera.backgroundColor = new UnityEngine.Color(0.9f, 0.9f, 0.9f, 1);
            _previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
            _previewUtility.camera.orthographic = true;
            _previewUtility.camera.orthographicSize = bounds.size.y / 2;

            float size = bounds.size.magnitude;
            float distance = size / (4 * Mathf.Tan(_previewUtility.camera.fieldOfView * 0.5f * Mathf.Deg2Rad));
            _previewUtility.camera.transform.position = bounds.center + _previewUtility.camera.transform.right * distance;
            _previewUtility.camera.transform.LookAt(bounds.center);

            GameObject prefab = _previewUtility.InstantiatePrefabInScene(gameObject);
            _previewUtility.AddSingleGO(prefab);

            _oldPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _oldPoint.transform.position = avatarDescriptor.ViewPosition;
            _oldPoint.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            var oldPointMaterial = new Material(Resources.Load<Shader>("Shaders/AlwaysShow"));
            oldPointMaterial.color = Color.green;
            _oldPoint.GetComponent<Renderer>().material = oldPointMaterial;
            _previewUtility.AddSingleGO(_oldPoint);


            newPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            newPoint.transform.position = avatarDescriptor.ViewPosition;
            newPoint.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            var newPointMaterial = new Material(Resources.Load<Shader>("Shaders/AlwaysShow"));
            newPointMaterial.color = Color.red;
            newPoint.GetComponent<Renderer>().material = newPointMaterial;

            _previewUtility.AddSingleGO(newPoint);
        }

        private static Bounds CalculateBounds(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            var bounds = new Bounds(obj.transform.position, Vector3.zero);
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

        public void OnDisable()
        {
            if (_previewUtility is null)
            {
                return;
            }
            _previewUtility.Cleanup();
            _previewUtility = null;
        }

        private void CreateGUI()
        {
            rootVisualElement.styleSheets.Add(EAUploader.styles);
            rootVisualElement.styleSheets.Add(EAUploader.tailwind);

            if (gameObject is null)
            {
                rootVisualElement.Add(new Label(L10n.Tr("Select a model")));
                return;
            }

            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Setup/Editor/ViewpointPositionEditor/ViewpointPositionEditor");
            visualTree.CloneTree(rootVisualElement);

            _image = new PreviewImage(_previewUtility);
            rootVisualElement.Q<VisualElement>("preview_area").Add(_image);

            var avatarDescriptor = Utility.GetAvatarDescriptor(gameObject);

            var horizontalSlider = rootVisualElement.Q<Slider>("horizontal_slider");
            var tail_z = bounds.center.z - bounds.size.z / 2;
            var head_z = bounds.center.z + bounds.size.z / 2;
            horizontalSlider.lowValue = tail_z;
            horizontalSlider.highValue = head_z;
            horizontalSlider.value = avatarDescriptor.ViewPosition.z;

            horizontalSlider.RegisterValueChangedCallback(evt =>
            {
                newPoint.transform.position = new Vector3(newPoint.transform.position.x, newPoint.transform.position.y, evt.newValue);
                _image.UpdateView();
            });

            var verticalSlider = rootVisualElement.Q<Slider>("vertical_slider");
            var tail_y = bounds.center.y - bounds.size.y / 2;
            var head_y = bounds.center.y + bounds.size.y / 2;
            verticalSlider.lowValue = tail_y;
            verticalSlider.highValue = head_y;
            verticalSlider.value = avatarDescriptor.ViewPosition.y;

            verticalSlider.RegisterValueChangedCallback(evt =>
            {
                newPoint.transform.position = new Vector3(newPoint.transform.position.x, evt.newValue, newPoint.transform.position.z);
                _image.UpdateView();
            });

            rootVisualElement.Q<Button>("invert_view").clicked += () =>
            {
                horizontalSlider.inverted = !horizontalSlider.inverted;
                _image.InvertView();
            };

            rootVisualElement.Q<Button>("apply_button").clicked += () =>
            {
                avatarDescriptor.ViewPosition = newPoint.transform.position;
            };

            rootVisualElement.Q<Button>("check_button").clicked += () =>
            {
                ViewpointPositionTester.ShowWindow();
            };
        }
    }

    public class PreviewImage : Image
    {
        private const int ImageSize = 1024;

        private static readonly Rect ImageDimension = new(0, 0, ImageSize, ImageSize);

        private readonly PreviewRenderUtility _previewUtility;

        public PreviewImage(PreviewRenderUtility previewUtility)
        {
            style.flexGrow = 1;
            _previewUtility = previewUtility;

            _previewUtility.BeginPreview(ImageDimension, GUIStyle.none);
            _previewUtility.camera.Render();
            image = _previewUtility.EndPreview();
        }

        internal void InvertView()
        {
            float size = ViewpointPositionEditor.bounds.size.magnitude;
            float distance = size / (4 * Mathf.Tan(_previewUtility.camera.fieldOfView * 0.5f * Mathf.Deg2Rad));
            _previewUtility.camera.transform.position = ViewpointPositionEditor.bounds.center + _previewUtility.camera.transform.forward * distance;
            _previewUtility.camera.transform.LookAt(ViewpointPositionEditor.bounds.center);

            _previewUtility.BeginPreview(ImageDimension, GUIStyle.none);
            _previewUtility.camera.Render();
            _previewUtility.EndAndDrawPreview(ImageDimension);
            MarkDirtyRepaint();
        }

        public void UpdateView()
        {
            _previewUtility.BeginPreview(ImageDimension, GUIStyle.none);
            _previewUtility.camera.Render();
            image = _previewUtility.EndPreview();
        }
    }

    internal class ViewpointPositionTester : EditorWindow
    {
        public static void ShowWindow()
        {
            ViewpointPositionTester wnd = GetWindow<ViewpointPositionTester>();
            wnd.titleContent = new GUIContent("Viewpoint Position Tester");
            wnd.ShowAuxWindow();
        }

        private PreviewRenderUtility _previewUtility;
        private PreviewImage _image;
        private GameObject _gameObject;

        public void OnEnable()
        {
            _gameObject = ViewpointPositionEditor.gameObject;
            _previewUtility ??= new PreviewRenderUtility();
            _previewUtility.camera.backgroundColor = new UnityEngine.Color(0.9f, 0.9f, 0.9f, 1);
            _previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
            _previewUtility.camera.nearClipPlane = 0.05f;
            _previewUtility.camera.fieldOfView = 60;
            _previewUtility.camera.transform.position = ViewpointPositionEditor.newPoint.transform.position;
            _previewUtility.camera.transform.LookAt(ViewpointPositionEditor.newPoint.transform.position + Vector3.forward);

            GameObject prefab = _previewUtility.InstantiatePrefabInScene(_gameObject);
            _previewUtility.AddSingleGO(prefab);
        }

        public void OnDisable()
        {
            if (_previewUtility is null)
            {
                return;
            }
            _previewUtility.Cleanup();
            _previewUtility = null;
        }

        private void CreateGUI()
        {
            _image = new PreviewImage(_previewUtility);
            rootVisualElement.Add(_image);
        }
    }
}