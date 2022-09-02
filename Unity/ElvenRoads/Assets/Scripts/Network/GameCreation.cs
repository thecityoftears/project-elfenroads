using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameCreation : MonoBehaviour
{

    public TMP_Dropdown variantSelector;
    public Toggle ELadditionalRound;
    public Toggle RandomDest;
    public Toggle EGWitch;
    public Toggle EGRandomGold;
    public SessionListGetter sessionList;

    public void VariantUpdated()
    {
        if (variantSelector.value == 0)
        { // elfenland
            ELadditionalRound.gameObject.SetActive(true);
            EGRandomGold.gameObject.SetActive(false);
            EGWitch.gameObject.SetActive(false);
        }
        else if (variantSelector.value == 1)
        { // elfengold
            ELadditionalRound.gameObject.SetActive(false);
            EGRandomGold.gameObject.SetActive(true);
            EGWitch.gameObject.SetActive(true);
        }
        else // error
            Debug.Log("ERROR: Selector is in an unexpected state");
    }

    public void CreateGame()
    {
        string[] serviceNameStrings;
        if (variantSelector.value == 0)
        { // elfenland
            serviceNameStrings = new string[3];
            serviceNameStrings[0] = "land";
            serviceNameStrings[1] = ELadditionalRound.isOn ? "4" : "3";
            serviceNameStrings[2] = RandomDest.isOn.ToString().ToLower();
        }
        else if (variantSelector.value == 1)
        { // elfengold
            serviceNameStrings = new string[5];
            serviceNameStrings[0] = "gold";
            serviceNameStrings[1] = "6";
            serviceNameStrings[2] = RandomDest.isOn.ToString().ToLower();
            serviceNameStrings[3] = EGWitch.isOn.ToString().ToLower();
            serviceNameStrings[4] = EGRandomGold.isOn.ToString().ToLower();
        }
        else
        {// error
            Debug.Log("ERROR: Selector is in an unexpected state");
            return;
        }
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(String.Join(':', serviceNameStrings));
        string encodedString = Convert.ToBase64String(plainTextBytes);
        encodedString = encodedString.TrimEnd('=');

        sessionList.CreateGame(encodedString);
    }
}
