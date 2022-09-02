using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.Messages.ClientToServer;

public class DeckButton : MonoBehaviour
{
    public void SelectCard()
    {
        DrawRandomCard msg = new DrawRandomCard();
        MessageHandler.Message(msg);
    }
}