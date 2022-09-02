using Unity;
using UnityEngine;
using UnityEngine.UI;
using Elfencore.Shared.Messages.ClientToServer;

public class AuctionBidButton : MonoBehaviour
{
    public Slider bidCoinRange;

    public void BidAuction()
    {
        PlaceBid msg = new PlaceBid();
        msg.bid = (int)bidCoinRange.value;
        MessageHandler.Message(msg);
    }
}