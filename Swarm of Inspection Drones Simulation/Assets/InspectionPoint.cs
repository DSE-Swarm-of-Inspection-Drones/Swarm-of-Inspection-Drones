using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectionPoint
{
    public Vector3 inspectionPosition;
    public Dictionary<InspectionDrone, float> inspectionCosts = new Dictionary<InspectionDrone, float>(); //unique to Drone
    public InspectionDrone claimedBy = null;

    public InspectionPoint(Vector3 inspectionPositionIn)
    {
        inspectionPosition = inspectionPositionIn;
    }
}
