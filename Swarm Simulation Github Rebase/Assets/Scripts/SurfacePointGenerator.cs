using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfacePointGenerator : MonoBehaviour   
{
    public class SurfacePoint
    {
        private Vector3 _position;
        public Vector3 position
        {
            get { return _position; }
        }
        private Vector3 _normal;
        public Vector3 normal
        {
            get { return _normal; }
        }

        public bool claimed = false;
        public int visits = 0;

        public SurfacePoint(Vector3 position, Vector3 normal)
        {
            _position = position;
            _normal = normal;
        }
    }
    
    public bool drawSurfacePoints = false;
    public List<SurfacePoint> SurfacePoints = null;
    public List<ProjectionPoint> projectionPoints = new List<ProjectionPoint>();

    public float gridX = 1;
    public float gridY = 1;
    public float gridZ = 1;
    public float distance = 1;
    
    public class ProjectionPoint
    {
        public Vector3 coordinate;
        public Vector3 projectionDirection;
        public float projectionDistance;

        public ProjectionPoint(Vector3 coordinateIn, Vector3 projectionDirectionIN, float projectionDistanceIN)
        {
            coordinate = coordinateIn;
            projectionDirection = projectionDirectionIN;
            projectionDistance = projectionDistanceIN;
        }
    }
    
    void addProjectionPoints(Vector3 direction1, Vector3 direction2, Vector3 direction3, int direction2Amount, int direction3Amount)
    {
        //List<ProjectionPoint> projectionPoints = new List<ProjectionPoint>();
        Vector3 startPoint = direction1 / 2 + direction2 / 2 + direction3 / 2;
        for (var i = 0; i < direction2Amount; i++)
        {
            for (var j = 0; j < direction3Amount; j++)
            {
                Vector3 newPoint = startPoint - direction2 / (direction2Amount-1) * i - direction3 / (direction3Amount-1) * j;
                ProjectionPoint projectionPoint = new ProjectionPoint(newPoint, -direction1, direction1.magnitude);
                projectionPoints.Add(projectionPoint);
            }
        }
    }

    public void CreateSurfacePointsButton()
    {
        CreateSurfacePoints(distance);
    }

    public List<SurfacePoint> CreateSurfacePoints(float pointDistance)
    {
        this.distance = pointDistance;
        SurfacePoints = new List<SurfacePoint>();
        projectionPoints = new List<ProjectionPoint>();
        addProjectionPoints(new Vector3(gridX, 0, 0), new Vector3(0, gridY, 0), new Vector3(0, 0, gridZ), (int)(gridY / distance), (int)(gridZ / distance));
        addProjectionPoints(new Vector3(-gridX, 0, 0), new Vector3(0, -gridY, 0), new Vector3(0, 0, -gridZ), (int)(gridY / distance), (int)(gridZ / distance));
        
        addProjectionPoints(new Vector3(0, gridY, 0), new Vector3(0, 0, gridZ), new Vector3(gridX, 0, 0), (int)(gridZ / distance), (int)(gridX / distance));
        addProjectionPoints(new Vector3(0, -gridY, 0), new Vector3(0, 0, -gridZ), new Vector3(-gridX, 0, 0), (int)(gridZ / distance), (int)(gridX / distance));

        addProjectionPoints(new Vector3(0, 0, gridZ), new Vector3(gridX, 0, 0), new Vector3(0, gridY, 0), (int)(gridX / distance), (int)(gridY / distance));
        addProjectionPoints(new Vector3(0, 0, -gridZ), new Vector3(-gridX, 0, 0), new Vector3(0, -gridY, 0), (int)(gridX / distance), (int)(gridY / distance));


        foreach (ProjectionPoint projectionPoint in projectionPoints)
        {
            //Ray ray = new Ray(projectionPoint.coordinate, projectionPoint.projectionDirection);
            RaycastHit hit;

            if (Physics.Raycast(transform.TransformPoint(projectionPoint.coordinate), transform.TransformPoint(projectionPoint.projectionDirection), out hit, projectionPoint.projectionDistance))
            {
                SurfacePoints.Add(new SurfacePoint(hit.point, hit.normal));
            }
        }


        List<SurfacePoint> surfacePointsToPurge = new List<SurfacePoint>();
        foreach(SurfacePoint surfacePoint in SurfacePoints)
        {
            foreach (SurfacePoint surfacePoint2 in SurfacePoints)
            {
                if (surfacePoint != surfacePoint2)
                {
                    if ((surfacePoint2.position - surfacePoint.position).magnitude <= 0.01f)
                    {
                        surfacePointsToPurge.Add(surfacePoint);
                    }
                }
            }
        }

        foreach (SurfacePoint surfacePointToPurge in surfacePointsToPurge)
        {
            SurfacePoints.Remove(surfacePointToPurge);
        }

        return SurfacePoints;
     }

    private void OnDrawGizmos()
    {
        if (drawSurfacePoints && SurfacePoints != null)
        {
            foreach (SurfacePoint SurfacePoint in SurfacePoints)
            {
                Gizmos.DrawSphere(SurfacePoint.position, 0.1f);
            }
        }

        Gizmos.DrawWireCube(transform.position, new Vector3(gridX, gridY, gridZ));

        /*foreach (ProjectionPoint projectionPoint in projectionPoints)
        {
            Gizmos.DrawSphere(transform.TransformPoint(projectionPoint.coordinate), 0.1f);
            Gizmos.DrawLine(transform.TransformPoint(projectionPoint.coordinate), transform.TransformPoint(projectionPoint.coordinate + projectionPoint.projectionDirection));
        }*/
    }
}

