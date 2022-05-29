using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectionCoordinator : MonoBehaviour
{
    public bool simulationDemonstration = false;
    public GameObject inspectionPointsParent;

    public float distanceWeight = 1f;
    public float verticalDistanceWeight = 1f;
    public float distanceToOtherDronesWeight = 1000f;
    public float rayWeight = 10f;

    [SerializeField]
    public List<InspectionPoint> uninspectedPoints = null;

    private List<InspectionDrone> _inspectionDrones = new List<InspectionDrone>();
    public List<InspectionDrone> inspectionDrones
    {
        get { return _inspectionDrones; }
    }

    SimulationRun simulationRun;

    [Header("DroneSpawn")]
    public Vector3 droneStartPosition = new Vector3(-22, -7, 37);
    public GameObject dronePrefab;
    public GameObject guidePrefab;
    public GameObject guideParent;
    public int numberOfDrones = 1;

    int collisionAmount = 0;
    float startRealTime;
    float startTime;

    bool inspectionFinished = false;
    bool inspectionStarted = false;

    private void Start()
    {
        if (simulationDemonstration) StartInspection(numberOfDrones, null);
    }

    public void StartInspection(int numberOfDrones, SimulationRun simulationRun)
    {
        this.numberOfDrones = numberOfDrones;
        this.simulationRun = simulationRun;

        //Get inspection points
        uninspectedPoints = new List<InspectionPoint>(inspectionPointsParent.GetComponentsInChildren<InspectionPoint>());

        //Create inspection drones
        for (int i = 0; i < numberOfDrones; i++)
        {
            GameObject tempTarget = Instantiate(guidePrefab, droneStartPosition + new Vector3(1, 0, 0) * i * 4, Quaternion.identity, guideParent.transform);
            GameObject tempDrone = Instantiate(dronePrefab, droneStartPosition + new Vector3(1, 0, 0) * i * 4, Quaternion.identity, transform);
            InspectionDrone tempDroneScript = tempDrone.GetComponent<InspectionDrone>();
            _inspectionDrones.Add(tempDroneScript);
            
            tempDroneScript.guide = tempTarget.GetComponent<InspectionGuideContainer>().guide;
            tempDroneScript.guideTarget = tempTarget.GetComponent<InspectionGuideContainer>().guideTarget.transform;
        }

        //Start inspection drones
        foreach (InspectionDrone drone in _inspectionDrones)
        {
            drone.StartInspection(this, inspectionDrones);
        }

        //Set start time
        startTime = Time.time;
        startRealTime = Time.realtimeSinceStartup;

        inspectionStarted = true;
    }

    private void Update()
    {
        if (uninspectedPoints != null && uninspectedPoints.Count == 0 && inspectionStarted == true) //inspection is finished
        {
            if (!inspectionFinished) //run only once
            {
                Debug.Log("Inspection finished: " + Time.time.ToString());
                inspectionFinished = true;
                if (simulationRun != null)
                    simulationRun.InspectionFinished(collisionAmount, Time.time - startTime, Time.realtimeSinceStartup - startRealTime);
            }
        }
    }

    public void InspectedPoint(InspectionPoint inspectionPoint)
    {
        uninspectedPoints.Remove(inspectionPoint);
    }

    public void CollisionReport()
    {
        collisionAmount += 1;
    }

    private void OnDrawGizmos()
    {
        /*if (uninspectedPoints != null && uninspectedPoints.Count != 0)
        {
            foreach (InspectionPoint inspectoinPoint in uninspectedPoints)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(inspectoinPoint.inspectionPosition, 0.2f);

                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(inspectoinPoint.smallDronePosition, 0.2f);
                Gizmos.DrawLine(inspectoinPoint.smallDronePosition, inspectoinPoint.inspectionPosition);

                Gizmos.color = Color.green;
                Gizmos.DrawSphere(inspectoinPoint.largeDronePosition, 0.2f);
                Gizmos.DrawLine(inspectoinPoint.largeDronePosition, inspectoinPoint.inspectionPosition);
                //Gizmo draw line from drone to inspection point
                Gizmos.color = Color.white;
                Gizmos.DrawLine(inspectoinPoint.inspectionPosition, inspectoinPoint.inspectionPosition + inspectoinPoint.normal);

            }
        }
    }*/
    }
}
