using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedStack<T> : LinkedList<T>
{
    //Extending LinkedList<T> to act as a FIFO stack with removable options
    public T Pop()
    {
        T first = First.Value;
        RemoveFirst();
        return first;
    }

    public void Push(T item)
    {
        AddFirst(item);
    }

    //Remove implemented by LinkedList<T>

    //Copy constructor for list (dunno why C# doesn't inherit copy constructors)
    public LinkedStack(List<T> list)
    {
        foreach (T item in list)
        {
            Push(item);
        }
    }
}

public class InspectionPointGenerator : MonoBehaviour
{
    SurfacePointGenerator surfacePointGenerator;
    public bool generateNewPoints = false;

    InspectionCoordinator inspectionCoordinator;
    List<InspectionPoint> inspectionPoints;
    public GameObject inspectionPointPrefab;
    public Transform inspectionPointsParent;

    public float pointDistance = 0.5f;

    [Header("Drone Properties")]
    public float minimumDistance = 1f;
    public float largeDroneDistance = 3f;
    public float smallDroneDistance = 2f;

    [Header("Inspection Properties")]
    public float maxOffNormalDeg = 80f;
    public float angResDeg = 10f;
    public float distanceWeight = 1f;
    public float offNormalWeight = 1f;
    public float closestDistanceWeightLarge = 5f;
    public float closestDistanceWeightSmall = 1f;
    public float angleWithHorizontalWeight = 1f;
    public float largeDroneCostFactor = 1f;

    LinkedStack<SurfacePointGenerator.SurfacePoint> surfacePointStack;
    List<SurfacePointGenerator.SurfacePoint> surfacePoints;

    [Header("Camera Properties")]
    public GameObject largeDroneCameraPrefab;
    public Vector2 viewportRange = new Vector2(0.1f, 0.9f);
    public Vector2 largeDroneCamRange = new Vector2(0f, 4f);

    public GameObject smallDroneCameraPrefab;
    public Vector2 smallDroneCamRange = new Vector2(0f, 2.5f);



    private void Start()
    {
        if (generateNewPoints)
        {
            /*Application.targetFrameRate = -1;   // No maximum frame rate to sleep on
            QualitySettings.vSyncCount = 0;     // No GPU vSync
            Time.captureFramerate = 25;         // Simulate 30 FPS (TODO consider adjusting this for your project)*/

            inspectionPoints = new List<InspectionPoint>();
            surfacePointGenerator = GetComponent<SurfacePointGenerator>();
            surfacePoints = surfacePointGenerator.CreateSurfacePoints(pointDistance);
            surfacePointStack = new LinkedStack<SurfacePointGenerator.SurfacePoint>(surfacePoints);
        }
    }
    /*foreach (SurfacePointGenerator.SurfacePoint surfacePoint in surfacePoints)
    {
        InspectionPoint temp = new InspectionPoint(surfacePoint);
        inspectionPoints.Add(temp);
    }

    List<InspectionPoint> inspectionPointsToRemove = new List<InspectionPoint>();*/



    void Update()
    {
        if (generateNewPoints)
        {
            if (surfacePointStack.Count == 0)
            {
                return;
            }

            //Go through surfacepoints and select one to promote to inspection point, use this to eliminate other surface points using camera.
            //Repeat untill no surfacepoints left
            SurfacePointGenerator.SurfacePoint surfacePointToCheck = surfacePointStack.Pop();
            GameObject inspectionPointObj = Instantiate(inspectionPointPrefab, inspectionPointsParent);
            inspectionPointObj.transform.position = surfacePointToCheck.position;
            InspectionPoint inspectionPoint = inspectionPointObj.GetComponent<InspectionPoint>();
            inspectionPoint.SetInspectionPoint(surfacePointToCheck);
            bool succesfulDronePoint = GetDronePoint(inspectionPoint);
            if (succesfulDronePoint)
            {
                
                (bool succesfullVisiblePoints, List<SurfacePointGenerator.SurfacePoint> visiblePoints) = ApplyCameraAndGetVisiblePoints(surfacePoints, inspectionPoint);
                if (surfacePointToCheck.visits <= 2)
                {
                    if (succesfullVisiblePoints)
                    {
                        foreach (SurfacePointGenerator.SurfacePoint visiblePoint in visiblePoints)
                        {
                            visiblePoint.claimed = true;
                            surfacePointStack.Remove(visiblePoint);
                        }
                    }
                    else
                    {
                        surfacePointStack.AddLast(surfacePointToCheck);
                        surfacePointToCheck.visits++;
                        Destroy(inspectionPoint.gameObject);
                    }
                }
                else //add irregardles of overlap
                {
                    foreach (SurfacePointGenerator.SurfacePoint visiblePoint in visiblePoints)
                    {
                        visiblePoint.claimed = true;
                        surfacePointStack.Remove(visiblePoint);
                    }
                }
            }
            else
            {
                //TODO: implement inspectionpoint with unsuccesful dronePoint
                Destroy(inspectionPoint.gameObject);
            }
        }
    }

