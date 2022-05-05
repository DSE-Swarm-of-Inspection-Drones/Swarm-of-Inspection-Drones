using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectionCoordinator : MonoBehaviour
{
    public float distanceWeight = 1f;
    public float verticalDistanceWeight = 1f;
    public float distanceToOtherDronesWeight = 1000f;
    public float rayWeight = 10f;

    public List<InspectionPoint> uninspectedPoints = new List<InspectionPoint>();
    public InspectionDrone[] inspectionDrones;
    SurfacePointGenerator surfacePointGenerator;

    // Start is called before the first frame update
    void Start()
    {
        surfacePointGenerator = GetComponent<SurfacePointGenerator>();
        foreach(Vector3 surfacePoint in surfacePointGenerator.SurfacePoints)
        {
            InspectionPoint temp = new InspectionPoint(surfacePoint);
            uninspectedPoints.Add(temp);
        }

        getInspectionDrones();
    }

    public InspectionDrone[] getInspectionDrones()
    {
        inspectionDrones = GetComponentsInChildren<InspectionDrone>();
        return inspectionDrones;
    }

    public void InspectedPoint(InspectionPoint inspectionPoint)
    {
        uninspectedPoints.Remove(inspectionPoint);
    }
}
