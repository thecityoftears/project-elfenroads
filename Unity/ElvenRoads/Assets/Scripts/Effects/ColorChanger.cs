using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{

    public float speed = 1;
    private Renderer rend;
 
    void Start()
    {
        rend = GetComponent<Renderer>();
    }
 
    void Update()
    {
        // Assign HSV values to float h, s & v. (Since material.color is stored in RGB)
        float h, s, v;

        Color.RGBToHSV(rend.material.color, out h, out s, out v);

        // Use HSV values to increase H in HSVToRGB. It looks like putting a value greater than 1 will round % 1 it
        rend.material.color = Color.HSVToRGB(h + Time.deltaTime * speed, s, v);
    }

}