    (bool, List<SurfacePointGenerator.SurfacePoint>) ApplyCameraAndGetVisiblePoints(List<SurfacePointGenerator.SurfacePoint> surfacePointsToCheck, InspectionPoint inspectionPoint)
    {
        //check surfacePoints from inspectionPoint
        GameObject cameraObj;
        Camera camera;
        Vector2 camRange;
        if (inspectionPoint.largeDrone) {
            cameraObj = Instantiate(largeDroneCameraPrefab, inspectionPoint.gameObject.transform);
            camera = cameraObj.GetComponent<Camera>();
            camRange = largeDroneCamRange;
        }
        else
        {
            cameraObj = Instantiate(smallDroneCameraPrefab, inspectionPoint.gameObject.transform);
            camera = cameraObj.GetComponent<Camera>();
            camRange = smallDroneCamRange;
        }

        cameraObj.transform.position = inspectionPoint.dronePosition;
        cameraObj.transform.LookAt(inspectionPoint.inspectionPosition);
        camera.aspect = 16f / 9f;
        List<SurfacePointGenerator.SurfacePoint> visiblePoints = new List<SurfacePointGenerator.SurfacePoint>();
        foreach (SurfacePointGenerator.SurfacePoint surfacePointToCheck in surfacePointsToCheck)
        {
            Vector3 surfacePointInViewport = camera.WorldToViewportPoint(surfacePointToCheck.position);
            Vector3 toSurfacePoint = (surfacePointToCheck.position - inspectionPoint.dronePosition);
            float distance = toSurfacePoint.magnitude;

            if (surfacePointInViewport.z > camRange.x
                && surfacePointInViewport.z < camRange.y
                && surfacePointInViewport.x > viewportRange.x
                && surfacePointInViewport.x < viewportRange.y
                && surfacePointInViewport.y > viewportRange.x
                && surfacePointInViewport.y < viewportRange.y)
            {

                float angle = Mathf.Abs(Vector3.Angle(-camera.transform.forward, surfacePointToCheck.normal));
                bool obstruction = Physics.Raycast(inspectionPoint.dronePosition, toSurfacePoint.normalized, distance * 0.95f);
                if (!obstruction && angle < 45f)
                {
                    visiblePoints.Add(surfacePointToCheck);
                }
            }
            else
            {

            }
        }

        int claimedPoints = 0;
        foreach (SurfacePointGenerator.SurfacePoint visiblePoint in visiblePoints)
        {
            if (visiblePoint.claimed)
            {
                claimedPoints++;
            }
        }

        float claimedRatio = claimedPoints / (float)(visiblePoints.Count);
        if (claimedRatio < 0.2f)
        {
            return (true, visiblePoints);
        }
        else
        {
            return (false, visiblePoints);
        }
    }

