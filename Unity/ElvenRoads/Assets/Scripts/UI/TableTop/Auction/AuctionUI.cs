using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Elfencore.Shared.GameState;
using TMPro;

public class AuctionUI : MonoBehaviour
{
    private List<GameObject> generatedCounters = new List<GameObject>();
    public Slider bidSlider;
    public Button bidButton;
    public Button passButton;

    public TextMeshProUGUI curBidAmountDisplay;
    public TextMeshProUGUI bidAmountDisplay;

    public GameObject counterPrototype;

    private void Start()
    {
        //coins are integer
        bidSlider.wholeNumbers = true;
    }

    public void Update()
    {
        //Update slider number
        bidAmountDisplay.text = " " + bidSlider.value;
    }

    public void UpdateUI()
    {
        if (Game.IsAuctionPhase())
        {
            gameObject.SetActive(true);

            DestroyOldGUI();
            //Update Auction CounterPiles
            GameObject counterGenerated = Instantiate(counterPrototype, counterPrototype.transform.parent);
            counterGenerated.GetComponent<Image>().sprite = UIResources.GetSpriteFor(Game.auction.upForAuction.Peek());

            //Update Bid Option
            UpdateBidOptionUI(Game.auction.currentBid + 1);

            //Update text
            curBidAmountDisplay.text = " Bid: " + Game.auction.currentBid + " Gold";

            if(Game.IsCurrentPlayer(Client.GetLocalPlayer())) {
                bidButton.image.color = UnityEngine.Color.white;
                passButton.image.color = UnityEngine.Color.white;
            }
            else {
                bidButton.image.color = UnityEngine.Color.grey;
                passButton.image.color = UnityEngine.Color.grey;
            }
        }
        else
            gameObject.SetActive(false);
    }

    private void DestroyOldGUI()
    {
        //Destroy auction counters
        foreach (var obj in generatedCounters)
        {
            Destroy(obj);
        }
        generatedCounters.Clear();
    }

    private void UpdateBidOptionUI(int minBidCoins)
    {
        int gold = Client.GetLocalPlayer().amountOfGold();
        bool canBid = gold > minBidCoins;
        bidSlider.gameObject.SetActive(canBid);
        bidButton.gameObject.SetActive(canBid);
        if (canBid)
        {
            bidSlider.minValue = minBidCoins;
            bidSlider.maxValue = gold;
        }
    }
}