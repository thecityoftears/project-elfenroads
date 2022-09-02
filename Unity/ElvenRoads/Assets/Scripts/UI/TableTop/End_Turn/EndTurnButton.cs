using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.Messages.ClientToServer;

public class EndTurnButton : MonoBehaviour
{

    public void SendEndTurnButtonRequest()
    {
        // send message to UI
        EndTurn e = new EndTurn();
        MessageHandler.Message(e);
    }
}