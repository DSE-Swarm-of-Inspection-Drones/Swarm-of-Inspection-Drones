using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPID : MonoBehaviour
{
    public Vector3 target;
    public Vector3 PIDC = new Vector3(10, 10, 10);

    PID AngPID;
    PID vertPID;
    
    void Start()
    {
        AngPID = new PID(PIDC, new float[] { -1, 1 }, new float[] { -10, 10});
        vertPID = new PID(PIDC, new float[] { -1, 1 }, new float[] { -10, 10 });
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 toTarget = target - transform.position;
        float toTargetAng = Vector3.Angle(transform.forward, toTarget);
        float response = AngPID.GetResponse(toTargetAng);

        Vector3 rotateVec = Vector3.Cross(transform.forward, toTarget);
        transform.Rotate(rotateVec, response * Time.fixedDeltaTime, Space.World);

        Vector3 vertPlaneNormal = Vector3.Cross(transform.forward, Vector3.up).normalized;
        Vector3 upOnVertPlane = Vector3.ProjectOnPlane(transform.up, vertPlaneNormal);
        if (upOnVertPlane.y < 0)
        {
            upOnVertPlane = -upOnVertPlane;
        }
        float toVert = Vector3.SignedAngle(transform.up, upOnVertPlane, transform.forward);
        Debug.DrawRay(transform.position, upOnVertPlane, Color.red);
        Debug.DrawRay(transform.position, transform.forward, Color.green);
        Debug.DrawRay(transform.position, vertPlaneNormal, Color.blue);
        Debug.DrawRay(transform.position, transform.up, Color.yellow);
        Debug.Log(toVert);
        transform.Rotate(transform.forward, vertPID.GetResponse(toVert) * Time.fixedDeltaTime, Space.World);
    }
}
