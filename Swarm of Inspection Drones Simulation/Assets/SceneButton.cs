using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneButton : MonoBehaviour
{
    public string levelToLoad;

    public void GoToLevel()
    {
        FindObjectOfType<SceneManagerWrapper>().LoadLevel(levelToLoad);
    }
}
