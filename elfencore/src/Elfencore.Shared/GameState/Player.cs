using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Elfencore.Shared.GameState
{
    /// <summary> The conceptual idea of a player. This should be separate from the idea of a Human Player.
    /// All methods in this class should work for AI players to stress that this is not a human player. </summary>
    public class Player
    {
        public Color bootColor = null;
        public Town location = null;

        /// <summary> Destination for variant where we get desitnation cards </summary>
        public Town destination;
        public HashSet<Town> visited = new HashSet<Town>();
        public int goldThisTurn = 0;
        public List<Counter> ownedCounters = new List<Counter>();
        public List<Card> ownedCards = new List<Card>();
        public List<Counter> schrodingerCounters = new List<Counter>();
        public string username;
        public int gold = 0;
        public bool selectedBoot = false;

        public Player(string username)
        {
            this.username = username;
            bootColor = Color.WHITE;
        }

        public Player(string username, Color bColor)
        {
            this.username = username;
            bootColor = bColor;
        }

        public Player()
        {
            bootColor = Color.WHITE;
        }

        public bool CardsRemovable(List<Card> cards)
        {
            int[] count = new int[9];
            foreach (Card card in ownedCards)
            {
                count[(int)card.type]++;
            }
            foreach (Card card in cards)
            {
                count[(int)card.type]--;
            }
            for (int i = 0; i < 9; i++)
            {
                if (count[i] < 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CountersRemovable(List<Counter> counters)
        {
            int[] count = new int[12];
            foreach(Counter counter in ownedCounters)
            {
                count[(int) counter.type]++;
            }
            foreach(Counter counter in counters)
            {
                count[(int)counter.type]--;
            }
            for(int i = 0; i < 12; i++)
            {
                if (count[i] < 0)
                {
                    return false;
                }
            }
            return true;
        }

        public void AddCounter(Counter c)
        { ownedCounters.Add(c); }

        public List<Counter> GetCounters()
        { return ownedCounters; }

        public List<Card> GetCards()
        { return ownedCards; }

        public void AddCard(Card c)
        { ownedCards.Add(c); }

        public void SetColor(Color c)
        { bootColor = c; }

        public Color GetColor()
        { return bootColor; }

        public int NumOfCountersHeld()
        { return ownedCounters.Count; }

        public int NumOfCardsHeld()
        { return ownedCards.Count; }

        public int NumOfTownVisited()
        { return visited.Count; }

        public void SetDestination(Town destination)
        { this.destination = destination; }

        public Town GetDestination()
        { return destination; }

        public Town GetLocation()
        { return location; }

        public void SetLocation(Town newLocation)
        { location = newLocation; }

        public int amountOfGold()
        { return gold; }

        public void AddGold(int amnt)
        { gold += amnt; }

        public void removeGold(int amnt)
        {
            if (gold >= amnt)
                gold -= amnt;
            else
                return;
            // TODO Display in UI "Not enough funds"
        }

        public bool ContainsCounterType(Counter c)
        {
            foreach (Counter count in ownedCounters)
            {
                if (count.SameType(c))
                    return true;
            }
            return false;
        }

        public string GetName()
        { return username; }

        public void MoveTo(Town t, bool hasGold)
        {
            location = t;
            visited.Add(t);
            goldThisTurn += t.getValue();
            if (hasGold)
            {
                goldThisTurn += t.getValue();
            }
        }

        public void RemoveCards<T>(List<T> cards) where T : Card
        {
            foreach (Card c in cards)
            {
                ownedCards.Remove(ownedCards.Where(item => item.type == c.type).First());
            }
            //ownedCards.RemoveAll(item => cards.Contains(item));
        }

        public void RemoveCard(Card card)
        {
            ownedCards.Remove(ownedCards.Where(item => item.type == card.type).First());
        }

        public void RemoveCounter(Counter c)
        {
            ownedCounters.Remove(ownedCounters.Where(item => item.type == c.type).First());
        }

        public void RemoveCountersButOne(Counter c)
        {
            bool kept = false;
            foreach (Counter count in ownedCounters)
            {
                if (count.SameType(c) && !kept)
                {
                    kept = true;
                }
                else
                {
                    Game.counterPile.Add(count);
                }
            }
            ownedCounters.Clear();
            ownedCounters.Add(c);
        }

        public void RemoveCountersButTwo(Counter c1, Counter c2)
        {
            bool kept1 = false;
            bool kept2 = false;
            foreach (Counter count in ownedCounters)
            {
                if (count.SameType(c1) && !kept1)
                {
                    kept1 = true;
                }
                else if (count.SameType(c2) && !kept2)
                {
                    kept2 = true;
                }
                else
                {
                    Game.counterPile.Add(count);
                }
            }
            ownedCounters.Clear();
            ownedCounters.Add(c1);
            ownedCounters.Add(c2);
        }

        public void RemoveCountersExcept(List<Counter> c)
        {
            int kept = 0;
            foreach (Counter count in c)
            {
                foreach (Counter owned in ownedCounters)
                {
                    if (count.SameType(owned) && kept < c.Count)
                    {
                        kept++;
                    }
                    else
                    {
                        Game.counterPile.Add(count);
                    }
                }
            }
            ownedCounters.Clear();
            foreach (Counter count in c)
            {
                ownedCounters.Add(count);
            }
        }

        public bool CanUseDoubleSpell()
        {
            return ownedCounters.Any(c => c.IsTrasportCounter());
        }
    }
};