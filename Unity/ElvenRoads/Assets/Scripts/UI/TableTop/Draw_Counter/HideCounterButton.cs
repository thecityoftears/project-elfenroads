using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using Elfencore.Shared.Messages.ClientToServer;

public class HideCounterButton : MonoBehaviour
{
    public Counter faceUpCounter;

    public void SelectHide()
    {
        ChooseCounterToMakeHidden msg = new ChooseCounterToMakeHidden();
        msg.Invis = faceUpCounter;

        MessageHandler.Message(msg);
    }
}
