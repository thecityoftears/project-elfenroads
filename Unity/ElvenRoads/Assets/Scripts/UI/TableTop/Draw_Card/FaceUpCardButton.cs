using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using Elfencore.Shared.Messages.ClientToServer;

public class FaceUpCardButton : MonoBehaviour
{
    public Card faceUpCard;

    public void SelectFaceUp()
    {
        DrawCard msg = new DrawCard();
        msg.Card = faceUpCard;
        MessageHandler.Message(msg);
    }
}