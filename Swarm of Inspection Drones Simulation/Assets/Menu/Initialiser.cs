using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Initialiser : MonoBehaviour
{
    [Header("Don'tDestroyOnload")]
   // GameObject simulationManager;
    GameObject storageManager;
    StorageManager storageManagerScript;
    GameObject sceneManager;

    [Header("Prefabs")]
    //public GameObject simulationManagerPrefab;
    public GameObject storageManagerPrefab;
    public GameObject sceneManagerPrefab;

    [Header("Storage")]
    List<string> directoryPaths = new List<string>();

    void Start()
    {
        //Probeer alle fundamentele scripts te vinden en gooi geen error wanneer ze niet gevonden worden
        try
        {
            //simulationManager = FindObjectOfType<SimulationManager>().gameObject;
            storageManager = FindObjectOfType<StorageManager>().gameObject;
            sceneManager = FindObjectOfType<SceneManagerWrapper>().gameObject;
        }
        catch { }

        //Wanneer een script niet gevonden werd, creer een nieuwe versie hiervan
        /*if (simulationManager == null)
        {
            simulationManager = Instantiate(simulationManagerPrefab);
            DontDestroyOnLoad(simulationManager);
        }*/
        if (sceneManager == null)
        {
            sceneManager = Instantiate(sceneManagerPrefab);
            DontDestroyOnLoad(sceneManager);
        }
        if (storageManager == null)
        {
            storageManager = Instantiate(storageManagerPrefab);
            DontDestroyOnLoad(storageManager);
        }

        if(Application.isBatchMode)
        {
            LoadBatchSim();
        }
    }

    public void LoadSingleSim()
    {
        sceneManager.GetComponent<SceneManagerWrapper>().LoadLevel("SimulationDemonstration");
    }

    public void LoadBatchSim()
    {
        sceneManager.GetComponent<SceneManagerWrapper>().LoadLevel("SimulationStart");
    }
}
