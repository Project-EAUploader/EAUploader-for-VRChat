using UnityEditor;
using UnityEngine;
using static labels;
using static styles;

public class EAUploaderGuide
{
    private static EALibrary library = new EALibrary();

    public static void Draw(Rect position)
    {
        Rect libraryArea = new Rect(position.x, position.y, position.width, position.height);

        library.DrawPrivateLibrary(libraryArea, "VRChatSDK", 100);
    }
}

