using UnityEditor;
using UnityEngine;

public class Guide
{
    private static EALibrary library = new EALibrary(); // EALibraryのインスタンスを作成

    public static void Draw(Rect position)
    {
        // Define the area for the library
        Rect libraryArea = new Rect(position.x, position.y, position.width, position.height);

        // Call the Draw method from the EALibrary instance to render the library UI or the article content
        library.Draw(libraryArea);
    }
}
