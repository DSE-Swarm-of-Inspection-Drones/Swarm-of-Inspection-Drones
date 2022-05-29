using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToClosestPoint : MonoBehaviour
{
    float closestDistance = float.MaxValue;
    
    void Update()
    {
        closestDistance = float.MaxValue;
        //Raycast in all directions of a sphere
        float sphereRadius = 10f;
        float resolution = 0.1f;

        //step between radius to zero with resolution
        for (float r = sphereRadius; r > 0; r -= resolution)
        {
            if (Physics.CheckSphere(transform.position, r))
            {
                closestDistance = r;
            }
            else
            {
                break;
            }
        }
        Debug.Log(closestDistance); 
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, closestDistance);
    }
}
