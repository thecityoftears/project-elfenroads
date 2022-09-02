using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Jacob W

/*
Similar to CameraController but allows the player to drag the camera freely
*/
public class CameraDragger : MonoBehaviour
{
    private static Camera cam;

    [SerializeField]
    private Vector3 oriPos = Vector3.zero; //Original Mouse Position in screen space when clicked

    [SerializeField]
    private Vector3 diffPos = Vector3.zero; // Difference in Positions of Mouse in screen space

    //[SerializeField]
    //private float minCamX, maxCamX, minCamY, maxCamY, minCamZ, maxCamZ;

    [SerializeField]
    private float minMapX, maxMapX, minMapZ, maxMapZ; // X, Y bounds

    [SerializeField]
    private MeshRenderer mr;

    [SerializeField]
    private float zoomStep, minCamSize, maxCamSize;

    [SerializeField]
    private float sensitivity;

    // Start is called before the first frame update
    private void Start()
    {
        cam = Camera.main;
        cam.aspect = mr.bounds.extents.x / mr.bounds.extents.z;
        minMapX = mr.bounds.center.x - mr.bounds.extents.x;
        maxMapX = mr.bounds.center.x + mr.bounds.extents.x;
        minMapZ = mr.bounds.center.z - mr.bounds.extents.z;
        maxMapZ = mr.bounds.center.z + mr.bounds.extents.z;
        maxCamSize = (maxMapZ - minMapZ) / 2;
    }

    // Update is called once per frame
    private void Update()
    {
        print(Input.mouseScrollDelta);
        Drag();
        Zoom();
        ClampCamera();
    }

    private void Drag()
    {
        //Get the original Position when the mouse is clicked
        if (Input.GetMouseButtonDown(0))
            oriPos = cam.ScreenToWorldPoint(Input.mousePosition);

        //Updates position once mouse let go
        if (Input.GetMouseButton(0))
        {
            Vector3 curPos = cam.ScreenToWorldPoint(Input.mousePosition);
            diffPos = curPos - oriPos;
            cam.transform.position -= diffPos;
        }
    }

    private void Zoom()
    {
        cam.orthographicSize = Mathf.Clamp(
            cam.orthographicSize - zoomStep * sensitivity * Input.mouseScrollDelta.y,
            minCamSize, maxCamSize);
    }

    private void ClampCamera()
    {
        Vector3 camHeight = cam.orthographicSize * cam.transform.up;
        Vector3 camWidth = cam.orthographicSize * cam.aspect * cam.transform.right;

        float minX = minMapX - camWidth.x;
        float maxX = maxMapX + camWidth.x;

        float minZ = minMapZ - camHeight.z;
        float maxZ = maxMapZ + camHeight.z;

        Vector3 camPos = cam.transform.position;
        camPos = new Vector3(Mathf.Clamp(camPos.x, minX, maxX),
                            camPos.y,
                            Mathf.Clamp(camPos.z, minZ, maxZ));

        cam.transform.position = camPos;
    }

    //=================== Message Functions Called by Zooming Buttons ===========================//
    public void ZoomIn()
    {
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - zoomStep, minCamSize, maxCamSize);
    }

    public void ZoomOut()
    {
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + zoomStep, minCamSize, maxCamSize);
    }
}