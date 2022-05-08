using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationRun
{
    private float _collisions;
    public float collisions{get { return _collisions; }}

    private float _inspectionTime;
    public float inspectionTIme { get { return _inspectionTime; } }

    private float _simulationTime;
    public float simulationTime { get { return _simulationTime; } }

    InspectionCoordinator inspectionCoordinator;
    SimulationManager simulationManager;

    public int numberOfDrones = 2;
    public float pointDistance = 1;

    // Start is called before the first frame update

    public SimulationRun(SimulationManager simulationManager, int numberOfDrones, float pointDistance)
    {
        this.simulationManager = simulationManager;
        this.numberOfDrones = numberOfDrones;
        this.pointDistance = pointDistance;
    }

    public void StartSimulation(InspectionCoordinator inspectionCoordinator)
    {
        this.inspectionCoordinator = inspectionCoordinator;
        inspectionCoordinator.StartInspection(numberOfDrones, pointDistance, this);
    }

    public void Collision()
    {
        _collisions += 1;
    }

    public void InspectionFinished(int collisions, float inspectionTime, float simulationTime)
    {
        _collisions = collisions;
        _inspectionTime = inspectionTime;
        _simulationTime = simulationTime;
        simulationManager.NextRun();
    }
}
