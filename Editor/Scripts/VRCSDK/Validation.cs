using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Networking;
using VRC;
using VRC.Core;
using VRC.SDK3.Avatars;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.Validation;
using VRC.SDKBase.Validation;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;
using Object = UnityEngine.Object;
using VRCStation = VRC.SDK3.Avatars.Components.VRCStation;

namespace EAUploader.VRCSDK
{
    public class ValidateResult
    {
        public enum ValidateResultType
        {
            Success,
            Warning,
            Error,
            Info,
            Link,
        }
        public Object Target { get; private set; }
        public ValidateResultType ResultType { get; private set; }
        public string ResultMessage { get; private set; }
        public Action Open { get; private set; }
        public Action Fix { get; private set; }
        public string Link { get; private set; }

        public ValidateResult(Object target, ValidateResultType resultType, string resultMessage, Action open, Action fix, string link = null)
        {
            Target = target;
            ResultType = resultType;
            ResultMessage = resultMessage;
            Open = open;
            Fix = fix;
            Link = link;
        }
    }

    public class Validation
    {
        private static bool ShowAvatarPerformanceDetails
        {
            get => EditorPrefs.GetBool("VRC.SDKBase_showAvatarPerformanceDetails", false);
            set => EditorPrefs.SetBool("VRC.SDKBase_showAvatarPerformanceDetails",
                value);
        }

        private const int MAX_ACTION_TEXTURE_SIZE = 256;

        private static PropertyInfo _legacyBlendShapeNormalsPropertyInfo;

        private static PropertyInfo LegacyBlendShapeNormalsPropertyInfo
        {
            get
            {
                if (_legacyBlendShapeNormalsPropertyInfo != null)
                {
                    return _legacyBlendShapeNormalsPropertyInfo;
                }

                Type modelImporterType = typeof(ModelImporter);
                _legacyBlendShapeNormalsPropertyInfo = modelImporterType.GetProperty(
                    "legacyComputeAllNormalsFromSmoothingGroupsWhenMeshHasBlendShapes",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                );

                return _legacyBlendShapeNormalsPropertyInfo;
            }
        }

        private static Action GetAvatarSubSelectAction(Component avatar, Type type)
        {
            List<Type> t = new List<Type> { type };
            return GetAvatarSubSelectAction(avatar, t.ToArray());
        }

        private static Action GetAvatarSubSelectAction(Component avatar, Type[] types)
        {
            return () =>
            {
                List<UnityEngine.Object> gos = new List<UnityEngine.Object>();
                foreach (Type t in types)
                {
                    List<Component> components = avatar.gameObject.GetComponentsInChildrenExcludingEditorOnly(t, true);
                    foreach (Component c in components)
                        gos.Add(c.gameObject);
                }

                Selection.objects = gos.Count > 0 ? gos.ToArray() : new UnityEngine.Object[] { avatar.gameObject };
            };
        }

        private readonly List<ValidateResult> results = new List<ValidateResult>();

        public List<ValidateResult>? CheckAvatarForValidationIssues(VRC_AvatarDescriptor avatar)
        {
            results.Clear();

            if (avatar == null) return null;

            string vrcFilePath = UnityWebRequest.UnEscapeURL(EditorPrefs.GetString("currentBuildingAssetBundlePath"));
            bool isMobilePlatform = ValidationEditorHelpers.IsMobilePlatform();
            if (!string.IsNullOrEmpty(vrcFilePath) &&
                ValidationHelpers.CheckIfAssetBundleFileTooLarge(ContentType.Avatar, vrcFilePath, out int fileSize, isMobilePlatform))
            {
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Warning, ValidationHelpers.GetAssetBundleOverSizeLimitMessageSDKWarning(ContentType.Avatar, fileSize, isMobilePlatform), delegate { Selection.activeObject = avatar.gameObject; }, null));
            }

            var perfStats = new AvatarPerformanceStats(ValidationEditorHelpers.IsMobilePlatform());
            AvatarPerformance.CalculatePerformanceStats(avatar.Name, avatar.gameObject, perfStats, isMobilePlatform);

            var OverallResults = CheckPerformanceInfo(avatar, perfStats, AvatarPerformanceCategory.Overall,
                               GetAvatarSubSelectAction(avatar, typeof(VRC_AvatarDescriptor)), null);
            results.AddRange(OverallResults);

            var polyCountResults = CheckPerformanceInfo(avatar, perfStats, AvatarPerformanceCategory.PolyCount,
                               GetAvatarSubSelectAction(avatar, new[] { typeof(MeshRenderer), typeof(SkinnedMeshRenderer) }), null);
            results.AddRange(polyCountResults);

            var AABBResults = CheckPerformanceInfo(avatar, perfStats, AvatarPerformanceCategory.AABB,
                                              GetAvatarSubSelectAction(avatar, typeof(VRC_AvatarDescriptor)), null);
            results.AddRange(AABBResults);

