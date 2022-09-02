using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.Messages.ClientToServer;

public class PassTurnButton : MonoBehaviour
{

    public void SendPassTurnButtonButtonRequest()
    {
        // send message to UI
        PassTurn e = new PassTurn();
        MessageHandler.Message(e);
    }
}