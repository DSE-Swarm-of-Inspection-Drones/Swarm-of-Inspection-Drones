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
    float[] actMax = new float[] { -Mathf.Infinity, Mathf.Infinity };

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

    public PID(Vector3 PIDCoefficients, float[] integMax, float[] actMax) : this(PIDCoefficients.x, PIDCoefficients.y, PIDCoefficients.z, integMax)
    {
        this.integMax = integMax;
        this.actMax = actMax;
    }

    // Update is called once per frame
    public float GetResponse(float error)
    {
        float time = Time.time;
        float deltaTime = time - prevTime;

        prop = error;
        if (float.IsNaN(prop)) prop = 0;
        integ += error * deltaTime;
        if (float.IsNaN(integ)) integ = 0;
        integ = Mathf.Clamp(integ, integMax[0], integMax[1]);
        deriv = (error - prevError) / deltaTime;
        if (float.IsNaN(deriv)) deriv = 0;

        prevTime = time;
        prevError = error;

        float actuation = Mathf.Clamp(P * prop + I * integ + D * deriv, actMax[0], actMax[1]);
        return actuation;
    }
}