    bool GetDronePoint(InspectionPoint inspectionPoint)
    {
        List<PotentialDronePoint> potentialDronePoints = new List<PotentialDronePoint>();
        Vector3 ones = Vector3.one;
        Vector3 perpendicular = Vector3.Cross(ones, inspectionPoint.normal);

        for (float offNormal = 0; offNormal < maxOffNormalDeg; offNormal += angResDeg)
        {
            Vector3 offNormalVector = Quaternion.AngleAxis(offNormal, perpendicular) * inspectionPoint.normal;
            for (float rotation = 0; rotation < 360; rotation += angResDeg * 2)
            {
                Vector3 directionVector = Quaternion.AngleAxis(rotation, inspectionPoint.normal) * offNormalVector;
                //for i between 0 and 1 with steps of 0.05
                for (float i = 0f; i < 1; i += 0.1f)
                {
                    float distanceFromInspectionPoint = i * largeDroneDistance * 1.2f;
                    Vector3 pointToInvestigate = inspectionPoint.inspectionPosition + directionVector.normalized * distanceFromInspectionPoint;
                    float closestDistance = ClosestDistanceToOtherCollidersInSphere(pointToInvestigate, 10f);

                    Vector3 toInvestigatedPoint = pointToInvestigate - inspectionPoint.inspectionPosition;
                    bool canNotSeeInspectionPoint = Physics.Raycast(inspectionPoint.inspectionPosition, toInvestigatedPoint.normalized, toInvestigatedPoint.magnitude * 0.95f);
                    if (closestDistance > minimumDistance & !canNotSeeInspectionPoint)
                    {
                        //calculate costs
                        float angleWithHorizontal = Mathf.Atan2(-directionVector.y, -directionVector.x);
                        float angleWithHorizontalCost = 0;
                        if (angleWithHorizontal > 0)
                        {
                            angleWithHorizontalCost = angleWithHorizontal / Mathf.PI / 2 * 4f;
                        }

                        float largeDroneCost = Mathf.Abs(largeDroneDistance - distanceFromInspectionPoint) / largeDroneDistance * distanceWeight
                            + offNormal * offNormalWeight
                            + 1 / closestDistance * closestDistanceWeightLarge
                            + angleWithHorizontalCost * angleWithHorizontalWeight;
                        Dictionary<string, float> largeDroneCosts = new Dictionary<string, float>() {
                            {"OptimalDist", Mathf.Abs(largeDroneDistance - distanceFromInspectionPoint) },
                            {"offNormal", offNormal },
                            {"closestDistance", closestDistance },
                            {"angleWithHorizontal", angleWithHorizontalCost }
                        };

                        float smallDroneCost = Mathf.Abs(smallDroneDistance - distanceFromInspectionPoint) / smallDroneDistance * distanceWeight
                            + offNormal * offNormalWeight
                            + 1 / closestDistance * closestDistanceWeightSmall;
                        Dictionary<string, float> smallDroneCosts = new Dictionary<string, float>() {
                            {"OptimalDist", Mathf.Abs(smallDroneDistance - distanceFromInspectionPoint) },
                            {"offNormal", offNormal },
                            {"closestDistance", closestDistance }
                        };

                        potentialDronePoints.Add(new PotentialDronePoint(pointToInvestigate, largeDroneCost, smallDroneCost, largeDroneCosts, smallDroneCosts));
                    }
                    else
                    {
                        /*if (!canNotSeeInspectionPoint)
                        {
                            Debug.Log(closestDistance);
                        }*/
                    }
                }
            }
        }

        if (potentialDronePoints.Count == 0)
        {
            return false;
        }
        else
        {
            //copy potentialDronePoints and sort by cost for large drones
            List<PotentialDronePoint> potentialDronePointsSortedLarge = new List<PotentialDronePoint>(potentialDronePoints);
            potentialDronePointsSortedLarge.Sort((a, b) => a.largeDroneCost.CompareTo(b.largeDroneCost));

            //copy potentialDronePoints and sort by cost for small drones
            List<PotentialDronePoint> potentialDronePointsSortedSmall = new List<PotentialDronePoint>(potentialDronePoints);
            potentialDronePointsSortedSmall.Sort((a, b) => a.smallDroneCost.CompareTo(b.smallDroneCost));

            //investigate best scores
            float bestLargeCost = potentialDronePointsSortedLarge[0].largeDroneCost * largeDroneCostFactor;
            float bestSmallCost = potentialDronePointsSortedSmall[0].smallDroneCost;

            bool largeDrone = false;
            bool smallDrone = false;
            if (bestLargeCost < bestSmallCost)
            {
                largeDrone = true;
                //inspectionPoint.dronePosition = potentialDronePointsSortedLarge[0].position;
            }
            else
            {
                smallDrone = true;
                //inspectionPoint.dronePosition = potentialDronePointsSortedSmall[0].position;
            }

            //select the best points for large and small drones
            //TODO: Move this to InspectionPoint
            inspectionPoint.largeDronePosition = potentialDronePointsSortedLarge[0].position;
            inspectionPoint.largeDrone = largeDrone;
            inspectionPoint.largePotential = potentialDronePointsSortedLarge[0];
            inspectionPoint.smallDronePosition = potentialDronePointsSortedSmall[0].position;
            inspectionPoint.smallDrone = smallDrone;
            inspectionPoint.smallPotential = potentialDronePointsSortedSmall[0];
            return true;
        }
    }

