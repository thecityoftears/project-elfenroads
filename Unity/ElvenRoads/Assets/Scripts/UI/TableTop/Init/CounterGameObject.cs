using System.Collections;
using System.Collections.Generic;
using Elfencore.Shared.GameState;
using UnityEngine;

public class CounterGameObject : MonoBehaviour, MouseDown
{
    public Counter c;
    public Road r;
    public InGameUIController UIController;

    public void OnMouseDown()
    {
        if (UIController.isInExchangeSpell && Game.phase == GamePhase.PlaceCounter)
        {
            UIController.GroundCounterSelected(c, r);
            gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = UnityEngine.Color.red;
            AudioManager.PlaySound("Card");
        }
    }
}