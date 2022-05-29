using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTester : MonoBehaviour
{
    Camera camera;

    SurfacePointGenerator surfacePointGenerator;
    List<SurfacePointGenerator.SurfacePoint> surfacePoints;
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        camera.aspect = 1.0f;

        surfacePointGenerator = FindObjectOfType<SurfacePointGenerator>();
        surfacePoints = surfacePointGenerator.CreateSurfacePoints(1);
    }

    private void OnDrawGizmos()
    {
        if (surfacePoints != null)
        {
            foreach (SurfacePointGenerator.SurfacePoint surfacePoint in surfacePoints)
            {
                Vector3 viewPort = camera.WorldToViewportPoint(surfacePoint.position);
                if (viewPort.z > 0
                    && viewPort.z < 10
                    && viewPort.x > 0
                    && viewPort.x < 1
                    && viewPort.y > 0
                    && viewPort.y < 1)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(surfacePoint.position, 0.3f);
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(surfacePoint.position, 0.3f);
                }
            }
        }
    }

}