            if (avatar.lipSync == VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape &&
                avatar.VisemeSkinnedMesh == null)
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("This avatar uses Visemes but the Face Mesh is not specified."), delegate { Selection.activeObject = avatar.gameObject; }, null));

            VerifyAvatarMipMapStreaming(avatar);
            VerifyMaxTextureSize(avatar);

            if (!avatar.TryGetComponent<Animator>(out var anim))
            {
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("This avatar does not contain an Animator, you need to add an Animator component for the avatar to work"), delegate { Selection.activeObject = avatar.gameObject; }, null));
                return results;
            }
            if (anim == null)
            {
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Warning, T7e.Get("This avatar does not contain an Animator, and will not animate in VRChat."), delegate { Selection.activeObject = avatar.gameObject; }, null));
            }
            else if (anim.isHuman == false)
            {
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Warning, T7e.Get("This avatar is not imported as a humanoid rig and will not play VRChat's provided animation set."), delegate { Selection.activeObject = avatar.gameObject; }, null));
            }
            else if (avatar.gameObject.activeInHierarchy == false)
            {
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("Your avatar is disabled in the scene hierarchy!"), delegate { Selection.activeObject = avatar.gameObject; }, null));
            }
            else
            {
                Transform lFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
                Transform rFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
                if ((lFoot == null) || (rFoot == null))
                    results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("Your avatar is humanoid, but its feet aren't specified!"), delegate { Selection.activeObject = avatar.gameObject; }, null));
                if (lFoot != null && rFoot != null)
                {
                    Vector3 footPos = lFoot.position - avatar.transform.position;
                    if (footPos.y < 0)
                        results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Warning, T7e.Get("Avatar feet are beneath the avatar's origin (the floor). That's probably not what you want."), delegate { Selection.activeObject = avatar.gameObject; }, null));
                }

                Transform lShoulder = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                Transform rShoulder = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
                if (lShoulder == null || rShoulder == null)
                    results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("Your avatar is humanoid, but its upper arms aren't specified!"), delegate { Selection.activeObject = avatar.gameObject; }, null));
                if (lShoulder != null && rShoulder != null)
                {
                    Vector3 shoulderPosition = lShoulder.position - avatar.transform.position;
                    if (shoulderPosition.y < 0.2f)
                        results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("This avatar is too short. The minimum is 20cm shoulder height."), delegate { Selection.activeObject = avatar.gameObject; }, null));
                    else if (shoulderPosition.y < 1.0f)
                        results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Warning, T7e.Get("This avatar is shorter than average."), delegate { Selection.activeObject = avatar.gameObject; }, null));
                    else if (shoulderPosition.y > 5.0f)
                        results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("This avatar is too tall. The maximum is 5m shoulder height."), delegate { Selection.activeObject = avatar.gameObject; }, null));
                    else if (shoulderPosition.y > 2.5f)
                        results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Warning, T7e.Get("This avatar is taller than average."), delegate { Selection.activeObject = avatar.gameObject; }, null));
                }

                if (AnalyzeIK(avatar, anim) == false)
                    results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Link, $"See Avatar Rig Requirements for more information.", null, null, VRCSdkControlPanelHelp.AVATAR_RIG_REQUIREMENTS_URL));
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, "Your avatar is humanoid, but its upper arms aren't specified!", delegate { Selection.activeObject = avatar.gameObject; }, null));
                if (!AnalyzeIK(avatar, anim))
                    results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Link, "See Avatar Rig Requirements for more information.", null, null, VRCSdkControlPanelHelp.AVATAR_RIG_REQUIREMENTS_URL));
            }

            ValidateFeatures(avatar, anim, perfStats);

            PipelineManager pm = avatar.GetComponent<PipelineManager>();

            PerformanceRating rating = perfStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.Overall);
            if (results.Count == 0)
            {
                if (!anim.isHuman)
                {
                    if (pm != null) pm.fallbackStatus = PipelineManager.FallbackStatus.InvalidRig;
                    results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Info, T7e.Get("This avatar does not have a humanoid rig, so it can not be used as a custom fallback."), null, null));
                }
                else if (rating > PerformanceRating.Good)
                {
                    if (pm != null) pm.fallbackStatus = PipelineManager.FallbackStatus.InvalidPerformance;
                    results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Info, T7e.Get("This avatar does not have an overall rating of Good or better, so it can not be used as a custom fallback. See the link below for details on Avatar Optimization."), null, null));
                }
                else
                {
                    if (pm != null) pm.fallbackStatus = PipelineManager.FallbackStatus.Valid;
                    results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Success, T7e.Get("This avatar can be used as a custom fallback."), null, null));
                    if (perfStats.animatorCount.HasValue && perfStats.animatorCount.Value > 1)
                        results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Info, T7e.Get("This avatar uses additional animators, they will be disabled when used as a fallback."), null, null));
                }

                // additional messages for Poor and Very Poor Avatars
#if UNITY_ANDROID || UNITY_IOS
                if (rating > PerformanceRating.Poor)
                    results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Info, T7e.Get("This avatar will be blocked by default due to performance. Your fallback will be shown instead.", null, null)));
                else if (rating > PerformanceRating.Medium)
                    results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Info, T7e.Get("This avatar will be blocked by default due to performance. Your fallback will be shown instead."), null, null));
#else
                if (rating > PerformanceRating.Medium)
                    results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Info, T7e.Get("This avatar will be blocked by default due to performance. Your fallback will be shown instead."), null, null));
