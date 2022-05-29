using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneORCA : MonoBehaviour
{
    class Plane
    {
        public Vector3 normal;
        public Vector3 point;

        public Plane()
        {
            this.normal = Vector3.zero;
            this.point = Vector3.zero;
        }

        public Plane(Vector3 normal, Vector3 point)
        {
            this.normal = normal;
            this.point = point;
        }
    }

    class ORCASphere
    {
        public Vector3 position;
        public float radius;
    }


    DroneVelocityPID droneVelocityPID;
    Rigidbody rb;
    public GameObject planePrefab;
    
    public bool debugMode = false;
    List<ORCASphere> ORCASpheres;

    public float timeHorizon;
    public float avoidanceRadius = 2f;

    // Start is called before the first frame update
    void Start()
    {
        droneVelocityPID = GetComponent<DroneVelocityPID>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Construct ORCA planes: inspired by https://github.com/snape/RVO2-3D/blob/main/src/Agent.cpp
        DroneORCA[] ORCAObjects = FindObjectsOfType<DroneORCA>();
        List<Plane> ORCAPlanes = new List<Plane>();
        Vector3 newVelocity = droneVelocityPID.desiredVel;
        foreach (DroneORCA ORCAObject in ORCAObjects)
        {
            if (ORCAObject == this)
            {
                continue;
            }

            Vector3 otherPosition = ORCAObject.gameObject.transform.position;
            Vector3 relativePosition = otherPosition - transform.position;
            Vector3 otherVelocity = ORCAObject.gameObject.GetComponent<Rigidbody>().velocity;
            Vector3 relativeVelocity = rb.velocity - otherVelocity;

            Plane ORCAPlane = new Plane();
            Vector3 u;

            Vector3 w = relativeVelocity - relativePosition / timeHorizon; //outward normal if circle
            float wDotRelativePos = Vector3.Dot(w, relativePosition);
            float combinedRadius = avoidanceRadius * 2;
            if (wDotRelativePos < 0 && Mathf.Pow(wDotRelativePos, 2) > Mathf.Pow(combinedRadius * w.magnitude, 2))
            {
                // project onto circle
                ORCAPlane.normal = w.normalized;
                u = (combinedRadius / timeHorizon - w.magnitude) * w.normalized;
            }
            else
            {
                // project onto cone
                float a = Mathf.Pow(relativePosition.magnitude, 2);
                float b = Vector3.Dot(relativePosition, relativeVelocity);

                Vector3 relativeCross = Vector3.Cross(relativePosition, relativeVelocity);
                float c = Vector3.Dot(relativeVelocity, relativeVelocity)
                    - Vector3.Dot(relativeCross, relativeCross) / (Mathf.Pow(relativePosition.magnitude, 2) - Mathf.Pow(combinedRadius, 2));

                float t = (b + Mathf.Pow(Mathf.Pow(b, 2) - a * c, 0.5f)) / a;
                Vector3 ww = relativeVelocity - t * relativePosition;

                ORCAPlane.normal = ww.normalized;
                u = (combinedRadius * t - ww.magnitude) * ww.normalized;
            }

            ORCAPlane.point = rb.velocity + 0.5f * u; //rb.velocity.magnitude / relativeVelocity.magnitude
            ORCAPlanes.Add(ORCAPlane);

            if (debugMode)
            {
                Debug.DrawRay(transform.position, relativeVelocity, Color.green);
                Debug.DrawRay(transform.position, relativePosition, Color.blue);
                ORCASpheres = new List<ORCASphere>();
                Vector3 prevPos = Vector3.zero;
                for (float tau = 1; tau <= timeHorizon; tau += 0.1f)
                {
                    Vector3 position = relativePosition/tau + transform.position;
                    float radius = combinedRadius / tau;
                    if ((position - prevPos).magnitude > radius)
                    {
                        ORCASpheres.Add(new ORCASphere() { position = position, radius = radius });
                        prevPos = position;
                    }                        
                }
            }
        }

        
        //Select new velocity based on ORCA planes
        bool selectedVelocity = false;
        int iter = 0;
        
        while (!selectedVelocity && iter < 500)
        {
            selectedVelocity = true; //will be set false if not complicit
            foreach (Plane ORCAPlane in ORCAPlanes)
            {
                //determine if on wrong side of plane
                bool onWrongSide = false;
                Vector3 velocityDelta = ORCAPlane.point - newVelocity;
                float velocityDeltaDotNormal = Vector3.Dot(velocityDelta, ORCAPlane.normal);
                if (velocityDeltaDotNormal > 0)
                {
                    onWrongSide = true;
                }
                if(debugMode)
                Debug.DrawRay(transform.position + newVelocity, velocityDelta, Color.cyan);

                //if on wrong side, project onto plane
                if (onWrongSide)
                {
                    selectedVelocity = false;
                    Vector3 projectedRelativePosition = Vector3.Project(velocityDelta, ORCAPlane.normal);
                    newVelocity += projectedRelativePosition * 1.0001f;
                }
            }
            if (iter >= 499)
            {
                Debug.Log("ORCA failed");
            }
            iter++;
        }

        droneVelocityPID.velocityOverride = true;
        droneVelocityPID.overiddenVelocity = newVelocity;
     
        if (debugMode)
        {
            Debug.DrawRay(transform.position, newVelocity, Color.white);            
            foreach (Plane ORCAPlane in ORCAPlanes)
            {
                Debug.DrawRay(transform.position + ORCAPlane.point, ORCAPlane.normal.normalized, Color.red);
                Debug.DrawRay(transform.position, ORCAPlane.point, Color.yellow);
            }
        }
    }

    private void OnDrawGizmos()
    {
        //draw all orcaspheres
        if (debugMode && ORCASpheres != null)
        {
            foreach (ORCASphere sphere in ORCASpheres)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireSphere(sphere.position, sphere.radius);
            }
        }

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
    }
}
