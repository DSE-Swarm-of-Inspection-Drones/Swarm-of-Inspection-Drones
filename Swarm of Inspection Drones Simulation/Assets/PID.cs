using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PID
{
    public float P;
    public float I;
    public float D;

    float prevError;

    float prevTime;

    float prop;
    float integ;
    float deriv;

    float[] integMax = new float[] { -Mathf.Infinity, Mathf.Infinity };

    public PID(float P, float I, float D)
    {
        this.P = P;
        this.I = I;
        this.D = D;
    }

    public PID(float P, float I, float D, float[] integMax): this(P,I,D)
    {
        this.integMax = integMax;
    }

    // Update is called once per frame
    public float GetResponse(float error)
    {
        float time = Time.time;
        float deltaTime = time - prevTime;

        prop = error;
        integ += error * deltaTime;
        Mathf.Clamp(integ, integMax[0], integMax[1]);
        deriv = (error - prevError) / deltaTime;

        prevTime = time;
        prevError = error;

        return P * prop + I * integ + D * deriv;
    }
}
