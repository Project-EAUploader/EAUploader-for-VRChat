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

    public List<ValidateResult> CheckAvatarForValidationIssues(VRC_AvatarDescriptor avatar)
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

      var OverallResults = CheckPerformanceInfo(avatar, perfStats, AvatarPerformanceCategory.Overall, GetAvatarSubSelectAction(avatar, typeof(VRC_AvatarDescriptor)), null);
      results.AddRange(OverallResults);

      results.Add(new ValidateResult(avatar, ValidateResult.ValidateResultType.Link, T7e.Get("Avatar Optimization Tips"), null, null, VRCSdkControlPanelHelp.AVATAR_OPTIMIZATION_TIPS_URL));

      return results;
    }

    private List<ValidateResult> CheckPerformanceInfo(VRC_AvatarDescriptor avatar, AvatarPerformanceStats perfStats, AvatarPerformanceCategory perfCategory, Action show, Action fix)
    {
      PerformanceRating rating = perfStats.GetPerformanceRatingForCategory(perfCategory);
      SDKPerformanceDisplay.GetSDKPerformanceInfoText(perfStats, perfCategory, out string text,
          out PerformanceInfoDisplayLevel displayLevel);

      var validationResults = new List<ValidateResult>();

      var seperatedText = text.Split('-');

      var performanceLevel = seperatedText[0].Replace("Overall Performance: ", "").Trim();
      var performanceDetails = seperatedText[1].Trim();

      text = T7e.Get("Overall Performance: ") + performanceLevel + "\n" + T7e.Get(performanceDetails);

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

  }
}