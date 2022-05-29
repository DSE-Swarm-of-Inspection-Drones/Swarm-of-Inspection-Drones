using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverController : MonoBehaviour
{
    public Vector3 PIDHoverC = new Vector3(10,10,10);
    public Vector3 PIDPositionC = new Vector3(20, 1, 50);

    public float maxThrust;
    PID PIDHover;
    PID PIDPositionX;
    PID PIDPositionZ;

    public Transform target;
    Rigidbody rb;

    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        PIDHover = new PID(PIDHoverC.x, PIDHoverC.y, PIDHoverC.z, new float[] { -maxThrust, maxThrust });
        PIDPositionX = new PID(PIDPositionC.x, PIDPositionC.y, PIDPositionC.z, new float[] { -1, 1 });
        PIDPositionZ = new PID(PIDPositionC.x, PIDPositionC.y, PIDPositionC.z, new float[] { -1, 1 });
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 error = target.position - transform.position;
        float thrustForce = Mathf.Clamp(PIDHover.GetResponse(error.y), 0, maxThrust);
        float xForce = PIDPositionX.GetResponse(error.x);
        float zForce = PIDPositionZ.GetResponse(error.z);

        float xAngle = Mathf.Asin(Mathf.Clamp(xForce / thrustForce, -0.5f, 0.5f));
        float zAngle = Mathf.Asin(Mathf.Clamp(zForce / thrustForce, -0.5f, 0.5f));

        if (xAngle == xAngle && zAngle == zAngle) //check for NaN and Inf
        {
            transform.localEulerAngles = new Vector3(Mathf.Rad2Deg * zAngle, 0, -Mathf.Rad2Deg * xAngle);
        }

        rb.AddForce(thrustForce * transform.up);
        //rb.AddForce(new Vector3(xForce, 0, zForce));
    }
}
