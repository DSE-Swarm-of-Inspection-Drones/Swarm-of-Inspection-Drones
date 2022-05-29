using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TimeController : MonoBehaviour
{
    public float timeScale = 1f;
    PID TimePID = new PID(0.01f,0,0, new float[] { -0.1f, 0.1f });
    public float targetFramerate = 25; //Hz
    float currentUpdateFramerate;

    public bool controlTimePID;

    [Header("UI")]
    public Text frameRateText = null;
    public Text timeScaleText = null;


    private void Update()
    {
        currentUpdateFramerate = 1f / Time.deltaTime;
        if (frameRateText != null && timeScaleText != null)
        {
            frameRateText.text = currentUpdateFramerate.ToString();
            timeScaleText.text = timeScale.ToString();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!controlTimePID)
        {
            Time.timeScale = timeScale;
            return;
        }

        float error = currentUpdateFramerate - targetFramerate;
        
        float timeScaleAct = TimePID.GetResponse(error);
        if (timeScaleAct == timeScaleAct)
        {
            timeScaleAct = Mathf.Clamp(timeScaleAct, -5f, 0.01f);
            timeScale += timeScaleAct;
            timeScale = Mathf.Clamp(timeScale, 0.1f, 100f);
            Time.timeScale = timeScale;
        }
    }
}
