using UnityEngine;
using UnityEngine.InputSystem;

// Controls all automatic movements and changes of the camera
public class CameraController : MonoBehaviour
{
    private Camera cam;

    // Camera is zoomed by changing FOV (maxFOV will be furthest zoomed out, set by Camera.FOV attribute, minFOV or most zoomed out is set in editor)
    private float maxFOV;
    public float minFOV = 10.0f;

    public float moveSensitivity = 10.0f;
    public float fastCamModifier = 2.5f;
    private bool fastCam = false;

    public float zoomSensitvity = 10.0f;

    // used to send movement information from OnMove to Update()
    private Vector3 movementVector = new Vector3(0.0f, 0.0f, 0.0f);

    // used to send zoom information from OnZoom to Update()
    private float zoomIntensity = 0.0f;

    private Vector2 startingPos;


    // defines how far in both x and y directions the cam can wander from spawnpoint
    public float xBounds;
    public float yBounds;

    // Start is called before the first frame update
    private void Start()
    {
        // we assume this script is going to be attached to a GameObject with a camera attached
        cam = GetComponent<Camera>();

        maxFOV = cam.fieldOfView;

        // define camera's starting position
        Vector3 pos = gameObject.transform.position;
        startingPos = new Vector2(pos.x, pos.z);
        
    }

    // Update is called once per frame
    private void Update()
    {
        
        // ratio of FOV makes sure we slow down cam movement when we are more zoomed in
        if(fastCam)
            cam.transform.position += Time.deltaTime * fastCamModifier * moveSensitivity * (cam.fieldOfView/maxFOV) * movementVector;
        else
            cam.transform.position += Time.deltaTime * moveSensitivity * (cam.fieldOfView/maxFOV) * movementVector;


        cam.fieldOfView += Time.deltaTime * zoomIntensity;

        ClampCamera();
    }

    // used to move the camera
    public void OnMove(InputValue input) {
        Vector2 inputVec = input.Get<Vector2>();
        movementVector = new Vector3(-inputVec.x, 0.0f, -inputVec.y);

    }

    
    public void OnZoom(InputValue input) {
        zoomIntensity = -input.Get<float>();
    }

    public void OnCamFast(InputValue input) {
        fastCam = input.Get<float>() > 0.0f ? true:false; 
    }

    private void ClampCamera()
    {
        // check FOV
        if(cam.fieldOfView > maxFOV)
            cam.fieldOfView = maxFOV;
        else if(cam.fieldOfView < minFOV)
            cam.fieldOfView = minFOV;

        
        // check bounds
        Vector3 pos = transform.position;
        Vector2 currentPos = new Vector2(pos.x, pos.z);

        // check x bounds
        if(currentPos.x > startingPos.x + xBounds/2)
            currentPos.x = startingPos.x + xBounds/2;
        else if(currentPos.x < startingPos.x - xBounds/2)
            currentPos.x = startingPos.x - xBounds/2;

        // check y bounds
        if(currentPos.y > startingPos.y + yBounds/2)
            currentPos.y = startingPos.y + yBounds/2;
        else if(currentPos.y < startingPos.y - yBounds/2)
            currentPos.y = startingPos.y - yBounds/2;
        
        //update
        transform.position = new Vector3(currentPos.x, pos.y, currentPos.y);
        
    }
}
