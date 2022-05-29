using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class InspectionPoint:MonoBehaviour
{
    public Vector3 inspectionPosition; //location of the point to be inspected
    public Vector3 dronePosition
    {
        get
        {
            if (largeDrone)
            {
                return largeDronePosition;
            }
            else
            {
                return smallDronePosition;
            }
        }
    }
    
    public Vector3 largeDronePosition; //location drone tries to reach
    public Vector3 smallDronePosition; //location drone tries to reach
    //public Vector3 largeDronePosition; //location large drone tries to reach
    //public Vector3 smallDronePosition; //location small drone tries to reach
    public Vector3 normal;

    public Dictionary<InspectionDrone, float> inspectionCosts; //unique to Drone
    public InspectionDrone claimedBy = null;

    public bool largeDrone;
    public bool smallDrone;
    
    public InspectionPointGenerator.PotentialDronePoint largePotential;
    public InspectionPointGenerator.PotentialDronePoint smallPotential;

    public void Start()
    {
        inspectionCosts = new Dictionary<InspectionDrone, float>();
    }
    
    public void SetInspectionPoint(SurfacePointGenerator.SurfacePoint surfacePointIn)
    {
        inspectionPosition = surfacePointIn.position;
        normal = surfacePointIn.normal;
    }
    //public bool largeDrone;
    //public bool smallDrone;

    public InspectionPoint(SurfacePointGenerator.SurfacePoint surfacePointIn)
    {
        inspectionPosition = surfacePointIn.position;
        normal = surfacePointIn.normal;
    }


    private void OnDrawGizmos()
    {
        /*if (Selection.Contains(gameObject))
        {
            Debug.Log("This boi");
        }*/
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(inspectionPosition, 0.2f);

        if (smallDrone)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(smallDronePosition, 0.2f);
            Gizmos.DrawLine(smallDronePosition, inspectionPosition);
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(largeDronePosition, 0.1f);
            Gizmos.DrawLine(largeDronePosition, inspectionPosition);
        }
        else if (largeDrone)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(largeDronePosition, 0.2f);
            Gizmos.DrawLine(largeDronePosition, inspectionPosition);
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(smallDronePosition, 0.1f);
            Gizmos.DrawLine(smallDronePosition, inspectionPosition);
        }
        else
        {
            Gizmos.color = Color.yellow;
        }
        /*
        Gizmos.DrawSphere(inspectoinPoint.dronePosition, 0.2f);

        //Gizmo draw line from drone to inspection point
        Gizmos.color = Color.white;
        Gizmos.DrawLine(inspectoinPoint.dronePosition, inspectoinPoint.inspectionPosition);*/
    }
}
