using System.Collections.Generic;
using Elfencore.Shared.GameState;

namespace Elfencore.Shared.Messages.ClientToServer
{
    public class ChooseCounterToKeep
    {
        public Counter Counter;
    }

    public class ChooseToGetGoldForTravel
    {
        public bool gold;
    }

    public class DrawCard
    {
        public Card Card;
    }

    public class DrawCounter
    {
        public Counter Counter;
    }

    public class DrawRandomCard
    {
    }

    public class TakeGoldDeck
    {
    }

    public class DrawRandomCounter
    {
    }

    public class EndTurn
    {
    }

    // TODO move boot
    public class PassTurn
    {
    }

    public class PlaceBid
    {
        public int bid;
    }

    public class PlaceCounter
    {
        public Counter Counter;
        public Road Road;
    }

    // TODO place gold counter
    public class PlayDoubleSpell
    {
        public Counter Counter;
        public Road Road;
    }

    public class PlayExchangeSpell
    {
        public Road First;
        public Road Second;
        public Counter CounterOne;
        public Counter CounterTwo;
    }

    public class SelectFaceDown
    {
        public Counter Counter;
    }

    public class TravelOnRoad
    {
        public List<Card> Cards;
        public Road Road;
        public bool isCaravan;

        public TravelOnRoad()
        { }

        public TravelOnRoad(Road pRoad, TransportType type, int cardsNeeded, bool useCaravan)
        {
            Cards = new List<Card>();
            for (int i = 0; i < cardsNeeded; i++)
                Cards.Add(new Card(type));
            isCaravan = useCaravan;
            Road = pRoad;
        }
    }

    public class UndoTurn
    {
    }

    public class UseWitchForFlight
    {
        public Town Town;
    }

    public class UseWitchForObstacle
    {
        public Road Road;
    }

    public class ChooseBoot
    {
        public Color Color;
    }

    public class ChooseCounterToMakeHidden
    {
        public Counter Invis;
    }

    public class RequestSave
    {
    }

    public class CountersToKeep
    {
        public List<Counter> Counters;
    }
}