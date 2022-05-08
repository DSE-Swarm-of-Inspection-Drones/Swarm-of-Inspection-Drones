using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    List<List<SimulationRun>> simulationBatches = new List<List<SimulationRun>>();
    Stack<SimulationRun> simulationStack = new Stack<SimulationRun>();
    int startNumber = 2;
    int endNumber = 4;
    int numberOfRuns = 1;
    float pointDistance = 1;

    bool simulationFinished = false;
    SceneManagerWrapper sceneManagerWrapper;
    void Start()
    {
        sceneManagerWrapper = FindObjectOfType<SceneManagerWrapper>();
        DontDestroyOnLoad(gameObject); //TODO: unsafe, when loading into this scene over and over many simulationmanagers will be created

        for (int droneNumber = startNumber; droneNumber <= endNumber; droneNumber++)
        {
            List<SimulationRun> currentBatch = new List<SimulationRun>();
            for (int runNumber = 0; runNumber <= numberOfRuns; runNumber++)
            {
                SimulationRun run = new SimulationRun(this, droneNumber, pointDistance);
                currentBatch.Add(run);
                simulationStack.Push(run);
            }
            simulationBatches.Add(currentBatch);
        }

        NextRun();
    }

    public void NextRun() //TODO: Move into Coroutine
    {
        sceneManagerWrapper.LoadLevel("Simulation");
        SimulationRun run = simulationStack.Pop();
        InspectionCoordinator inspectionCoordinator = FindObjectOfType<InspectionCoordinator>();
        run.StartSimulation(inspectionCoordinator);
    }

    /*public void RunFinished(SimulationRun run)
    {
        sceneManagerWrapper.LoadLevel("Simulation");
    }*/
}
