using UnityEditor;
using UnityEngine;

public class Guide
{


    public static void Draw(Rect position)
    {

        Rect libraryArea = new Rect(position.x, position.y, position.width, position.height);


        library.Draw(libraryArea);
    }
}
