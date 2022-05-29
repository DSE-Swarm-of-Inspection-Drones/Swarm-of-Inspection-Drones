using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    Camera camera;

    [Header("Rotation")]
    public float mouseSensitivity = 10;
    public Transform target;
    public float dstTarget = 20;

    float yaw;
    float pitch;

    public Vector3 offset;

    int escLog = 0;

    bool rotationLock;

    [Header("Translation")]
    public float lookSpeedH = 2f;
    public float lookSpeedV = 2f;
    public float startZoomSpeed = 1000f;
    public float startDragSpeed = 20000f;

    float zoomSpeed;
    float zoomlevel;
    float mouseCollective;
    public float zoomlevelStart = 2;
    public float minZoom = 0;
    public float maxZoom = 50;
    bool zoomLocked;

    float dragSpeed;

    // Update is called once per frame

    void Start()
    {
        camera = GetComponent<Camera>();
        offset = new Vector3(0, 2, 0);
        //Cursor.lockState = CursorLockMode.Locked;
        zoomlevel = zoomlevelStart;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Transform objectHit = hit.transform;
                Tags objectTagsScript = objectHit.gameObject.GetComponent<Tags>();
                if (objectTagsScript != null)
                {
                    if (objectTagsScript.HasTag("Targetable"))
                    {
                        target = objectHit;
                    }
                }
            }
        }
        if (Input.GetMouseButton(2))
        {
            target = null;
        }

        mouseCollective += -1 * Input.GetAxis("Mouse ScrollWheel");
        zoomLocked = false;

        if (mouseCollective < minZoom)
        {
            zoomLocked = true;
            zoomlevel = minZoom;
        }
        if (mouseCollective > maxZoom)
        {
            zoomLocked = true;
            zoomlevel = maxZoom;
        }

        zoomSpeed = (zoomlevel / maxZoom) * startZoomSpeed;
        dragSpeed = (zoomlevel / maxZoom) * startDragSpeed;

        if (target != null)
        {
            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    rotationLock = true;
                    escLog += 1;
                    if (escLog == 2)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        rotationLock = false;
                        escLog = 0;
                    }
                }

                if (!rotationLock)
                {
                    Vector3 targetRotation = new Vector3(pitch, yaw);
                    transform.eulerAngles = targetRotation;
                }
            }
            transform.position = target.position + offset - transform.forward * dstTarget;

        }
        else
        {
            //Look around with Right Mouse
            if (Input.GetMouseButton(1))
            {
                yaw += lookSpeedH * Input.GetAxis("Mouse X");
                pitch -= lookSpeedV * Input.GetAxis("Mouse Y");

                transform.eulerAngles = new Vector3(pitch, yaw, 0f);
            }

            //drag camera around with Middle Mouse
            if (Input.GetMouseButton(2))
            {
                transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * dragSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * dragSpeed, 0);
            }

            //Zoom in and out with Mouse Wheel
            if (!zoomLocked)
            {
                transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, Space.Self);
            }
        }
    }
}

