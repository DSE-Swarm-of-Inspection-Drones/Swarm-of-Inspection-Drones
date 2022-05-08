using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectionCoordinator : MonoBehaviour
{
    public float distanceWeight = 1f;
    public float verticalDistanceWeight = 1f;
    public float distanceToOtherDronesWeight = 1000f;
    public float rayWeight = 10f;

    private List<InspectionPoint> _uninspectedPoints = new List<InspectionPoint>();
    public List<InspectionPoint> uninspectedPoints
    {
        get { return _uninspectedPoints; }
    }

    private List<InspectionDrone> _inspectionDrones = new List<InspectionDrone>();
    public List<InspectionDrone> inspectionDrones
    {
        get { return _inspectionDrones; }
    }

    SurfacePointGenerator surfacePointGenerator;

    SimulationRun simulationRun;

    [Header("DroneSpawn")]
    public Vector3 droneStartPosition = new Vector3(-22, -7, 37);
    public GameObject drone;
    public GameObject target;

    int numberOfDrones;
    float pointDistance;

    int collisionAmount = 0;
    float startRealTime;
    float startTime;

    bool inspectionFinished;

    public void StartInspection(int numberOfDrones, float pointDistance, SimulationRun simulationRun)
    {
        this.numberOfDrones = numberOfDrones;
        this.pointDistance = pointDistance;
        this.simulationRun = simulationRun;

        surfacePointGenerator = GetComponent<SurfacePointGenerator>();
        surfacePointGenerator.CreateSurfacePoints(pointDistance);
        foreach (Vector3 surfacePoint in surfacePointGenerator.SurfacePoints)
        {
            InspectionPoint temp = new InspectionPoint(surfacePoint);
            _uninspectedPoints.Add(temp);
        }

        CreateInspectionDrones();
        StartInspection();
        startTime = Time.time;
        startRealTime = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        if (_uninspectedPoints.Count == 0)
        {
            if (!inspectionFinished)
            {
                Debug.Log("Inspection finished: " + Time.time.ToString());
                simulationRun.InspectionFinished(collisionAmount, Time.time - startTime, Time.realtimeSinceStartup - startRealTime);
                inspectionFinished = true;
            }
        }
    }

    private void CreateInspectionDrones()
    {
        for (int i = 0; i < numberOfDrones; i++)
        {
            GameObject tempTarget = Instantiate(target, droneStartPosition + new Vector3(1, 0, 0) * i*2, Quaternion.identity, transform);
            GameObject tempDrone = Instantiate(drone, droneStartPosition + new Vector3(1, 0, 0) * i*2, Quaternion.identity, transform);
            InspectionDrone tempDroneScript = tempDrone.GetComponent<InspectionDrone>();
            _inspectionDrones.Add(tempDroneScript);
            tempDroneScript.target = tempTarget.transform;
        }
    }

    private void StartInspection()
    {
        foreach (InspectionDrone drone in _inspectionDrones)
        {
            drone.StartInspection(this, inspectionDrones);
        }
    }

    public void InspectedPoint(InspectionPoint inspectionPoint)
    {
        _uninspectedPoints.Remove(inspectionPoint);
    }

    public void CollisionReport()
    {
        collisionAmount += 1;
    }
}
