using UnityEngine;
using VRC.SDKBase;

namespace EAUploader.CustomPrefabUtility
{
    public class Utility
    {
        public static float GetAvatarHeight(GameObject avatar)
        {
            var avatarDescriptor = avatar.GetComponent<VRC_AvatarDescriptor>();
            if (avatarDescriptor != null)
            {
                // ViewPosition.y ���A�o�^�[�̖ڐ��̍���
                return avatarDescriptor.ViewPosition.y;
            }

            // �f�t�H���g
            return 0f;
        }

        public static bool CheckAvatarHasVRCAvatarDescriptor(GameObject avatar)
        {
            if (avatar == null) return false;
            return avatar.GetComponent<VRC_AvatarDescriptor>() != null;
        }

        public static VRC_AvatarDescriptor GetAvatarDescriptor(GameObject avatar)
        {
            return avatar.GetComponent<VRC_AvatarDescriptor>();
        }
    }
}