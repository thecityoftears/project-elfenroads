using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using UnityEngine.EventSystems;
using Elfencore.Shared.Messages.ClientToServer;

public class WitchSelector : Selector, IPointerDownHandler
{

    public bool selectable = true;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable)
        {
            AudioManager.PlaySound("Card");
            UseWitchForObstacle w = new UseWitchForObstacle();
            w.Road = targetRoad;
            MessageHandler.Message(w);
        }
    }
}