/*
   public class LineBetweenPoints
   {
       public Vector3 point1;
       public Vector3 point2;
       public Vector3 direction;

       public LineBetweenPoints(Vector3 from, Vector3 to)
       {
           point1 = from;
           point2 = to;
           direction = to - from;
       }
   }


   [SerializeField] float GridWidth;
   [SerializeField] float GridHeight;
   [SerializeField] float GridLength;
   [SerializeField] float PointDistance;

   public GameObject inspectionCube;

   private bool SameSide(Vector3 p1, Vector3 p2, Vector3 a, Vector3 b) //https://blackpawn.com/texts/pointinpoly/
   {
       Vector3 cp1 = Vector3.Cross(b - a, p1 - a);
       Vector3 cp2 = Vector3.Cross(b - a, p2 - a);
       if (Vector3.Dot(cp1, cp2) >= 0)
       {
           return true;
       }
       else
       {
           return false;
       }
   }

   private bool PointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
   {
       if (SameSide(p, a, b, c) && SameSide(p, b, a, c) && SameSide(p, c, a, b))
       {
           return true;
       }
       else
       {
           return false;
       }
   }

   //https://stackoverflow.com/questions/48705719/generate-x-amount-of-points-between-two-points
   void linearInterp(Vector3 from, Vector3 to, Vector3[] result, int chunkAmount)
   {
       //divider must be between 0 and 1
       float divider = 1f / chunkAmount;
       float linear = 0f;

       if (chunkAmount == 0)
       {
           Debug.LogError("chunkAmount Distance must be > 0 instead of " + chunkAmount);
           return;
       }

       if (chunkAmount == 1)
       {
           result[0] = Vector3.Lerp(from, to, 0.5f); //Return half/middle point
           return;
       }

       for (int i = 0; i < chunkAmount; i++)
       {
           if (i == 0)
           {
               linear = divider / 2;
           }
           else
           {
               linear += divider; //Add the divider to it to get the next distance
           }
           // Debug.Log("Loop " + i + ", is " + linear);
           result[i] = Vector3.Lerp(from, to, linear);
       }
   }

   private float getRelevantScale(Vector3 direction)
   {
       Vector3 relevantScale = Vector3.Scale(new Vector3(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z)), transform.localScale);
       if (relevantScale.x != 0f)
       {
           return relevantScale.x;
       }
       if (relevantScale.y != 0f)
       {
           return relevantScale.y;
       }
       if (relevantScale.z != 0f)
       {
           return relevantScale.z;
       }
       return 0f;
   }



   public void InspectCube()
   {
       Mesh mesh = inspectionCube.GetComponent<MeshFilter>().mesh;
       Vector3[] vertices = mesh.vertices;
       Vector3[] normals = mesh.normals;
       int[] indices = mesh.triangles;
       int triangleCount = indices.Length / 3;

       //https://answers.unity.com/questions/725081/finding-the-vertices-that-make-a-face.html
       for (int i = 0; i < triangleCount; i++)
       {
           Vector3 V1 = vertices[indices[i * 3]];
           Vector3 V2 = vertices[indices[i * 3 + 1]];
           Vector3 V3 = vertices[indices[i * 3 + 2]];

           LineBetweenPoints Line1 = new LineBetweenPoints(V1, V2);
           LineBetweenPoints Line2 = new LineBetweenPoints(V1, V3);
           LineBetweenPoints Line3 = new LineBetweenPoints(V2, V3);

           LineBetweenPoints aligned1 = null;
           LineBetweenPoints aligned2 = null;
           if (Mathf.Abs(Line1.direction.x) == 1 || Mathf.Abs(Line1.direction.y) == 1 || Mathf.Abs(Line1.direction.z) == 1) //check if L1 aligned with axis, others should be 0
           {
               if (aligned1 == null)
               {
                   aligned1 = Line1;
               }
               else
               {
                   aligned2 = Line1;
               }
           }
           if (Mathf.Abs(Line2.direction.x) == 1 || Mathf.Abs(Line2.direction.y) == 1 || Mathf.Abs(Line2.direction.z) == 1) //check if L1 aligned with axis, others should be 0
           {
               if (aligned1 == null)
               {
                   aligned1 = Line2;
               }
               else
               {
                   aligned2 = Line2;
               }
           }
           if (Mathf.Abs(Line3.direction.x) == 1 || Mathf.Abs(Line3.direction.y) == 1 || Mathf.Abs(Line3.direction.z) == 1) //check if L1 aligned with axis, others should be 0
           {
               if (aligned1 == null)
               {
                   aligned1 = Line3;
               }
               else
               {
                   aligned2 = Line3;
               }
           }

           float scaleAligned1 = getRelevantScale(aligned1.direction);
           float scaleAligned2 = getRelevantScale(aligned2.direction);
           for (var i = 0; i < (int)scaleAligned1; i++)
           {
               for (var j = 0; j < (int)scaleAligned2; j++)
               {
                   Vector3 point = aligned1.point1 + 
               }
           }
       }

       for (var i = 0; i < vertices.Length; i++)
       {
           Debug.Log(vertices[i]);
       }

       for (var i = 0; i < normals.Length; i++)
       {
           Debug.Log(normals[i]);
       }
   }*/