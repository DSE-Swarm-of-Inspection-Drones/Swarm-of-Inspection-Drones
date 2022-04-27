using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(NodeCreator))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NodeCreator myScript = (NodeCreator)target;
        if (GUILayout.Button("Build Object"))
        {
            myScript.CreateNodes();
        }
    }
}
