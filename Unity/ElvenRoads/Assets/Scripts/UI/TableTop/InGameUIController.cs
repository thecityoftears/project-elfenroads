using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using Elfencore.Shared.Messages.ClientToServer;

/// <summary> Main hub for dispatching UI events </summary>
public class InGameUIController : MonoBehaviour
{
    public CardDisplay heldCardUI;
    public CounterDisplay heldCounterUI;
    public BootUIManager bootUI;
    public TownUIManager townUI;
    public RoadUIManager roadUI;
    public CardToTravelPopup cardsForTravelUI;
    public EndGameDisplay endOfGameUI;
    public BootSelectionUI bootSelectionUI;
    public RoundDisplay displayRoundUI;
    public EndRoundCounterDisplay endRoundCounterUI;
    public DrawCardUI drawCardUI;
    public HideCounterUI hideCounterUI;
    public DrawCounterUI drawCounterUI;
    public AuctionUI auctionUI;
    public PlayerPanelScript playerPanelUI;
    public OpponentUI opponentUI;
    public GameObject WaitingScreen;
    public ErrorPopup errorPopup;

    public GameObject endTurnButton;
    public GameObject passTurnButton;

    public GameObject keepGoldUI;

    public KeepGoldButton goldButton;

    private Road selectedRoad = null;
    private int cardsToBePicked = int.MinValue;
    private Road targetRoad = null;
    private List<CardPressedButton> cardsForCaravan = new List<CardPressedButton>();

    #region Double Spell Process

    private bool isInDoubleSpell;
    private Counter counter2Place;

    #endregion Double Spell Process

    #region Exchange Spell Process

    [HideInInspector]
    public bool isInExchangeSpell;
    private Counter counter1;
    private Road road1;
    private Road road2;
    private Counter counter2;

    #endregion Exchange Spell Process

    public bool usingWitch = false;

    /// <summary> Used at the beginning of the game to setup all game elements. GameState should already be setup at this point </summary>
    public void SetupGameUI()
    {
        AudioManager.PlaySound("GameMusic");
        townUI.SetupUI(this);
        roadUI.SetupUI(this);
        bootUI.SetupUI(this);
        opponentUI.SetupUI();
        bootSelectionUI.SetupUI();
        WaitingScreen.SetActive(false);
        UpdateUI();
    }

    /// <summary> Used whenever there is a UI update that changes the GameState </summary>
    public void UpdateUI()
    {
        Debug.Log(Game.winnerDeclared);
        if (Game.winnerDeclared)
        {
            endOfGameUI.DisplayWinner(Game.winner);
            endOfGameUI.gameObject.SetActive(true);
        }
        isInExchangeSpell = false;

        // Update always-on-screen-UI
        playerPanelUI.UpdateUI();
        bootUI.UpdateUI();
        roadUI.UpdateUI();
        heldCardUI.UpdateUI();
        heldCounterUI.UpdateUI();
        townUI.UpdateUI();
        opponentUI.UpdateUI();
        displayRoundUI.UpdateUI();
        goldButton.UpdateUI();

        // close boot selection if a color has been chosen
        if (Client.GetLocalPlayer().selectedBoot)
        {
            bootSelectionUI.gameObject.SetActive(false);
        }

        //Check Phase
        Debug.Log(Game.phase);

        hideCounterUI.UpdateUI();
        bootSelectionUI.UpdateUI();
        drawCardUI.UpdateUI();
        drawCounterUI.UpdateUI();
        auctionUI.UpdateUI();
        endRoundCounterUI.UpdateUI();
        cardsForTravelUI.gameObject.SetActive(false);

        if (Game.IsCurrentPlayer(Client.GetLocalPlayer()) && Game.phase == GamePhase.PlaceCounter)
        {
            passTurnButton.SetActive(true);
        }
        else
        {
            passTurnButton.SetActive(false);
        }

        if (Game.IsCurrentPlayer(Client.GetLocalPlayer()) && Game.phase == GamePhase.MoveBoot)
        {
            endTurnButton.SetActive(true);
        }
        else
        {
            endTurnButton.SetActive(false);
        }

        if (Game.finishedPhase.FindIndex(p => p.GetName() == Client.GetLocalPlayer().GetName()) == -1 && Game.phase == GamePhase.EndOfMoveBoot)
        {
            keepGoldUI.SetActive(true);
        }
        else
        {
            keepGoldUI.SetActive(false);
        }

        if (Game.phase != GamePhase.MoveBoot)
        {
            cardsForTravelUI.gameObject.SetActive(false);
        }
    }

