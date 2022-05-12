using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SurfacePointGenerator))]
public class SurfacePointGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SurfacePointGenerator myScript = (SurfacePointGenerator)target;
        //if (GUILayout.Button("inspectCube"))
        //{
        //    myScript.InspectCube();
        //}

        if (GUILayout.Button("CreateSurfacePoints"))
        {
            myScript.CreateSurfacePointsButton();
        }
    }
}
