using Unity;
using UnityEngine;
using UnityEngine.UI;
using Elfencore.Shared.Messages.ClientToServer;

public class AuctionPassButton : MonoBehaviour
{
    public void PassAuction()
    {
        PassTurn msg = new PassTurn();
        MessageHandler.Message(msg);
    }
}