#endif
            }
            else
            {
                // shouldn't matter because we can't hit upload button
                if (pm != null) pm.fallbackStatus = PipelineManager.FallbackStatus.InvalidPlatform;
            }

            return results;
        }

        private List<ValidateResult> CheckPerformanceInfo(VRC_AvatarDescriptor avatar, AvatarPerformanceStats perfStats, AvatarPerformanceCategory perfCategory, Action show, Action fix)
        {
            PerformanceRating rating = perfStats.GetPerformanceRatingForCategory(perfCategory);
            SDKPerformanceDisplay.GetSDKPerformanceInfoText(perfStats, perfCategory, out string text,
                out PerformanceInfoDisplayLevel displayLevel);

            var validationResults = new List<ValidateResult>();

            switch (displayLevel)
            {
                case PerformanceInfoDisplayLevel.None:
                    {
                        break;
                    }
                case PerformanceInfoDisplayLevel.Verbose:
                    {
                        if (ShowAvatarPerformanceDetails)
                        {
                            validationResults.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Success, text, show, fix));
                        }

                        break;
                    }
                case PerformanceInfoDisplayLevel.Info:
                    {
                        validationResults.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Success, text, show, fix));
                        break;
                    }
                case PerformanceInfoDisplayLevel.Warning:
                    {
                        validationResults.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Warning, text, show, fix));
                        break;
                    }
                case PerformanceInfoDisplayLevel.Error:
                    {
                        validationResults.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, text, show, fix));
                        validationResults.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, text, show, fix));
                        break;
                    }
                default:
                    {
                        validationResults.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("Unknown performance display level."), show, fix));
                        break;
                    }
            }

            return validationResults;
        }

        private ValidateResult? VerifyAvatarMipMapStreaming(Component avatar)
        {
            List<TextureImporter> badTextureImporters = new List<TextureImporter>();
            List<UnityEngine.Object> badTextures = new List<UnityEngine.Object>();
            foreach (Renderer r in avatar.gameObject.GetComponentsInChildrenExcludingEditorOnly<Renderer>(true))
            {
                foreach (Material m in r.sharedMaterials)
                {
                    if (!m)
                        continue;
                    int[] texIDs = m.GetTexturePropertyNameIDs();
                    if (texIDs == null)
                        continue;
                    foreach (int i in texIDs)
                    {
                        Texture t = m.GetTexture(i);
                        if (!t)
                            continue;
                        string path = AssetDatabase.GetAssetPath(t);
                        if (string.IsNullOrEmpty(path))
                            continue;
                        if (AssetImporter.GetAtPath(path) is TextureImporter importer && importer.mipmapEnabled && !importer.streamingMipmaps)
                        {
                            badTextureImporters.Add(importer);
                            badTextures.Add(t);
                        }
                    }
                }
            }

            if (badTextureImporters.Count == 0)
                return null;

            return new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("This avatar has mipmapped textures without 'Streaming Mip Maps' enabled."), () => Selection.objects = badTextures.ToArray(), () =>
            {
                List<string> paths = new List<string>();
                foreach (TextureImporter t in badTextureImporters)
                {
                    Undo.RecordObject(t, "Set Mip Map Streaming");
                    t.streamingMipmaps = true;
                    t.streamingMipmapsPriority = 0;
                    EditorUtility.SetDirty(t);
                    paths.Add(t.assetPath);
                }

                AssetDatabase.ForceReserializeAssets(paths);
                AssetDatabase.Refresh();
            });
        }

        private ValidateResult? VerifyMaxTextureSize(Component avatar)
        {
            var renderers = avatar.gameObject.GetComponentsInChildrenExcludingEditorOnly<Renderer>(true);
            List<TextureImporter> badTextureImporters = VRCSdkControlPanel.GetOversizeTextureImporters(renderers);

            if (badTextureImporters.Count == 0)
                return null;

            return new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("This avatar has textures bigger than 2048. Please reduce them to save memory for users."),
                null,
                () =>
                {
                    List<string> paths = new List<string>();
                    foreach (TextureImporter t in badTextureImporters)
                    {
                        Undo.RecordObject(t, $"Set Max Texture Size to {VRCSdkControlPanel.MAX_SDK_TEXTURE_SIZE}");
                        t.maxTextureSize = VRCSdkControlPanel.MAX_SDK_TEXTURE_SIZE;
                        EditorUtility.SetDirty(t);
                        paths.Add(t.assetPath);
                    }

                    AssetDatabase.ForceReserializeAssets(paths);
                    AssetDatabase.Refresh();
                });
        }

        private bool AnalyzeIK(Object ad, Animator anim)
        {
            bool hasHead = false;
            bool hasFeet = false;
            bool hasHands = false;
#if VRC_SDK_VRCSDK2
            bool hasThreeFingers = false;
#endif
            bool correctSpineHierarchy = false;
            bool correctLeftArmHierarchy = false;
            bool correctRightArmHierarchy = false;
            bool correctLeftLegHierarchy = false;
            bool correctRightLegHierarchy = false;

            bool status = true;

            Transform head = anim.GetBoneTransform(HumanBodyBones.Head);
            Transform lFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
            Transform lHand = anim.GetBoneTransform(HumanBodyBones.LeftHand);
            Transform rHand = anim.GetBoneTransform(HumanBodyBones.RightHand);

            hasHead = head != null;
            hasFeet = (lFoot != null && rFoot != null);
            hasHands = (lHand != null && rHand != null);

            if (!hasHead || !hasFeet || !hasHands)
            {
                results.Add(new ValidateResult(ad, ValidateResult.ValidateResultType.Error, T7e.Get("Humanoid avatar must have head, hands and feet bones mapped."),
                                       delegate { Selection.activeObject = anim.gameObject; }, null));
                return false;
            }

            Transform lThumb = anim.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
            Transform lIndex = anim.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
            Transform lMiddle = anim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
            Transform rThumb = anim.GetBoneTransform(HumanBodyBones.RightThumbProximal);
            Transform rIndex = anim.GetBoneTransform(HumanBodyBones.RightIndexProximal);
            Transform rMiddle = anim.GetBoneTransform(HumanBodyBones.RightMiddleProximal);

            Transform pelvis = anim.GetBoneTransform(HumanBodyBones.Hips);
            Transform chest = anim.GetBoneTransform(HumanBodyBones.Chest);
            Transform upperChest = anim.GetBoneTransform(HumanBodyBones.UpperChest);
            Transform torso = anim.GetBoneTransform(HumanBodyBones.Spine);

            Transform neck = anim.GetBoneTransform(HumanBodyBones.Neck);
            Transform lClav = anim.GetBoneTransform(HumanBodyBones.LeftShoulder);
            Transform rClav = anim.GetBoneTransform(HumanBodyBones.RightShoulder);


            if (neck == null || lClav == null || rClav == null || pelvis == null || torso == null || chest == null)
            {
                string missingElements =
                    ((neck == null) ? "Neck, " : "") +
                    (((lClav == null) || (rClav == null)) ? "Shoulders, " : "") +
                    ((pelvis == null) ? "Pelvis, " : "") +
                    ((torso == null) ? "Spine, " : "") +
                    ((chest == null) ? "Chest, " : "");
                missingElements = missingElements.Remove(missingElements.LastIndexOf(',')) + ".";
                results.Add(new ValidateResult(ad, ValidateResult.ValidateResultType.Error, T7e.Get("Spine hierarchy missing elements, please map: ") + missingElements,
                                                          delegate { Selection.activeObject = anim.gameObject; }, null));
                return false;
            }

            correctSpineHierarchy = (upperChest != null) ? (lClav.parent == upperChest && rClav.parent == upperChest && neck.parent == upperChest) : (lClav.parent == chest && rClav.parent == chest && neck.parent == chest);

            if (!correctSpineHierarchy)
            {
                results.Add(new ValidateResult(ad, ValidateResult.ValidateResultType.Error, T7e.Get("Spine hierarchy incorrect. Make sure that the parent of both Shoulders and the Neck is the Chest (or UpperChest if set)."),
                    delegate
                    {
                        List<Object> gos = new List<Object>
                        {
                            lClav.gameObject,
                            rClav.gameObject,
                            neck.gameObject,
                            upperChest?.gameObject ?? chest.gameObject
                        };
                        Selection.objects = gos.ToArray();
                    }, null));
                return false;
            }

            Transform lShoulder = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Transform lElbow = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            Transform rShoulder = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Transform rElbow = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);

            correctLeftArmHierarchy = lShoulder && lElbow && lShoulder.GetChild(0) == lElbow && lHand &&
                                      lElbow.GetChild(0) == lHand;
            correctRightArmHierarchy = rShoulder && rElbow && rShoulder.GetChild(0) == rElbow && rHand &&
                                       rElbow.GetChild(0) == rHand;

            if (!(correctLeftArmHierarchy && correctRightArmHierarchy))
            {
                results.Add(new ValidateResult(ad, ValidateResult.ValidateResultType.Error, T7e.Get("LowerArm is not first child of UpperArm or Hand is not first child of LowerArm: you may have problems with Forearm rotations."),
                    delegate
                    {
                        List<Object> gos = new List<Object>();
                        if (!correctLeftArmHierarchy && lShoulder)
                            gos.Add(lShoulder.gameObject);
                        if (!correctRightArmHierarchy && rShoulder)
                            gos.Add(rShoulder.gameObject);
                        if (gos.Count > 0)
                            Selection.objects = gos.ToArray();
                        else
                            Selection.activeObject = anim.gameObject;
                    }, null));
                status = false;
            }

            Transform lHip = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            Transform lKnee = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            Transform rHip = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            Transform rKnee = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);

            correctLeftLegHierarchy = lHip && lKnee && lHip.GetChild(0) == lKnee && lKnee.GetChild(0) == lFoot;
            correctRightLegHierarchy = rHip && rKnee && rHip.GetChild(0) == rKnee && rKnee.GetChild(0) == rFoot;

            if (!(correctLeftLegHierarchy && correctRightLegHierarchy))
            {
                results.Add(new ValidateResult(ad, ValidateResult.ValidateResultType.Error, T7e.Get("LowerLeg is not first child of UpperLeg or Foot is not first child of LowerLeg: you may have problems with Shin rotations."),
                    delegate
                    {
                        List<Object> gos = new List<Object>();
                        if (!correctLeftLegHierarchy && lHip)
                            gos.Add(lHip.gameObject);
                        if (!correctRightLegHierarchy && rHip)
                            gos.Add(rHip.gameObject);
                        if (gos.Count > 0)
                            Selection.objects = gos.ToArray();
                        else
                            Selection.activeObject = anim.gameObject;
                    }, null));
                status = false;
            }

            if (!(IsAncestor(pelvis, rFoot) && IsAncestor(pelvis, lFoot) && IsAncestor(pelvis, lHand) &&
                  IsAncestor(pelvis, rHand)))
            {
                results.Add(new ValidateResult(ad, ValidateResult.ValidateResultType.Error, T7e.Get("This avatar has a split hierarchy (Hips bone is not the ancestor of all humanoid bones). IK may not work correctly."),
                    delegate
                    {
                        List<Object> gos = new List<Object> { pelvis.gameObject };
                        if (!IsAncestor(pelvis, rFoot))
                            gos.Add(rFoot.gameObject);
                        if (!IsAncestor(pelvis, lFoot))
                            gos.Add(lFoot.gameObject);
                        if (!IsAncestor(pelvis, lHand))
                            gos.Add(lHand.gameObject);
                        if (!IsAncestor(pelvis, rHand))
                            gos.Add(rHand.gameObject);
                        Selection.objects = gos.ToArray();
                    }, null));
                status = false;
            }

            // if thigh bone rotations diverge from 180 from hip bone rotations, full-body tracking/ik does not work well
            if (!lHip || !rHip) return status;
            {
                Vector3 hipLocalUp = pelvis.InverseTransformVector(Vector3.up);
                Vector3 legLDir = lHip.TransformVector(hipLocalUp);
                Vector3 legRDir = rHip.TransformVector(hipLocalUp);
                float angL = Vector3.Angle(Vector3.up, legLDir);
                float angR = Vector3.Angle(Vector3.up, legRDir);
                if (angL >= 175f && angR >= 175f) return status;
                string angle = $"{Mathf.Min(angL, angR):F1}";
                results.Add(new ValidateResult(ad, ValidateResult.ValidateResultType.Warning, T7e.Get("The angle between pelvis and thigh bones should be close to 180 degrees. (This avatar's angle is ") + ($"{angle})") + T7e.Get("Your avatar may not work well with full-body IK and Tracking."),
                    delegate
                    {
                        List<Object> gos = new List<Object>();
                        if (angL < 175f)
                            gos.Add(rFoot.gameObject);
                        if (angR < 175f)
                            gos.Add(lFoot.gameObject);
                        Selection.objects = gos.ToArray();
                    }, null));
                status = false;
            }

            return status;
        }

        private static bool IsAncestor(Object ancestor, Transform child)
        {
            bool found = false;
            Transform thisParent = child.parent;
            while (thisParent != null)
            {
                if (thisParent == ancestor)
                {
                    found = true;
                    break;
                }

                thisParent = thisParent.parent;
            }

            return found;
        }

        private void GenerateDebugHashset(VRCAvatarDescriptor avatar)
        {
            avatar.animationHashSet.Clear();

            foreach (VRCAvatarDescriptor.CustomAnimLayer animLayer in avatar.baseAnimationLayers)
            {
                if (animLayer.animatorController is not AnimatorController controller) continue;

                foreach (AnimatorControllerLayer layer in controller.layers)
                {
                    ProcessStateMachine(layer.stateMachine, "");
                    void ProcessStateMachine(AnimatorStateMachine stateMachine, string prefix)
                    {
                        //Update prefix
                        prefix = prefix + stateMachine.name + ".";

                        //States
                        foreach (var state in stateMachine.states)
                        {
                            VRCAvatarDescriptor.DebugHash hash = new VRCAvatarDescriptor.DebugHash();
                            string fullName = prefix + state.state.name;
                            hash.hash = Animator.StringToHash(fullName);
                            hash.name = fullName.Remove(0, layer.stateMachine.name.Length + 1);
                            avatar.animationHashSet.Add(hash);
                        }

                        //Sub State Machines
                        foreach (var subMachine in stateMachine.stateMachines)
                            ProcessStateMachine(subMachine.stateMachine, prefix);
                    }
                }
            }
        }

        private void ValidateFeatures(VRC_AvatarDescriptor avatar, Animator anim, AvatarPerformanceStats perfStats)
        {
            //Create avatar debug hashset
            VRCAvatarDescriptor avatarSDK3 = avatar as VRCAvatarDescriptor;
            if (avatarSDK3 != null)
            {
                GenerateDebugHashset(avatarSDK3);
            }

            //Validate Playable Layers
            if (avatarSDK3 != null && avatarSDK3.customizeAnimationLayers)
            {
                VRCAvatarDescriptor.CustomAnimLayer gestureLayer = avatarSDK3.baseAnimationLayers[2];
                if (anim != null
                    && anim.isHuman
                    && gestureLayer.animatorController != null
                    && gestureLayer.type == VRCAvatarDescriptor.AnimLayerType.Gesture
                    && !gestureLayer.isDefault)
                {
                    AnimatorController controller = gestureLayer.animatorController as AnimatorController;
                    if (controller is AnimatorController animController && animController.layers[0].avatarMask == null)
                        results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("Gesture Layer needs valid mask on first animator layer"),
                                                       delegate { OpenAnimatorControllerWindow(animController); }, null));
                }
            }

            //Expression menu images
            if (avatarSDK3 != null)
            {
                bool ValidateTexture(Texture2D texture)
                {
                    string path = AssetDatabase.GetAssetPath(texture);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null)
                        return true;
                    TextureImporterPlatformSettings settings = importer.GetDefaultPlatformTextureSettings();

                    //Max texture size
                    if ((texture.width > MAX_ACTION_TEXTURE_SIZE || texture.height > MAX_ACTION_TEXTURE_SIZE) &&
                        settings.maxTextureSize > MAX_ACTION_TEXTURE_SIZE)
                        return false;

                    //Compression
                    if (settings.textureCompression == TextureImporterCompression.Uncompressed)
                        return false;

                    //Success
                    return true;
                }

                void FixTexture(Texture2D texture)
                {
                    string path = AssetDatabase.GetAssetPath(texture);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null)
                        return;
                    TextureImporterPlatformSettings settings = importer.GetDefaultPlatformTextureSettings();

                    //Max texture size
                    if (texture.width > MAX_ACTION_TEXTURE_SIZE || texture.height > MAX_ACTION_TEXTURE_SIZE)
                        settings.maxTextureSize = Math.Min(settings.maxTextureSize, MAX_ACTION_TEXTURE_SIZE);

                    //Compression
                    if (settings.textureCompression == TextureImporterCompression.Uncompressed)
                        settings.textureCompression = TextureImporterCompression.Compressed;

                    //Set & Reimport
                    importer.SetPlatformTextureSettings(settings);
                    AssetDatabase.ImportAsset(path);
                }

                //Find all textures
                List<Texture2D> textures = new List<Texture2D>();
                List<VRCExpressionsMenu> menuStack = new List<VRCExpressionsMenu>();
                FindTextures(avatarSDK3.expressionsMenu);

                void FindTextures(VRCExpressionsMenu menu)
                {
                    if (menu == null || menuStack.Contains(menu)) //Prevent recursive menu searching
                        return;
                    menuStack.Add(menu);

                    //Check controls
                    foreach (VRCExpressionsMenu.Control control in menu.controls)
                    {
                        AddTexture(control.icon);
                        if (control.labels != null)
                        {
                            foreach (VRCExpressionsMenu.Control.Label label in control.labels)
                                AddTexture(label.icon);
                        }

                        if (control.subMenu != null)
                            FindTextures(control.subMenu);
                    }

                    void AddTexture(Texture2D texture)
                    {
                        if (texture != null)
                            textures.Add(texture);
                    }
                }

                //Validate
                bool isValid = true;
                foreach (Texture2D texture in textures)
                {
                    if (!ValidateTexture(texture))
                        isValid = false;
                }

                if (!isValid)
                    results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("Images used for Actions & Moods are too large."),
                                                                          delegate { Selection.activeObject = avatar.gameObject; }, FixTextures));

                //Fix
                void FixTextures()
                {
                    foreach (Texture2D texture in textures)
                        FixTexture(texture);
                }
            }

            //Expression menu parameters
            if (avatarSDK3 != null)
            {
                //Check for expression menu/parameters object
                if (avatarSDK3.expressionsMenu != null || avatarSDK3.expressionParameters != null)
                {
                    //Menu
                    if (avatarSDK3.expressionsMenu == null)
                        results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("VRCExpressionsMenu object reference is missing."),
                                                       delegate { Selection.activeObject = avatarSDK3; }, null));

                    //Parameters
                    if (avatarSDK3.expressionParameters == null)
                        results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("VRCExpressionParameters object reference is missing."),
                                                                                  delegate { Selection.activeObject = avatarSDK3; }, null));
                }

                //Check if parameters is valid
                if (avatarSDK3.expressionParameters != null && avatarSDK3.expressionParameters.CalcTotalCost() > VRCExpressionParameters.MAX_PARAMETER_COST)
                {
                    results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("VRCExpressionParameters has too many parameters defined."),
                                                                         delegate { Selection.activeObject = avatarSDK3.expressionParameters; }, null));
                }

                //Find all existing parameters
                if (avatarSDK3.expressionsMenu != null && avatarSDK3.expressionParameters != null)
                {
                    List<VRCExpressionsMenu> menuStack = new List<VRCExpressionsMenu>();
                    List<string> parameters = new List<string>();
                    List<VRCExpressionsMenu> selects = new List<VRCExpressionsMenu>();
                    FindParameters(avatarSDK3.expressionsMenu);

                    void FindParameters(VRCExpressionsMenu menu)
                    {
                        if (menu == null || menuStack.Contains(menu)) //Prevent recursive menu searching
                            return;
                        menuStack.Add(menu);

                        //Check controls
                        foreach (VRCExpressionsMenu.Control control in menu.controls)
                        {
                            AddParameter(control.parameter);
                            if (control.subParameters != null)
                            {
                                foreach (VRCExpressionsMenu.Control.Parameter subParameter in control.subParameters)
                                {
                                    AddParameter(subParameter);
                                }
                            }

                            if (control.subMenu != null)
                                FindParameters(control.subMenu);
                        }

                        void AddParameter(VRCExpressionsMenu.Control.Parameter parameter)
                        {
                            if (parameter != null)
                            {
                                parameters.Add(parameter.name);
                                selects.Add(menu);
                            }
                        }
                    }

                    //Validate parameters
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        string parameter = parameters[i];
                        VRCExpressionsMenu select = selects[i];

                        //Find
                        bool exists = string.IsNullOrEmpty(parameter) || avatarSDK3.expressionParameters.FindParameter(parameter) != null;
                        if (!exists)
                        {
                            results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("VRCExpressionsMenu uses a parameter that is not defined."),
                                                                                                delegate { Selection.activeObject = select; }, null));
                        }
                    }

                    //Validate param choices
                    foreach (var menu in menuStack)
                    {
                        foreach (var control in menu.controls)
                        {
                            bool isValid = true;
                            if (control.type == VRCExpressionsMenu.Control.ControlType.FourAxisPuppet)
                            {
                                isValid &= ValidateNonBoolParam(control.subParameters[0].name);
                                isValid &= ValidateNonBoolParam(control.subParameters[1].name);
                                isValid &= ValidateNonBoolParam(control.subParameters[2].name);
                                isValid &= ValidateNonBoolParam(control.subParameters[3].name);
                            }
                            else if (control.type == VRCExpressionsMenu.Control.ControlType.RadialPuppet)
                            {
                                isValid &= ValidateNonBoolParam(control.subParameters[0].name);
                            }
                            else if (control.type == VRCExpressionsMenu.Control.ControlType.TwoAxisPuppet)
                            {
                                isValid &= ValidateNonBoolParam(control.subParameters[0].name);
                                isValid &= ValidateNonBoolParam(control.subParameters[1].name);
                            }
                            if (!isValid)
                            {
                                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("VRCExpressionsMenu uses an invalid parameter for a control.\nControl: ") + control.name,
                                                                       delegate { Selection.activeObject = menu; }, null));
                            }
                        }

                        bool ValidateNonBoolParam(string name)
                        {
                            VRCExpressionParameters.Parameter param = string.IsNullOrEmpty(name) ? null : avatarSDK3.expressionParameters.FindParameter(name);
                            if (param?.valueType == VRCExpressionParameters.ValueType.Bool)
                                return false;
                            return true;
                        }
                    }
                }

                //Dynamic Bones
                if (perfStats.dynamicBone != null && (perfStats.dynamicBone.Value.colliderCount > 0 || perfStats.dynamicBone.Value.componentCount > 0))
                {
                    results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Warning, T7e.Get("This avatar uses depreciated DynamicBone components. Upgrade to PhysBones to guarantee future compatibility."),
                        null, () => AvatarDynamicsSetup.ConvertDynamicBonesToPhysBones(new GameObject[] { avatar.gameObject })));
                }
            }

            List<Component> componentsToRemove = VRC.SDK3.Validation.AvatarValidation.FindIllegalComponents(avatar.gameObject).ToList();

            // create a list of the PipelineSaver component(s)
            List<Component> toRemoveSilently = new List<Component>();
            foreach (Component c in componentsToRemove)
            {
                if (c.GetType().Name == "PipelineSaver")
                {
                    toRemoveSilently.Add(c);
                }
            }

            // delete PipelineSaver(s) from the list of the Components we will destroy now
            foreach (Component c in toRemoveSilently)
            {
                componentsToRemove.Remove(c);
            }

            HashSet<string> componentsToRemoveNames = new HashSet<string>();
            List<Component> toRemove = componentsToRemove;
            foreach (Component c in toRemove)
            {
                if (!componentsToRemoveNames.Contains(c.GetType().Name))
                    componentsToRemoveNames.Add(c.GetType().Name);
            }

            if (componentsToRemoveNames.Count > 0)
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("The following component types are found on the Avatar and will be removed by the client: ") +
                                       string.Join(", ", componentsToRemoveNames.ToArray()), delegate { ShowRestrictedComponents(toRemove); }, delegate { FixRestrictedComponents(toRemove); }));

            List<AudioSource> audioSources =
                avatar.gameObject.GetComponentsInChildrenExcludingEditorOnly<AudioSource>(true).ToList();
            if (audioSources.Count > 0)
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Warning, T7e.Get("Audio sources found on Avatar, they will be adjusted to safe limits, if necessary."),
                                                          GetAvatarSubSelectAction(avatar, typeof(AudioSource)), null));

            foreach (var audioSource in audioSources)
            {
                if (audioSource.clip && audioSource.clip.loadType == AudioClipLoadType.DecompressOnLoad && !audioSource.clip.loadInBackground)
                    results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("Found an audio clip with load type `Decompress On Load` which doesn't have `Load In Background` enabled.\nPlease enable `Load In Background` on the audio clip."),
                                                                                                 GetAvatarAudioSourcesWithDecompressOnLoadWithoutBackgroundLoad(avatar), null));
            }

            List<VRCStation> stations =
                avatar.gameObject.GetComponentsInChildrenExcludingEditorOnly<VRCStation>(true).ToList();
            if (stations.Count > 0)
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Warning, T7e.Get("Stations found on Avatar, they will be adjusted to safe limits, if necessary."),
                                                                             GetAvatarSubSelectAction(avatar, typeof(VRCStation)), null));

            if (VRCSdkControlPanel.HasSubstances(avatar.gameObject))
            {
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Warning, T7e.Get("This avatar has one or more Substance materials, which is not supported and may break in-game. Please bake your Substances to regular materials."),
                    () => Selection.objects = VRCSdkControlPanel.GetSubstanceObjects(avatar.gameObject), null));
            }

            CheckAvatarMeshesForLegacyBlendShapesSetting(avatar);
            CheckAvatarMeshesForMeshReadWriteSetting(avatar);

