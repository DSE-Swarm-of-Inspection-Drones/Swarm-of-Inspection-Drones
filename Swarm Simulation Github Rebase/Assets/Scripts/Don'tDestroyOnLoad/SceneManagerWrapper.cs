using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerWrapper : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void LoadLevel(string Level)
    {
        SceneManager.LoadScene(Level, LoadSceneMode.Single);
    }

    /*public bool LoadLevelBlocking(string level)
    {
        SceneManager.LoadScene(level);
        while (SceneManager.GetActiveScene().name != level)
        {
        }

        return true;
    }*/
}