    /// <summary> Called when a town is clicked and a set of cards needs to be selected </summary>
    public void DisplayCardSelection(Town dest)
    {
        if (Game.phase == GamePhase.MoveBoot)
        {
            Player localPlayer = Client.GetLocalPlayer();
            Town source = localPlayer.GetLocation();
            cardsForTravelUI.gameObject.SetActive(true);
            cardsForTravelUI.DisplayOptionsFor(source, dest);
        }

        // Will probably create a Module in the Canvas that will receive the town and determine what cards to display
    }

    /// <summary> Called when a road is clicked and a counter needs to be selected to place </summary>
    public void RoadSelected(Road r)
    {
        // tell that there is a road selected
        selectedRoad = r;
        heldCounterUI.CountersCanBeClicked(r, isInDoubleSpell); // need to start accepting input from CounterUI

        // TODO: maybe create a display to CounterDisplay that it is time select a counter
    }

    /// <summary> Used to flag that a caravan has been chosen and cards must be selected </summary>
    public void CaravanSelected(int cardsNeeded, Road targetRoad)
    {
        heldCardUI.CardsCanBeClicked(true);
        cardsToBePicked = cardsNeeded;
        cardsForCaravan.Clear();
        this.targetRoad = targetRoad;
    }

    private void DoubleSpellCounterSelected(Counter c)
    {
        void ClearDoubleSpellData()
        {
            counter2Place = null;
            isInDoubleSpell = false;
        }

        //Update the road
        selectedRoad = Game.GetRoad(selectedRoad.source.getName(), selectedRoad.dest.getName(), selectedRoad.region);

        if (!isInDoubleSpell || !c.IsTrasportCounter() || !c.CanTravel(selectedRoad.region))
            return;

        counter2Place = c;

        PlayDoubleSpell msg = new PlayDoubleSpell();
        msg.Road = selectedRoad;
        msg.Counter = counter2Place;
        MessageHandler.Message(msg);

        //Clear data for the last one
        ClearDoubleSpellData();
    }

   

    public void GroundCounterSelected(Counter c, Road r) {
        if(isInExchangeSpell && c.IsTrasportCounter()) {
            if(counter1 == null) { 
                counter1 = c;
                road1 = r;
            }
            else {
                counter2 = c;
                road2 = r;
                if(!counter2.CanTravel(road1.region))
                    DisplayErrorMessage("Selected counter: " + counter2.type.ToString() + " cannot be placed in region: " + road1.region.ToString());
                else if(!counter1.CanTravel(road2.region))
                    DisplayErrorMessage("Selected counter: " + counter1.type.ToString() + " cannot be placed in region: " + road2.region.ToString());
                else {
                    PlayExchangeSpell msg = new PlayExchangeSpell();
                    msg.CounterOne = counter1;
                    msg.CounterTwo = counter2;
                    msg.First = road1;
                    msg.Second = road2;
                    MessageHandler.Message(msg);
                }

                counter1 = null;
                counter2 = null;
                road1 = null;
                road2 = null;
            }
        }
    }

