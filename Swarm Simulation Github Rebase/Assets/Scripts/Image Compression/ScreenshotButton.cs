using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ImagesForCompression))]
public class ScreenshotButton : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ImagesForCompression myScript = (ImagesForCompression)target;


        if (GUILayout.Button("Run"))
        {
            myScript.CreateScreenshot();
        }
    }
}