using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneVelocityPID : MonoBehaviour
{
    public Vector3 PIDAltC = new Vector3(10, 10, 10); //sensitivity of vel to alt
    public Vector3 PIDVertVelC = new Vector3(10, 10, 10); //sensitivity of actuation to vert vel
    public Vector3 PIDPositionC = new Vector3(20, 1, 50); //sensitivity of vel to pos
    public Vector3 PIDVelocityC = new Vector3(20, 1, 50); //sensitivity of actuation to pos
    public Vector3 PIDAttC = new Vector3(1, 1, 1); //Sensitivity of angvel to att
    public Vector3 PIDAngVelC = new Vector3(1, 1, 1); //Sensitivity of actuation to angvel
    public Vector3 PIDHorizontalAttC = new Vector3(1, 1, 1);

    public float maxThrust;
    public float maxVel;
    public float maxTorque;
    PID PIDAlt;
    PID PIDVertVel;
    PID PIDPositionX;
    PID PIDPositionZ;
    PID PIDVelocityX;
    PID PIDVelocityZ;

    PID PIDAtt;
    PID PIDAngVel;

    PID PIDHorizontalAtt;

    public Transform target = null;
    Rigidbody rb;

    public bool offsetOverride;
    public Vector3 overriddenOffset;

    public bool targetOverride;
    public Vector3 overriddenTargetPosition;

    public bool velocityOverride;
    public Vector3 overiddenVelocity;

    public bool horizontalTargetOverride;
    public Vector3 overriddenHorizontalTargetPosition;

    public Vector3 desiredVel;

    public bool fakeAngle = true;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        PIDAlt = new PID(PIDAltC, new float[] { -maxVel, maxVel }, new float[] { -maxVel, maxVel });
        PIDVertVel = new PID(PIDVertVelC, new float[] { -maxThrust, maxThrust }, new float[] { -maxThrust, maxThrust });
        PIDPositionX = new PID(PIDPositionC, new float[] { -maxVel, maxVel }, new float[] { -maxVel, maxVel });
        PIDPositionZ = new PID(PIDPositionC, new float[] { -maxVel, maxVel }, new float[] { -maxVel, maxVel });
        PIDVelocityX = new PID(PIDVelocityC, new float[] { -maxThrust, maxThrust }, new float[] { -maxThrust * 0.3f, maxThrust * 0.3f });
        PIDVelocityZ = new PID(PIDVelocityC, new float[] { -maxThrust, maxThrust }, new float[] { -maxThrust * 0.3f, maxThrust * 0.3f });
        PIDAtt = new PID(PIDAttC, new float[] { -1, 1 }, new float[] { -1, 1 });
        PIDAngVel = new PID(PIDAngVelC, new float[] { -maxTorque, maxTorque }, new float[] { -maxTorque, maxTorque });
        PIDHorizontalAtt = new PID(PIDHorizontalAttC, new float[] { -maxTorque, maxTorque }, new float[] { -maxTorque, maxTorque });
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target == null)
        {
            return; //wait for target
        }

        Vector3 offset = new Vector3(0, 0, 0);
        if (offsetOverride)
        {
            offset = overriddenOffset;
        }
        Vector3 targetPostion = target.position;
        if (targetOverride)
        {
            targetPostion = overriddenTargetPosition;
        }

        Vector3 positionError = targetPostion - (transform.position + transform.TransformDirection(offset));
        float vertVel = PIDAlt.GetResponse(positionError.y);
        float xVel = PIDPositionX.GetResponse(positionError.x);
        float zVel = PIDPositionZ.GetResponse(positionError.z);

        desiredVel = new Vector3(xVel, vertVel, zVel);
        Debug.DrawRay(transform.position, desiredVel, Color.magenta);
        //Debug.DrawRay(transform.position, rb.velocity, Color.blue);
        Vector3 goalVel;
        if (velocityOverride)
        {
            goalVel = overiddenVelocity;
        }
        else
        {
            goalVel = desiredVel;
        }

        /*if (goalVel.magnitude > maxVel)
        {
            goalVel = goalVel.normalized * maxVel;
        }*/
        
        //rb.velocity = goalVel;
        Vector3 velocityError = goalVel - rb.velocity;
        float vertThrust = PIDVertVel.GetResponse(velocityError.y);
        float xThrust = PIDVelocityX.GetResponse(velocityError.x);
        float zThrust = PIDVelocityZ.GetResponse(velocityError.z);

        Vector3 desiredThrust = new Vector3(xThrust, vertThrust, zThrust);
        if (fakeAngle)
        {
            float xAngle = Mathf.Asin(Mathf.Clamp(xThrust / vertThrust, -0.5f, 0.5f));
            float zAngle = Mathf.Asin(Mathf.Clamp(zThrust / vertThrust, -0.5f, 0.5f));
            if (xAngle == xAngle && zAngle == zAngle) //check for NaN and Inf
            {
                transform.localEulerAngles = new Vector3(Mathf.Rad2Deg * zAngle, 0, -Mathf.Rad2Deg * xAngle);
            }
        }
        else
        {
            //body acceleration (rotate transform.up)
            Vector3 rotationVector = -Vector3.Cross(desiredThrust, transform.up);
            float angleError = Vector3.Angle(transform.up, desiredThrust) * Mathf.Deg2Rad;
            float rotation = PIDAtt.GetResponse(angleError);
            Vector3 angVelDesire = rotation * rotationVector.normalized;
            Vector3 currentAngVel = rb.angularVelocity;
            Vector3 velRotationVector;
            velRotationVector = angVelDesire - currentAngVel;
            float velError = velRotationVector.magnitude;
            float angVel = PIDAngVel.GetResponse(velError);


            if (angVel == angVel)
            {
                rb.AddTorque(velRotationVector.normalized * angVel);
                //rb.AddTorque(rotationVector.normalized * rotation);
            }

            //point to objective (rotate around transform.up)
            //project positionError onto transform.up
            Vector3 horizontalPositionError = positionError;
            if (horizontalTargetOverride)
            {
                horizontalPositionError = overriddenHorizontalTargetPosition - transform.position;
            }
            Vector3 positionErrorOnHorizontal = Vector3.ProjectOnPlane(horizontalPositionError, transform.up);
            float horizontalAngleError = Vector3.SignedAngle(transform.forward, positionErrorOnHorizontal, transform.up);
            float horizontalAngleActuation = PIDHorizontalAtt.GetResponse(horizontalAngleError);
            if (horizontalAngleActuation == horizontalAngleActuation)
            {
                rb.AddTorque(transform.up * horizontalAngleActuation);
            }

            //Debug.DrawRay(transform.position, desiredThrust.normalized, Color.red);
            //Debug.DrawRay(transform.position, angVelDesire.normalized, Color.blue);
            //Debug.DrawRay(transform.position, velRotationVector.normalized, Color.green);
            //Debug.DrawRay(transform.position, currentAngVel.normalized, Color.yellow);
            //Debug.DrawRay(transform.position, desiredVel, Color.red);
            //Debug.DrawRay(transform.position, rb.velocity, Color.green);
            //rb.AddForce(new Vector3(xForce, 0, zForce));
        }
        
        if (vertThrust == vertThrust) //check for NaN and Inf
        {
            rb.AddForce(desiredThrust.magnitude * transform.up);
        }
    }
}