    public class PotentialDronePoint {
        public Vector3 position;
        public float largeDroneCost;
        public float smallDroneCost;
        public Dictionary<string, float> largeDroneCosts;
        public Dictionary<string, float> smallDroneCosts;
        
        public PotentialDronePoint(Vector3 position, float largeDroneCost, float smallDroneCost, Dictionary<string, float> largeDroneCosts, Dictionary<string, float> smallDroneCosts)
        {
            this.position = position;
            this.largeDroneCost = largeDroneCost;
            this.smallDroneCost = smallDroneCost;
            this.largeDroneCosts = largeDroneCosts;
            this.smallDroneCosts = smallDroneCosts;
        }
    }

    float ClosestDistanceToOtherCollidersInSphere(Vector3 sphereCenter, float Radius, float resolution = 0.1f)
    {
        float closestDistance = float.MaxValue;
        //Raycast in all directions of a sphere
        float sphereRadius = Radius;

        //step between radius to zero with resolution
        for (float r = sphereRadius; r > 0; r -= resolution)
        {
            if (Physics.CheckSphere(sphereCenter, r))
            {
                closestDistance = r;
            }
            else
            {
                break;
            }
        }

        return closestDistance;
    }
}

/* if point too much overlap, add back to end of stack
 * 
/*
    public List<InspectionPoint> GetInspectionPoints(float pointDistance, float switchDroneDistance, float largeDroneDistance, float minimumDistance)

    class PotentialDronePoint{
        public Vector3 position;
        public float largeDroneCost;
        public float smallDroneCost;

        public PotentialDronePoint(Vector3 position, float largeDroneCost, float smallDroneCost)
        {
            this.position = position;
            this.largeDroneCost = largeDroneCost;
            this.smallDroneCost = smallDroneCost;       
        }
    }

    public List<InspectionPoint> GetInspectionPoints(float pointDistance, float smallDroneDistance, float largeDroneDistance, float minimumDistance)
    {
        // switchDroneDistance needs to be 2x larger than minimumDistance
        inspectionPoints = new List<InspectionPoint>();
        surfacePointGenerator = GetComponent<SurfacePointGenerator>();
        List< SurfacePointGenerator.SurfacePoint> surfacePoinst = surfacePointGenerator.CreateSurfacePoints(pointDistance);
        foreach (SurfacePointGenerator.SurfacePoint surfacePoint in surfacePoinst)
        {
            InspectionPoint temp = new InspectionPoint(surfacePoint);
            inspectionPoints.Add(temp);
        }

        //Create dronePosition
        List<InspectionPoint> inspectionPointsToRemove = new List<InspectionPoint>();

        int counter = 0;
        foreach (InspectionPoint inspectionPoint in inspectionPoints)
        {
            //Debug incrementing counter
            //Debug.Log(counter);
            counter++;
            
            List<PotentialDronePoint> potentialDronePoints = new List<PotentialDronePoint>();
            Vector3 ones = Vector3.one;
            Vector3 perpendicular = Vector3.Cross(ones, inspectionPoint.normal);
            
            for (float offNormal = 0; offNormal < maxOffNormalDeg; offNormal += angResDeg)
            {
                Vector3 offNormalVector = Quaternion.AngleAxis(offNormal, perpendicular) * inspectionPoint.normal;
                for (float rotation = 0; rotation < 360; rotation += angResDeg*2)
                {
                    Vector3 directionVector = Quaternion.AngleAxis(rotation, inspectionPoint.normal) * offNormalVector;
                    //for i between 0 and 1 with steps of 0.05
                    for (float i = 0f; i < 1; i += 0.1f)
                    {
                        float distanceFromInspectionPoint = i * largeDroneDistance * 1.2f;
                        Vector3 pointToInvestigate = inspectionPoint.inspectionPosition + directionVector.normalized * distanceFromInspectionPoint;
                        float closestDistance = ClosestDistanceToOtherCollidersInSphere(pointToInvestigate, 10f);

                        Vector3 toInvestigatedPoint = pointToInvestigate - inspectionPoint.inspectionPosition;
                        bool canNotSeeInspectionPoint = Physics.Raycast(inspectionPoint.inspectionPosition, toInvestigatedPoint.normalized, toInvestigatedPoint.magnitude*0.95f);
                        if (closestDistance > minimumDistance &!canNotSeeInspectionPoint)
                        {
                            //calculate costs
                            float angleWithHorizontal = Mathf.Atan2(-directionVector.y, -directionVector.x);
                            float angleWithHorizontalCost = 0;
                            if (angleWithHorizontal > 0)
                            {
                                angleWithHorizontalCost = angleWithHorizontal/Mathf.PI/2*4f;
                            }
                            
                            float largeDroneCost = Mathf.Abs(largeDroneDistance - distanceFromInspectionPoint) / largeDroneDistance * distanceWeight
                                + offNormal / maxOffNormalDeg * offNormalWeight
                                + 5/closestDistance
                                + angleWithHorizontalCost;
                            float smallDroneCost = Mathf.Abs(smallDroneDistance - distanceFromInspectionPoint) / smallDroneDistance * distanceWeight
                                + offNormal / maxOffNormalDeg * offNormalWeight
                                + 1/closestDistance;
                            
                            potentialDronePoints.Add(new PotentialDronePoint(pointToInvestigate, largeDroneCost, smallDroneCost));
                        }
                        else
                        {
                            /*if (!canNotSeeInspectionPoint)
                            {
                                Debug.Log(closestDistance);
                            }*/
             /*         }
                    }
                }
                
                if (potentialDronePoints.Count == 0)
                {
                    inspectionPointsToRemove.Add(inspectionPoint);
                }
                else
                {
                    //copy potentialDronePoints and sort by cost for large drones
                    List<PotentialDronePoint> potentialDronePointsSortedLarge = new List<PotentialDronePoint>(potentialDronePoints);
                    potentialDronePointsSortedLarge.Sort((a, b) => a.largeDroneCost.CompareTo(b.largeDroneCost));

                    //copy potentialDronePoints and sort by cost for small drones
                    List<PotentialDronePoint> potentialDronePointsSortedSmall = new List<PotentialDronePoint>(potentialDronePoints);
                    potentialDronePointsSortedSmall.Sort((a, b) => a.smallDroneCost.CompareTo(b.smallDroneCost));

                    //select the best points for large and small drones
                    inspectionPoint.largeDronePosition = potentialDronePointsSortedLarge[0].position;
                    inspectionPoint.smallDronePosition = potentialDronePointsSortedSmall[0].position;
                }*/
                /*float[,] distanceArray = new float[100, 2];
                List<float[]> distanceArray2 = new List<float[]>();
                //for i between 0 and 1 with steps of 0.01
                for (float i = 0; i < 1; i += 0.1f)
                {
                    Vector3 pointToInvestigate = inspectionPoint.inspectionPosition + inspectionPoint.normal.normalized * i * largeDroneDistance;
                    float closestDistance = ClosestDistanceToOtherCollidersInSphere(pointToInvestigate, switchDroneDistance);
                    distanceArray[(int)(i * 100), 0] = i;
                    distanceArray[(int)(i * 100), 1] = closestDistance;

                    distanceArray2.Add(new float[2] { i, closestDistance });
                }

                //Sort distanceArray2 by closest distance
                distanceArray2.Sort(delegate (float[] x, float[] y)
                {
                    return x[1].CompareTo(y[1]);
                });

                Debug.Log("Sorted");*/
                // Sort distanceArray by closest distance using Linq
                //var sortedArray = distanceArray.OrderBy(x => x[1].Value);
  /*          }
        }

            

            foreach (InspectionPoint inspectionPoint in inspectionPointsToRemove)
        {
            inspectionPoints.Remove(inspectionPoint);
        }

        return inspectionPoints;
    }
*/


