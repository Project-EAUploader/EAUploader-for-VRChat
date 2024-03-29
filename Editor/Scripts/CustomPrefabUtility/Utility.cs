using UnityEngine;
using VRC.SDKBase;
using VRM;

namespace EAUploader.CustomPrefabUtility
{
    public class Utility
    {
        public static float GetAvatarHeight(GameObject avatar)
        {
            var avatarDescriptor = avatar.GetComponent<VRC_AvatarDescriptor>();
            if (avatarDescriptor != null)
            {
                return avatarDescriptor.ViewPosition.y;
            }

            return 0f;
        }

        public static bool CheckAvatarHasVRCAvatarDescriptor(GameObject avatar)
        {
            if (avatar == null) return false;
            return avatar.GetComponent<VRC_AvatarDescriptor>() != null;
        }

        public static bool CheckAvatarIsVRM(GameObject avatar)
        {
            return avatar.GetComponent<VRMMeta>() != null;
        }

        public static VRC_AvatarDescriptor GetAvatarDescriptor(GameObject avatar)
        {
            return avatar.GetComponent<VRC_AvatarDescriptor>();
        }
    }
}
