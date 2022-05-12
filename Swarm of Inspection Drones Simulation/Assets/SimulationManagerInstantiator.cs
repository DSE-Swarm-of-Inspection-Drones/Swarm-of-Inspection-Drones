using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManagerInstantiator : MonoBehaviour
{
    public GameObject simulationManagerPrefab;
    // Start is called before the first frame update
    void Start()
    {
        SimulationManager simulationManager = FindObjectOfType<SimulationManager>();
        if (simulationManager == null)
        {
            GameObject simulationManagerObj = Instantiate(simulationManagerPrefab);
            DontDestroyOnLoad(simulationManagerObj);
        }
    }
}
