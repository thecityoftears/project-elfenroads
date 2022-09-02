using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Elfencore.Shared.GameState;

public class CaravanSelector : MonoBehaviour, IPointerDownHandler
{
    public int cardsNeeded;
    InGameUIController UIController;
    public Road targetRoad;

    void Start()
    {
        UIController = GameObject.FindGameObjectWithTag("UIManager").GetComponent<InGameUIController>();
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        // tell UIManager to display caravan card selector (select the cards desired to use with caravan)
        AudioManager.PlaySound("Card");
        UIController.CaravanSelected(cardsNeeded, targetRoad);
    }
}