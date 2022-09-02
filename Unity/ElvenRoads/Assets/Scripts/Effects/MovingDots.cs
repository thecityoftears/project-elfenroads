using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MovingDots : MonoBehaviour
{
    public string baseText;

    public float timeBetweenChanges;

    public int maxDots = 3;

    public TextMeshProUGUI textBox;

    private int numOfDots = 1;

    private float lastUpdateTime;

    void Start() {
        lastUpdateTime = float.MinValue;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time >= lastUpdateTime + timeBetweenChanges) {
            numOfDots = ((numOfDots) % maxDots) + 1;

            string textToPrint = baseText;
            for(int i = 0; i < numOfDots; i++) {
                textToPrint = textToPrint + '.';
            }

            textBox.text = textToPrint;

            lastUpdateTime = Time.time;
        }
        
    }
}
