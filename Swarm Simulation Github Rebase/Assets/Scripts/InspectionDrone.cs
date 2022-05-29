using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class InspectionDrone : MonoBehaviour
{  
    InspectionCoordinator inspectionCoordinator;
    List<InspectionDrone> inspectionDrones = null; //set by InspectionCoordinator

    InspectionPoint activePoint = null;
    InspectionPoint oldActivePoint = null;
    InspectionPoint olderActivePoint = null;

    DroneVelocityPID droneVelocityPID;
    PID guideVelocityPID;
    Vector3 guideVelocityC = new Vector3(1, 0, 0);
    Pathfinding.AILerp guideLerp;

    public float totalCost;
    bool updatedCosts = false;

    public GameObject guide;
    public Transform guideTarget;

    List<InspectionPoint> localInspectionPoints = null;

    public bool drawInspectionPoints = false;

    //int myIndex;

    public bool inspectionFinished;
    Vector3 startPos;

    CameraPID cameraPID;

    // Start is called before the first frame update
    public void StartInspection(InspectionCoordinator inspectionCoordinator, List<InspectionDrone> inspectionDrones)
    {
        this.inspectionCoordinator = inspectionCoordinator;
        this.inspectionDrones = inspectionDrones;

        cameraPID = GetComponentInChildren<CameraPID>();
        droneVelocityPID = GetComponent<DroneVelocityPID>();

        droneVelocityPID.target = guide.transform;
        droneVelocityPID.overriddenOffset = cameraPID.gameObject.transform.localPosition;
        startPos = transform.position;
        InvokeRepeating("UpdateCosts", transform.GetSiblingIndex() * Random.Range(0.9f, 1.1f)*2, Random.Range(0.9f*2, 1.1f*2)); // * 5f

        guideVelocityC = new Vector3(1, 1, 0); //???? Why ???? Me no understand
        guideVelocityPID = new PID(guideVelocityC, new float[] { 0.1f, 0.1f }, new float[] { -1f, 2f});
        guideLerp = guide.GetComponentInChildren<Pathfinding.AILerp>();
    }

    void FixedUpdate()
    {
        if (inspectionDrones == null)
        {
            return; //Wait untill StartInspection has run for sure.
        }

        //myIndex = transform.GetSiblingIndex();
        if (updatedCosts)
        {
            UpdateActivePoint();
        }

        if (activePoint != null)
        {
            guideTarget.position = activePoint.dronePosition;
            cameraPID.target = activePoint.inspectionPosition;

            float distanceToActivePoint = (cameraPID.gameObject.transform.position - activePoint.dronePosition).magnitude;
            if (distanceToActivePoint < 1)
            {
                droneVelocityPID.offsetOverride = true;
                droneVelocityPID.targetOverride = true;
                droneVelocityPID.overriddenTargetPosition = activePoint.dronePosition;

                droneVelocityPID.horizontalTargetOverride = true;
                droneVelocityPID.overriddenHorizontalTargetPosition = activePoint.inspectionPosition;
                float angleToDesiredCamera = Vector3.Angle(cameraPID.transform.forward, -activePoint.normal);
                if (distanceToActivePoint < 1 && angleToDesiredCamera < 10)
                {
                    inspectionCoordinator.InspectedPoint(activePoint);
                    activePoint = null;
                }
            }
            else
            {
                droneVelocityPID.offsetOverride = false;
                droneVelocityPID.targetOverride = false;
                droneVelocityPID.horizontalTargetOverride = false;
            }

            oldActivePoint = activePoint;
            olderActivePoint = oldActivePoint;
        }
        else
        {
            cameraPID.target = transform.position + transform.forward;
            if (inspectionFinished)
            {
                guideTarget.position = startPos;
            }
        }

        float distanceFromGuide = (guide.transform.position - transform.position).magnitude;
        float distanceFromeGuideError = 3 - distanceFromGuide;
        float guideVelocity = guideVelocityPID.GetResponse(distanceFromeGuideError);
        guideLerp.speed = guideVelocity;
    }

    void UpdateCosts()
    {
        localInspectionPoints = new List<InspectionPoint>(inspectionCoordinator.uninspectedPoints);
        totalCost = 0;
        foreach (InspectionPoint inspectionPoint in localInspectionPoints)
        {
            //Calculate costs
            //Distance to this drone
            Vector3 pointPosition = inspectionPoint.dronePosition;
            Vector3 relativePosition = Vector3.zero;
            if (activePoint != null)
            {
                relativePosition = (pointPosition - activePoint.dronePosition);
            }
            relativePosition += (pointPosition - transform.position);

            float distance = relativePosition.magnitude;
            //Vertical distance to this drone
            float verticalDistance = Mathf.Abs(relativePosition.y);

            float distanceToOtherDrones = 0f;
            //Distance to other drones
            foreach (InspectionDrone drone in inspectionDrones)
            {
                if (drone != this)
                {
                    if (drone.activePoint != null)
                    {
                        distanceToOtherDrones += 1f / Mathf.Pow((drone.activePoint.dronePosition - pointPosition).magnitude + 2f, 0.75f) - 1;
                    }
                    distanceToOtherDrones += 1f / Mathf.Pow((drone.gameObject.transform.position - pointPosition).magnitude + 2f, 0.75f) - 1;
                }
            }

            float ray = 0;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, (pointPosition - transform.position), out hit, (pointPosition - transform.position).magnitude * 0.9f))
            {
                ray = 1f;
            }

            float cost = inspectionCoordinator.distanceWeight * Mathf.Log(distance+0.1f) +
                inspectionCoordinator.verticalDistanceWeight * Mathf.Log(verticalDistance+0.1f) * 1f/distance +
                inspectionCoordinator.distanceToOtherDronesWeight * distanceToOtherDrones / inspectionDrones.Count +
                inspectionCoordinator.rayWeight * ray * 1f/distance;

            inspectionPoint.inspectionCosts[this] = cost;
            totalCost += cost;
        }

        localInspectionPoints.Sort((p1, p2) => p1.inspectionCosts[this].CompareTo(p2.inspectionCosts[this]));
        updatedCosts = true;
    }

    void UpdateActivePoint()
    {
        int i = 0;
        bool notChosen = true;
        while (notChosen)
        {
            if (i >= localInspectionPoints.Count)
            {
                inspectionFinished = true;
                return;
            }
            else
            {
                inspectionFinished = false; 
            }

            InspectionPoint investigating = localInspectionPoints[i];
            if (investigating.claimedBy == null || investigating.claimedBy == this)
            {
                activePoint = investigating;
                investigating.claimedBy = this;
                if (oldActivePoint != null)
                {
                    oldActivePoint.claimedBy = null;
                }

                notChosen = false;
            }
            i += 1;
        }
        updatedCosts = false;
    }

    /*private void OnTriggerEnter(Collider other)
    {
        inspectionCoordinator.CollisionReport();
        //Debug.Log("Collider");
    }*/

    private void OnCollisionEnter(Collision Collision)
    {
        if (inspectionCoordinator!=null)
        inspectionCoordinator.CollisionReport();
        //Debug.Log("Collision");
    }

    private void OnDrawGizmos()
    {
        /*if (drawInspectionPoints && localInspectionPoints != null && inspectionFinished == false)
        {
            float maxCost = localInspectionPoints[localInspectionPoints.Count - 1].inspectionCosts[this];
            float minCost = localInspectionPoints[0].inspectionCosts[this];
            foreach (InspectionPoint inspectionPoint in localInspectionPoints)
            {
                float relativeCost = (inspectionPoint.inspectionCosts[this] - minCost) / (maxCost - minCost);
                Gizmos.color = Color.Lerp(Color.green, Color.red, relativeCost);
                Gizmos.DrawSphere(inspectionPoint.inspectionPosition, 0.1f);
            }
        }*/
    }
}