#if UNITY_ANDROID
            IEnumerable<Shader> illegalShaders = VRC.SDK3.Validation.AvatarValidation.FindIllegalShaders(avatar.gameObject);
            foreach (Shader s in illegalShaders)
            {
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("Avatar uses unsupported shader '") + s.name + T7e.Get("'. You can only use the shaders provided in 'VRChat/Mobile' for Quest avatars."),
                                                                                      delegate () { Selection.activeObject = avatar.gameObject; }, null));
            }
#endif

            if (ScanAvatarForWriteDefaults(avatarSDK3))
            {
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Warning, T7e.Get("One or more of the animation states on this avatar have Write Defaults turned on. We recommend keeping Write Defaults off and explicitly animating any parameter that needs to be set by the animation instead."),
                                                                                      null, null));
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Link, T7e.Get("Write Defaults Guidelines"), null, null, VRCSdkControlPanelHelp.AVATAR_WRITE_DEFAULTS_ON_STATES_URL));
            }

            foreach (AvatarPerformanceCategory perfCategory in Enum.GetValues(typeof(AvatarPerformanceCategory)))
            {
                if (perfCategory == AvatarPerformanceCategory.Overall ||
                    perfCategory == AvatarPerformanceCategory.PolyCount ||
                    perfCategory == AvatarPerformanceCategory.AABB ||
                    perfCategory == AvatarPerformanceCategory.AvatarPerformanceCategoryCount)
                {
                    continue;
                }

                Action show = null;

                switch (perfCategory)
                {
                    case AvatarPerformanceCategory.AnimatorCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(Animator));
                        break;
                    case AvatarPerformanceCategory.AudioSourceCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(AudioSource));
                        break;
                    case AvatarPerformanceCategory.BoneCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(SkinnedMeshRenderer));
                        break;
                    case AvatarPerformanceCategory.ClothCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(Cloth));
                        break;
                    case AvatarPerformanceCategory.ClothMaxVertices:
                        show = GetAvatarSubSelectAction(avatar, typeof(Cloth));
                        break;
                    case AvatarPerformanceCategory.LightCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(Light));
                        break;
                    case AvatarPerformanceCategory.LineRendererCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(LineRenderer));
                        break;
                    case AvatarPerformanceCategory.MaterialCount:
                        show = GetAvatarSubSelectAction(avatar,
                            new[] { typeof(MeshRenderer), typeof(SkinnedMeshRenderer) });
                        break;
                    case AvatarPerformanceCategory.MeshCount:
                        show = GetAvatarSubSelectAction(avatar,
                            new[] { typeof(MeshRenderer), typeof(SkinnedMeshRenderer) });
                        break;
                    case AvatarPerformanceCategory.ParticleCollisionEnabled:
                        show = GetAvatarSubSelectAction(avatar, typeof(ParticleSystem));
                        break;
                    case AvatarPerformanceCategory.ParticleMaxMeshPolyCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(ParticleSystem));
                        break;
                    case AvatarPerformanceCategory.ParticleSystemCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(ParticleSystem));
                        break;
                    case AvatarPerformanceCategory.ParticleTotalCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(ParticleSystem));
                        break;
                    case AvatarPerformanceCategory.ParticleTrailsEnabled:
                        show = GetAvatarSubSelectAction(avatar, typeof(ParticleSystem));
                        break;
                    case AvatarPerformanceCategory.PhysicsColliderCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(Collider));
                        break;
                    case AvatarPerformanceCategory.PhysicsRigidbodyCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(Rigidbody));
                        break;
                    case AvatarPerformanceCategory.PolyCount:
                        show = GetAvatarSubSelectAction(avatar,
                            new[] { typeof(MeshRenderer), typeof(SkinnedMeshRenderer) });
                        break;
                    case AvatarPerformanceCategory.SkinnedMeshCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(SkinnedMeshRenderer));
                        break;
                    case AvatarPerformanceCategory.TrailRendererCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(TrailRenderer));
                        break;
                    case AvatarPerformanceCategory.PhysBoneComponentCount:
                    case AvatarPerformanceCategory.PhysBoneTransformCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone));
                        break;
                    case AvatarPerformanceCategory.PhysBoneColliderCount:
                    case AvatarPerformanceCategory.PhysBoneCollisionCheckCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBoneCollider));
                        break;
                    case AvatarPerformanceCategory.ContactCount:
                        show = GetAvatarSubSelectAction(avatar, typeof(VRC.Dynamics.ContactBase));
                        break;
                    default:
                        // Unhandled performance category
                        break;
                }

                // we can only show these buttons if DynamicBone is installed

                Type dynamicBoneType = typeof(VRC.SDK3.Validation.AvatarValidation).Assembly.GetType("DynamicBone");
                Type dynamicBoneColliderType = typeof(VRC.SDK3.Validation.AvatarValidation).Assembly.GetType("DynamicBoneCollider");
                if ((dynamicBoneType != null) && (dynamicBoneColliderType != null))
                {
                    switch (perfCategory)
                    {
                        case AvatarPerformanceCategory.DynamicBoneColliderCount:
                            show = GetAvatarSubSelectAction(avatar, dynamicBoneColliderType);
                            break;
                        case AvatarPerformanceCategory.DynamicBoneCollisionCheckCount:
                            show = GetAvatarSubSelectAction(avatar, dynamicBoneColliderType);
                            break;
                        case AvatarPerformanceCategory.DynamicBoneComponentCount:
                            show = GetAvatarSubSelectAction(avatar, dynamicBoneType);
                            break;
                        case AvatarPerformanceCategory.DynamicBoneSimulatedBoneCount:
                            show = GetAvatarSubSelectAction(avatar, dynamicBoneType);
                            break;
                        default:
                            // Unhandled performance category
                            break;
                    }
                }

                CheckPerformanceInfo(avatar, perfStats, perfCategory, show, null);
            }

            results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Link, T7e.Get("Avatar Optimization Tips"), null, null, VRCSdkControlPanelHelp.AVATAR_OPTIMIZATION_TIPS_URL));
        }

        private void OpenAnimatorControllerWindow(object animatorController)
        {
            Assembly asm = Assembly.Load("UnityEditor.Graphs");
            Module editorGraphModule = asm.GetModule("UnityEditor.Graphs.dll");
            Type animatorWindowType = editorGraphModule.GetType("UnityEditor.Graphs.AnimatorControllerTool");
            EditorWindow animatorWindow = EditorWindow.GetWindow(animatorWindowType, false, "Animator", false);
            PropertyInfo propInfo = animatorWindowType.GetProperty("animatorController");
            if (propInfo != null) propInfo.SetValue(animatorWindow, animatorController, null);
        }

        private static void ShowRestrictedComponents(IEnumerable<Component> componentsToRemove)
        {
            List<Object> gos = new List<Object>();
            foreach (Component c in componentsToRemove)
                gos.Add(c.gameObject);
            Selection.objects = gos.ToArray();
        }

        private static void FixRestrictedComponents(IEnumerable<Component> componentsToRemove)
        {
            if (componentsToRemove is List<Component> list)
            {
                for (int v = list.Count - 1; v > -1; v--)
                {
                    Object.DestroyImmediate(list[v]);
                }
            }
        }

        private static Action GetAvatarAudioSourcesWithDecompressOnLoadWithoutBackgroundLoad(Component avatar)
        {
            return () =>
            {
                List<Object> gos = new List<Object>();
                AudioSource[] audioSources = avatar.GetComponentsInChildren<AudioSource>(true);

                foreach (var audioSource in audioSources)
                {
                    if (audioSource.clip && audioSource.clip.loadType == AudioClipLoadType.DecompressOnLoad && !audioSource.clip.loadInBackground)
                    {
                        gos.Add(audioSource.gameObject);
                    }
                }

                Selection.objects = gos.Count > 0 ? gos.ToArray() : new Object[] { avatar.gameObject };
            };
        }

        private void CheckAvatarMeshesForLegacyBlendShapesSetting(Component avatar)
        {
            if (LegacyBlendShapeNormalsPropertyInfo == null)
            {
                Debug.LogError(
                    T7e.Get("Could not check for legacy blend shape normals because 'legacyComputeAllNormalsFromSmoothingGroupsWhenMeshHasBlendShapes' was not found."));
                return;
            }

            // Get all of the meshes used by skinned mesh renderers.
            HashSet<Mesh> avatarMeshes = GetAllMeshesInGameObjectHierarchy(avatar.gameObject);
            HashSet<Mesh> incorrectlyConfiguredMeshes =
                ScanMeshesForIncorrectBlendShapeNormalsSetting(avatarMeshes);
            if (incorrectlyConfiguredMeshes.Count > 0)
            {
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("This avatar contains skinned meshes that were imported with Blendshape Normals set to 'Calculate' but aren't using 'Legacy Blendshape Normals'. This will significantly increase the size of the uploaded avatar. This must be fixed in the mesh import settings before uploading."),
                    null, () => EnableLegacyBlendShapeNormals(incorrectlyConfiguredMeshes)));
            }
        }

        private static void EnableLegacyBlendShapeNormals(IEnumerable<Mesh> meshesToFix)
        {
            HashSet<string> meshAssetPaths = new HashSet<string>();
            foreach (Mesh meshToFix in meshesToFix)
            {
                // Can't get ModelImporter if the model isn't an asset.
                if (!AssetDatabase.Contains(meshToFix))
                {
                    continue;
                }

                string meshAssetPath = AssetDatabase.GetAssetPath(meshToFix);
                if (string.IsNullOrEmpty(meshAssetPath))
                {
                    continue;
                }

                if (meshAssetPaths.Contains(meshAssetPath))
                {
                    continue;
                }

                meshAssetPaths.Add(meshAssetPath);
            }

            foreach (string meshAssetPath in meshAssetPaths)
            {
                ModelImporter avatarImporter = AssetImporter.GetAtPath(meshAssetPath) as ModelImporter;
                if (avatarImporter == null)
                {
                    continue;
                }

                if (avatarImporter.importBlendShapeNormals != ModelImporterNormals.Calculate)
                {
                    continue;
                }

                LegacyBlendShapeNormalsPropertyInfo.SetValue(avatarImporter, true);
                avatarImporter.SaveAndReimport();
            }
        }

        private static HashSet<Mesh> ScanMeshesForIncorrectBlendShapeNormalsSetting(IEnumerable<Mesh> avatarMeshes)
        {
            HashSet<Mesh> incorrectlyConfiguredMeshes = new HashSet<Mesh>();
            foreach (Mesh avatarMesh in avatarMeshes)
            {
                // Can't get ModelImporter if the model isn't an asset.
                if (!AssetDatabase.Contains(avatarMesh))
                {
                    continue;
                }

                string meshAssetPath = AssetDatabase.GetAssetPath(avatarMesh);
                if (string.IsNullOrEmpty(meshAssetPath))
                {
                    continue;
                }

                ModelImporter avatarImporter = AssetImporter.GetAtPath(meshAssetPath) as ModelImporter;
                if (avatarImporter == null)
                {
                    continue;
                }

                if (avatarImporter.importBlendShapeNormals != ModelImporterNormals.Calculate)
                {
                    continue;
                }

                bool useLegacyBlendShapeNormals = (bool)LegacyBlendShapeNormalsPropertyInfo.GetValue(avatarImporter);
                if (useLegacyBlendShapeNormals)
                {
                    continue;
                }

                incorrectlyConfiguredMeshes.Add(avatarMesh);
            }

            return incorrectlyConfiguredMeshes;
        }

        private static HashSet<Mesh> GetAllMeshesInGameObjectHierarchy(GameObject avatar)
        {
            HashSet<Mesh> avatarMeshes = new HashSet<Mesh>();
            foreach (SkinnedMeshRenderer avatarSkinnedMeshRenderer in avatar
                .GetComponentsInChildrenExcludingEditorOnly<SkinnedMeshRenderer>(true))
            {
                if (avatarSkinnedMeshRenderer == null)
                {
                    continue;
                }

                Mesh skinnedMesh = avatarSkinnedMeshRenderer.sharedMesh;
                if (skinnedMesh == null)
                {
                    continue;
                }

                if (avatarMeshes.Contains(skinnedMesh))
                {
                    continue;
                }

                avatarMeshes.Add(skinnedMesh);
            }

            foreach (MeshFilter avatarMeshFilter in avatar.GetComponentsInChildrenExcludingEditorOnly<MeshFilter>(true))
            {
                if (avatarMeshFilter == null)
                {
                    continue;
                }

                Mesh skinnedMesh = avatarMeshFilter.sharedMesh;
                if (skinnedMesh == null)
                {
                    continue;
                }

                if (avatarMeshes.Contains(skinnedMesh))
                {
                    continue;
                }

                avatarMeshes.Add(skinnedMesh);
            }

            foreach (ParticleSystemRenderer avatarParticleSystemRenderer in avatar
                .GetComponentsInChildrenExcludingEditorOnly<ParticleSystemRenderer>(true))
            {
                if (avatarParticleSystemRenderer == null)
                {
                    continue;
                }

                Mesh[] avatarParticleSystemRendererMeshes = new Mesh[avatarParticleSystemRenderer.meshCount];
                avatarParticleSystemRenderer.GetMeshes(avatarParticleSystemRendererMeshes);
                foreach (Mesh avatarParticleSystemRendererMesh in avatarParticleSystemRendererMeshes)
                {
                    if (avatarParticleSystemRendererMesh == null)
                    {
                        continue;
                    }

                    if (avatarMeshes.Contains(avatarParticleSystemRendererMesh))
                    {
                        continue;
                    }

                    avatarMeshes.Add(avatarParticleSystemRendererMesh);
                }
            }

            return avatarMeshes;
        }

        private void CheckAvatarMeshesForMeshReadWriteSetting(Component avatar)
        {
            // Get all of the meshes used by skinned mesh renderers.
            HashSet<Mesh> avatarMeshes = GetAllMeshesInGameObjectHierarchy(avatar.gameObject);
            HashSet<Mesh> incorrectlyConfiguredMeshes =
                ScanMeshesForDisabledMeshReadWriteSetting(avatarMeshes);
            if (incorrectlyConfiguredMeshes.Count > 0)
            {
                results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Error, T7e.Get("This avatar contains meshes that were imported with Read/Write disabled. This must be fixed in the mesh import settings before uploading."),
                    null, () => EnableMeshReadWrite(incorrectlyConfiguredMeshes)));
            }
        }

        private static void EnableMeshReadWrite(IEnumerable<Mesh> meshesToFix)
        {
            HashSet<string> meshAssetPaths = new HashSet<string>();
            foreach (Mesh meshToFix in meshesToFix)
            {
                // Can't get ModelImporter if the model isn't an asset.
                if (!AssetDatabase.Contains(meshToFix))
                {
                    continue;
                }

                string meshAssetPath = AssetDatabase.GetAssetPath(meshToFix);
                if (string.IsNullOrEmpty(meshAssetPath))
                {
                    continue;
                }

                if (meshAssetPaths.Contains(meshAssetPath))
                {
                    continue;
                }

                meshAssetPaths.Add(meshAssetPath);
            }

            foreach (string meshAssetPath in meshAssetPaths)
            {
                ModelImporter avatarImporter = AssetImporter.GetAtPath(meshAssetPath) as ModelImporter;
                if (avatarImporter == null)
                {
                    continue;
                }

                if (avatarImporter.isReadable)
                {
                    continue;
                }

                avatarImporter.isReadable = true;
                avatarImporter.SaveAndReimport();
            }
        }

        private static HashSet<Mesh> ScanMeshesForDisabledMeshReadWriteSetting(IEnumerable<Mesh> avatarMeshes)
        {
            HashSet<Mesh> incorrectlyConfiguredMeshes = new HashSet<Mesh>();
            foreach (Mesh avatarMesh in avatarMeshes)
            {
                // Can't get ModelImporter if the model isn't an asset.
                if (!AssetDatabase.Contains(avatarMesh))
                {
                    continue;
                }

                string meshAssetPath = AssetDatabase.GetAssetPath(avatarMesh);
                if (string.IsNullOrEmpty(meshAssetPath))
                {
                    continue;
                }

                ModelImporter avatarImporter = AssetImporter.GetAtPath(meshAssetPath) as ModelImporter;
                if (avatarImporter == null)
                {
                    continue;
                }

                if (avatarImporter.isReadable)
                {
                    continue;
                }

                incorrectlyConfiguredMeshes.Add(avatarMesh);
            }

            return incorrectlyConfiguredMeshes;
        }

        private static bool ScanAvatarForWriteDefaults(VRCAvatarDescriptor avatarSDK3)
        {
            if (avatarSDK3 != null)
            {
                foreach (VRCAvatarDescriptor.CustomAnimLayer customLayer in avatarSDK3.baseAnimationLayers)
                {
                    if (customLayer.animatorController is AnimatorController controller)
                    {
                        foreach (AnimatorControllerLayer controllerLayer in controller.layers)
                        {
                            // This will scan this state machine and all child state machines it contains.
                            bool wdEncountered = ScanStateMachineForWriteDefaults(controllerLayer.stateMachine);
                            if (wdEncountered)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static bool ScanStateMachineForWriteDefaults(AnimatorStateMachine stateMachine)
        {
            foreach (ChildAnimatorState childState in stateMachine.states)
            {
                if (childState.state.writeDefaultValues)
                {
                    // Ignore blend trees using the Direct blend type, because disabling WD on these reportedly causes
                    // side-effects.
                    BlendTree blendTree = childState.state.motion as BlendTree;
                    if (blendTree == null || blendTree.blendType != BlendTreeType.Direct)
                    {
                        // Found any state that writes defaults.
                        return true;
                    }
                }
            }

            // This state machine could itself contain nested state machines. Recursively search those too.
            foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
            {
                bool subResult = ScanStateMachineForWriteDefaults(childStateMachine.stateMachine);
                if (subResult)
                {
                    return true;
                }
            }

            return false;
        }
    }
}