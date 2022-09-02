using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{

    public float speed = 1;
    private Image image;
 
    void Start()
    {
        image = GetComponent<Image>();
    }
 
    void Update()
    {
        Color col = image.color;
        col.a = Mathf.Abs(Mathf.Cos(Time.time * speed));
        image.color = col;
    }

}
