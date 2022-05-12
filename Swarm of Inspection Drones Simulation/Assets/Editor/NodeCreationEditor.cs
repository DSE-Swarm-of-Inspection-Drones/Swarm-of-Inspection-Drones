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
        if (GUILayout.Button("Generate Nodes"))
        {
            myScript.CreateNodes();
        }

        if (GUILayout.Button("Delete Nodes"))
        {
            myScript.DeleteNodes();
        }
    }
}
