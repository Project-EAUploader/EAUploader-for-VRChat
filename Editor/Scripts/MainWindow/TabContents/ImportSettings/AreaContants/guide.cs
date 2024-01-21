#if !EA_ONBUILD
using UnityEditor;
using UnityEngine;

public class Guide
{
    // EALibraryインスタンス
    private static EALibrary library = new EALibrary();

    public static void Draw(Rect position)
    {
        Rect libraryArea = new Rect(position.x, position.y, position.width, position.height);

        library.Draw(libraryArea);
    }
}
#endif