using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.Messages.ClientToServer;

public class ChooseBootButton : MonoBehaviour
{
    public Elfencore.Shared.GameState.Color color;

    public void SendChooseBootRequest()
    {
        // send message to UI
        ChooseBoot cb = new ChooseBoot();
        cb.Color = color;
        MessageHandler.Message(cb);

        transform.parent.parent.gameObject.SetActive(false); // turn off chooseBoot UI (SHOULD PROB CHECK IF REQUEST IS SUCCESSFUL)
    }
}