/*foreach (InspectionPoint inspectionPoint in inspectionPoints)
            {
                //Check for collisions along the inspectionPoint normal
                RaycastHit hit;
                if (Physics.Raycast(inspectionPoint.inspectionPosition, inspectionPoint.normal, out hit))
                {
                    float toHitDistance = (hit.point - inspectionPoint.inspectionPosition).magnitude;

                    //------------|minimum|----o-----------------|switchDrone|----------O--------------------------------------|largeDrone|
                    //-------------------------^Smalldrone if collider < switchdrone----^Largedrone if collider < largeDrone-------^Largedrone if collider > largeDrone
                    //switchDrone > 2* minimum
                    if (toHitDistance < switchDroneDistance)
                    {
                        inspectionPoint.smallDrone = true;
                        float inspectionDistance = toHitDistance * 0.5f;
                        if (inspectionDistance < minimumDistance)
                        {
                            inspectionPointsToRemove.Add(inspectionPoint);
                        }
                        else
                        {
                            Vector3 sphereCenter = inspectionPoint.inspectionPosition + inspectionPoint.normal.normalized * inspectionDistance;
                            float distanceToCollider = ClosestDistanceToOtherCollidersInSphere(sphereCenter, minimumDistance + 10f);

                            if (distanceToCollider < minimumDistance)
                            {
                                inspectionPointsToRemove.Add(inspectionPoint);
                            }
                            else
                            {
                                inspectionPoint.dronePosition = inspectionPoint.inspectionPosition + inspectionPoint.normal.normalized * inspectionDistance;
                            }
                        }                        
                    }
                    else if (toHitDistance < largeDroneDistance + switchDroneDistance)
                    {
                        Vector3 sphereCenter = inspectionPoint.inspectionPosition + inspectionPoint.normal.normalized * toHitDistance * 0.5f;
                        float distanceToCollider = ClosestDistanceToOtherCollidersInSphere(sphereCenter, minimumDistance + 10f);

                        if (distanceToCollider < minimumDistance)
                        {
                            inspectionPointsToRemove.Add(inspectionPoint);
                        }
                        if (distanceToCollider < switchDroneDistance)
                        {
                            inspectionPoint.smallDrone = true;
                            inspectionPoint.dronePosition = inspectionPoint.inspectionPosition + inspectionPoint.normal.normalized * toHitDistance * 0.5f;
                        }
                        else
                        {
                            inspectionPoint.largeDrone = true;
                            inspectionPoint.dronePosition = inspectionPoint.inspectionPosition + inspectionPoint.normal.normalized * toHitDistance * 0.5f;
                        }
                    }
                    else
                    {
                        Vector3 sphereCenter = inspectionPoint.inspectionPosition + inspectionPoint.normal.normalized * largeDroneDistance;
                        float distanceToCollider = ClosestDistanceToOtherCollidersInSphere(sphereCenter, minimumDistance + 10f);

                        if (distanceToCollider < minimumDistance)
                        {
                            inspectionPointsToRemove.Add(inspectionPoint);
                        }
                        if (distanceToCollider < switchDroneDistance)
                        {
                            inspectionPoint.smallDrone = true;
                            inspectionPoint.dronePosition = inspectionPoint.inspectionPosition + inspectionPoint.normal.normalized * largeDroneDistance;
                        }
                        else
                        {
                            inspectionPoint.largeDrone = true;
                            inspectionPoint.dronePosition = inspectionPoint.inspectionPosition + inspectionPoint.normal.normalized * largeDroneDistance;
                        }                    
                     }
                }
                else
                {
                    inspectionPoint.largeDrone = true;
                    inspectionPoint.dronePosition = inspectionPoint.inspectionPosition + inspectionPoint.normal.normalized * largeDroneDistance;
                }
            }*/
            //}*/

         /*   foreach (InspectionPoint inspectionPoint in inspectionPointsToRemove)
        {
            inspectionPoints.Remove(inspectionPoint);
        }

        return inspectionPoints;
    }
}
     /*       }*/

    /*    foreach (InspectionPoint inspectionPoint in inspectionPointsToRemove)
        {
            inspectionPoints.Remove(inspectionPoint);
        }

        return inspectionPoints;
    }
}*/
