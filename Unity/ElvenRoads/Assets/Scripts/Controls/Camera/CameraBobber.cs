using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBobber : MonoBehaviour
{
    public GameObject cameraTarget;

    private float degree = 0.0f;

    public float speed = 5.0f;

    public float distance = 1.0f;

    private Vector3 startPos;

    void Start() {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        degree += Time.deltaTime * speed;

        transform.position = startPos + (Vector3.up * distance * Mathf.Cos(Mathf.Deg2Rad * degree));


        if(cameraTarget != null)
            transform.LookAt(cameraTarget.transform);
        
    }
}
