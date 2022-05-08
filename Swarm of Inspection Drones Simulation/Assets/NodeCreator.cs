using UnityEngine;
using System.Collections;

public class NodeCreator : MonoBehaviour
{
    public GameObject Node;
    [SerializeField] float GridWidth;
    [SerializeField] float GridHeight;
    [SerializeField] float GridLength;
    [SerializeField] float PointDistance;

    

    Vector3 startPoint;
    public void CreateNodes()
    {
        int WidthNumber = (int)(GridWidth / PointDistance);
        int HeightNumber = (int)(GridHeight / PointDistance);
        int LengthNumber = (int)(GridLength / PointDistance);

        startPoint = new Vector3(-GridWidth, -GridHeight, -GridLength) / 2f + transform.position; //+ new Vector3(PointDistance, PointDistance, PointDistance) / 2f;
        for (int i = 0; i <= WidthNumber; i++)
        {
            for (int j = 0; j <= HeightNumber; j++)
            {
                for (int k = 0; k <= LengthNumber; k++)
                {
                    Vector3 pos = startPoint + new Vector3(i, j, k) * PointDistance;
                    if (!Physics.CheckBox(pos, Vector3.one * PointDistance / 2f, Quaternion.identity))
                    {
                        Instantiate(Node, pos, Quaternion.identity, transform);
                    }
                }
            }
        }
    }

    public void DeleteNodes()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(GridWidth, GridHeight, GridLength));
    }
}
