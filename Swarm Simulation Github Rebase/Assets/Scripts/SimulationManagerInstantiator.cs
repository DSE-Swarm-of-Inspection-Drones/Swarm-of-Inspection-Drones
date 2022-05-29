using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManagerInstantiator : MonoBehaviour
{
    public GameObject simulationManagerPrefab;
    // Start is called before the first frame update

    public bool automaticPlay;

    public int startNumber = 2;
    public int endNumber = 4;
    public int numberOfRuns = 1;
    public float pointDistance = 1.4f;

    public InputField startNumberInp;
    public InputField endNumberInp;
    public InputField numberOfRunsInp;
    public InputField pointDistanceInp;
    void Start()
    {
        if (Application.isBatchMode)
        {
            automaticPlay = true;
        }

        if (automaticPlay)
        {
            SpawnSimulationManager();
        }
    }

    private void Update()
    {
        if (!automaticPlay)
        {
            try
            {
                startNumber = int.Parse(startNumberInp.text);
                endNumber = int.Parse(endNumberInp.text);
                numberOfRuns = int.Parse(numberOfRunsInp.text);
                pointDistance = float.Parse(pointDistanceInp.text);
            }
            catch { }
        }
    }

    public void SpawnSimulationManager()
    {
        SimulationManager simulationManager = FindObjectOfType<SimulationManager>();
        if (simulationManager == null)
        {
            GameObject simulationManagerObj = Instantiate(simulationManagerPrefab);
            simulationManager = simulationManagerObj.GetComponent<SimulationManager>();
            DontDestroyOnLoad(simulationManagerObj);
            simulationManager.StartSimulationManager(startNumber, endNumber, numberOfRuns, pointDistance);
        }
    }
}
