using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using UnityEngine.EventSystems;
using Elfencore.Shared.Messages.ClientToServer;

public class CardSelector : MonoBehaviour, IPointerDownHandler
{
    public List<Card> cardsUsedIfSelected = new List<Card>();

    public bool selectable = true;

    public Town src;
    public Town dest;

    public Road road;

    InGameUIController UIController;

    // Start is called before the first frame update
    void Start()
    {
        UIController = GameObject.FindGameObjectWithTag("UIManager").GetComponent<InGameUIController>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable)
        {
            AudioManager.PlaySound("Card");
            // make move boot message request
            TravelOnRoad moveBoot = new TravelOnRoad();
            moveBoot.Cards = new List<Card>();
            foreach (Card c in cardsUsedIfSelected)
            {
                moveBoot.Cards.Add(c);
            }

            moveBoot.Road = road;
            moveBoot.isCaravan = false;
            MessageHandler.Message(moveBoot);
            UIController.cardsForTravelUI.gameObject.SetActive(false);
            // maybe tell UI controller to get rid of popup
        }
    }
}