    public void HeldCounterSelected(Counter c)
    {
        //If successfully performed a double spell action, return
        if (isInDoubleSpell)
        {
            DoubleSpellCounterSelected(c);
            return;
        }

        if (selectedRoad == null || Game.phase != GamePhase.PlaceCounter)
        {
            selectedRoad = null; // in case we have the latter case
            return;
        }

        Road updatedSelectedRoad = selectedRoad;
        if(selectedRoad != null)
            updatedSelectedRoad = Game.GetRoad(selectedRoad.source.getName(), selectedRoad.dest.getName(), selectedRoad.region);

        heldCounterUI.CountersCanBeClicked(null, false); // need to stop accepting input from CardUI
        if (c.IsTrasportCounter())
        {
            // TODO add support for double counter
            if (updatedSelectedRoad != null && !updatedSelectedRoad.ContainsTransportCounter() && Game.travelValues.ContainsKey(new KeyValuePair<TransportType, Region>(c.GetTransportType(), updatedSelectedRoad.region)))
            {
                PlaceCounter placeCounter = new PlaceCounter();
                placeCounter.Counter = c;
                placeCounter.Road = updatedSelectedRoad;
                MessageHandler.Message(placeCounter);
            }
        }
        else if (c.IsObstacle())
        {
            if (updatedSelectedRoad != null && !updatedSelectedRoad.ContainsObstacle() && c.ValidHere(updatedSelectedRoad.region))
            {
                PlaceCounter placeCounter = new PlaceCounter();
                placeCounter.Counter = c;
                placeCounter.Road = updatedSelectedRoad;
                MessageHandler.Message(placeCounter);
            }
        }
        else if (c.IsDoubleSpell())
        {
            // TODO
            //If the player does not have any other counters or the road doesn't have a transport counter yet, he cant use double spell
            if (updatedSelectedRoad != null && updatedSelectedRoad.ContainsTransportCounter() && Client.GetLocalPlayer().CanUseDoubleSpell())
            {
                isInDoubleSpell = true;
                heldCounterUI.CountersCanBeClicked(updatedSelectedRoad, true);
            }
        }
        else if (c.IsExchangeSpell())
        {
            //Place the exchange spell on a road, select a counter on the road, select another road, select a counter on that road, exchange the two(must ensure they are valid)
            if(Game.IsCurrentPlayer(Client.GetLocalPlayer())) {
                isInExchangeSpell = true;
                roadUI.ActivateCounterBorders();
            }
            
        }
        else if (c.type == Counter.CounterType.GOLD)
        {
            PlaceCounter placeCounter = new PlaceCounter();
            placeCounter.Counter = c;
            placeCounter.Road = updatedSelectedRoad;
            MessageHandler.Message(placeCounter);
        }
        else
            Debug.Log("ERROR: Invalid Counter type");
    }

    /// <summary> Used to select cards to use for a caravan move </summary>
    public void CardSelected(CardPressedButton cb)
    {
        Card c = cb.card;

        if (c.IsWitchCard() && Game.phase == GamePhase.MoveBoot && Game.IsCurrentPlayer(Client.GetLocalPlayer()))
        {
            Debug.Log("Clicked on Witch");
            usingWitch = !usingWitch;
            cb.exit.SetActive(usingWitch);
            townUI.UpdateAccessibleTownsForWitch(usingWitch);
            return;
        }

        if (cardsToBePicked < 0) // we are not allowed to pick cards at the moment
            return;

        if (!(c.IsTravelCard()))
            Debug.Log("ERROR: a non-travel card is being used for travel");
        else
        {
            if(cardsForCaravan.Contains(cb)) {
                cardsForCaravan.Remove(cb);
                cb.GetComponent<UnityEngine.UI.Image>().color = UnityEngine.Color.white;
                cardsToBePicked++;
            }
            else {
                cardsForCaravan.Add(cb);
                cardsToBePicked--;
                cb.gameObject.GetComponent<UnityEngine.UI.Image>().color = UnityEngine.Color.grey;
                if (cardsToBePicked == 0)
                { // make move request
                    TravelOnRoad caravanRequest = new TravelOnRoad();
                    
                    caravanRequest.Cards = new List<Card>();
                    foreach(CardPressedButton _cb in cardsForCaravan) {
                        caravanRequest.Cards.Add(_cb.card);
                    }
                    caravanRequest.Road = targetRoad;
                    caravanRequest.isCaravan = true;
                    cardsToBePicked = int.MinValue;
                    MessageHandler.Message(caravanRequest);
                }
            }
            
        }
    }

    public void DisplayErrorMessage(string msg)
    {
        errorPopup.gameObject.SetActive(true);
        errorPopup.DisplayText(msg);
    }
}