using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ErrorPopup : MonoBehaviour
{
    public TextMeshProUGUI tmp;
    public void DisplayText(string text) {
        tmp.text = "Error: " + text;
    }

}
