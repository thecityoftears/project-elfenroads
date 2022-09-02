using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.Messages.ClientToServer;

public class CoinButton : MonoBehaviour
{
    /// <summary>
    /// Take all coins instead taking a card
    /// </summary>
    public void SelectCoins()
    {
        TakeGoldDeck msg = new TakeGoldDeck();
        MessageHandler.Message(msg);
    }
}