using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SimulationManager : MonoBehaviour
{
    List<List<SimulationRun>> simulationBatches = new List<List<SimulationRun>>();
    Stack<SimulationRun> simulationStack = new Stack<SimulationRun>();
    int startNumber = 2;
    int endNumber = 4;
    int numberOfRuns = 1;
    float pointDistance = 1.4f;

    bool simulationFinished = false;
    SceneManagerWrapper sceneManagerWrapper;
    StorageManager storageManager;
    string dateTime;

    SimulationRun currentRun = null;
    void Start()
    {
        dateTime = DateTime.Now.ToString("MMddyyyy_HHmmss");
        sceneManagerWrapper = FindObjectOfType<SceneManagerWrapper>();
        storageManager = FindObjectOfType<StorageManager>();

        for (int droneNumber = startNumber; droneNumber <= endNumber; droneNumber++)
        {
            List<SimulationRun> currentBatch = new List<SimulationRun>();
            for (int runNumber = 0; runNumber <= numberOfRuns; runNumber++)
            {
                SimulationRun run = new SimulationRun(this, droneNumber, pointDistance, runNumber);
                currentBatch.Add(run);
                simulationStack.Push(run);
            }
            simulationBatches.Add(currentBatch);
        }

        NextRun();
    }

    public void NextRun() //TODO: Move into Coroutine
    {
        string level = "SimulationStart";
        sceneManagerWrapper.LoadLevel(level);
        StartCoroutine("WaitForStart", level);
    }

    IEnumerator WaitForStart(string sceneNumber)
    {
        while (SceneManager.GetActiveScene().name != sceneNumber)
        {
            yield return null;
        }

        // Do anything after proper scene has been loaded
        if (SceneManager.GetActiveScene().name == sceneNumber)
        {
            string level = "Simulation";
            sceneManagerWrapper.LoadLevel(level);
            StartCoroutine("ExecuteOnLoad", level);
        }
    }

    IEnumerator ExecuteOnLoad(string sceneNumber)
    {
        while (SceneManager.GetActiveScene().name != sceneNumber)
        {
            yield return null;
        }

        // Do anything after proper scene has been loaded
        if (SceneManager.GetActiveScene().name == sceneNumber)
        {
            if (currentRun != null)
            {
                storageManager.SaveSimulation(dateTime, startNumber, endNumber, 
                currentRun.numberOfDrones, currentRun.pointDistance, currentRun.runNumber.ToString(),
                currentRun.inspectionTIme, currentRun.simulationTime, currentRun.collisions);
            }
            currentRun = simulationStack.Pop();
            InspectionCoordinator inspectionCoordinator = FindObjectOfType<InspectionCoordinator>();
            currentRun.StartSimulation(inspectionCoordinator);
        }
    }




    /*public void RunFinished(SimulationRun run)
    {
        sceneManagerWrapper.LoadLevel("Simulation");
    }*/
}
