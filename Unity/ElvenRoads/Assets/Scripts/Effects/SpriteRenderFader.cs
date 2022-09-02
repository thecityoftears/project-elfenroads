using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRenderFader : MonoBehaviour
{
    public float speed = 2;
    private SpriteRenderer sr;
 
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }
 
    void Update()
    {
        Color col = sr.color;
        col.a = Mathf.Abs(Mathf.Cos(Time.time * speed));
        sr.color = col;
    }


}
