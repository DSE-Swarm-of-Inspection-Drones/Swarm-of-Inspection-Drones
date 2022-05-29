using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InspectionCoordinator))]
public class InspectionCoordinatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        InspectionCoordinator myScript = (InspectionCoordinator)target;
        //if (GUILayout.Button("inspectCube"))
        //{
        //    myScript.InspectCube();
        //}

        /*if (GUILayout.Button("CreateInspectionPoints"))
        {
            myScript.GetInspectionPoints();
        }*/
    }
}