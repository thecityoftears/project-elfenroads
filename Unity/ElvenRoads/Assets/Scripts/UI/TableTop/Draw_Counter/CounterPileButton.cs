using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.Messages.ClientToServer;

public class CounterPileButton : MonoBehaviour
{
    public void SelectPile() {
        DrawRandomCounter msg = new DrawRandomCounter();

        MessageHandler.Message(msg);
    }
}
