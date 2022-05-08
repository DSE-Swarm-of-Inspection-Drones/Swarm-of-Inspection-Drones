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

    public float totalCost;
    bool updatedCosts = false;

    public Transform target;

    List<InspectionPoint> localInspectionPoints = null;

    public bool drawInspectionPoints = false;

    int myIndex;

    public bool inspectionFinished;
    Vector3 startPos;

    // Start is called before the first frame update
    public void StartInspection(InspectionCoordinator inspectionCoordinator, List<InspectionDrone> inspectionDrones)
    {
        GetComponent<Pathfinding.AIDestinationSetter>().target = target;
        this.inspectionCoordinator = inspectionCoordinator;
        this.inspectionDrones = inspectionDrones;
        startPos = transform.position;
        InvokeRepeating("UpdateCosts", transform.GetSiblingIndex() * Random.Range(0.9f, 1.1f)*2, Random.Range(0.9f, 1.1f)); // * 5f
    }

    void FixedUpdate()
    {
        if (inspectionDrones == null)
        {
            return; //Wait untill StartInspection has run for sure.
        }

        myIndex = transform.GetSiblingIndex();
        if (updatedCosts)
        {
            UpdateActivePoint();
        }

        if (activePoint != null)
        {
            target.position = activePoint.inspectionPosition;

            float distanceToActivePoint = (transform.position - activePoint.inspectionPosition).magnitude;
            if (distanceToActivePoint < 5)
            {
                inspectionCoordinator.InspectedPoint(activePoint);
                activePoint = null;
            }

            oldActivePoint = activePoint;
        }
        else
        {
            if (inspectionFinished)
            {
                target.position = startPos;
            }
        }
    }

    void UpdateCosts()
    {
        localInspectionPoints = new List<InspectionPoint>(inspectionCoordinator.uninspectedPoints);
        totalCost = 0;
        foreach (InspectionPoint inspectionPoint in localInspectionPoints)
        {
            //Calculate costs
            //Distance to this drone
            Vector3 pointPosition = inspectionPoint.inspectionPosition;
            Vector3 relativePosition = Vector3.zero;
            if (activePoint != null)
            {
                relativePosition = (pointPosition - activePoint.inspectionPosition);
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
                        distanceToOtherDrones += 1f / Mathf.Pow((drone.activePoint.inspectionPosition - pointPosition).magnitude + 2f, 0.75f);
                    }
                    distanceToOtherDrones += 1f / Mathf.Pow((drone.gameObject.transform.position - pointPosition).magnitude + 2f, 0.75f);
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

            if (localInspectionPoints[i].claimedBy == null)
            {
                activePoint = localInspectionPoints[i];
                localInspectionPoints[i].claimedBy = this;
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

    private void OnTriggerEnter(Collider other)
    {
        inspectionCoordinator.CollisionReport();
        Debug.Log("Collider");
    }

    private void OnCollisionEnter(Collision Collision)
    {
        inspectionCoordinator.CollisionReport();
        Debug.Log("Collision");
    }

    private void OnDrawGizmos()
    {
        if (drawInspectionPoints && localInspectionPoints != null && inspectionFinished == false)
        {
            float maxCost = localInspectionPoints[localInspectionPoints.Count - 1].inspectionCosts[this];
            float minCost = localInspectionPoints[0].inspectionCosts[this];
            foreach (InspectionPoint inspectionPoint in localInspectionPoints)
            {
                float relativeCost = (inspectionPoint.inspectionCosts[this] - minCost) / (maxCost - minCost);
                Gizmos.color = Color.Lerp(Color.green, Color.red, relativeCost);
                Gizmos.DrawSphere(inspectionPoint.inspectionPosition, 0.1f);
            }
        }
    }
}

