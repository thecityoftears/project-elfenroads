using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using Elfencore.Shared.Messages.ClientToServer;
using UnityEngine.UI;

public class ChooseCounterButton : MonoBehaviour
{
    public Counter counterToKeep;
    public EndRoundCounterDisplay ui;

    public void ChooseCounterToKeep()
    {
        ChooseCounterToKeep msg = new ChooseCounterToKeep();
        msg.Counter = counterToKeep;

        MessageHandler.Message(msg);
    }

    public void addCounter()
    {
        this.gameObject.GetComponent<Button>().interactable = false;
        ui.addCounter(counterToKeep);
    }
}
