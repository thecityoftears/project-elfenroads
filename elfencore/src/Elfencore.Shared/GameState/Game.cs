using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

namespace Elfencore.Shared.GameState
{
    public enum Variant
    {
        ELFENLAND,
        ELFENGOLD
    }

    /// <summary> The conceptual container of game. Should not contain any verification. Methods should run as if they are valid.
    /// The responsibility of verification lays withing another set of classes. </summary>
    public static class Game
    {
        public static readonly Dictionary<KeyValuePair<TransportType, Region>, int> travelValues = new Dictionary<KeyValuePair<TransportType, Region>, int>() {
            { new KeyValuePair<TransportType, Region>(TransportType.GIANTPIG, Region.PLAINS), 1},
            { new KeyValuePair<TransportType, Region>(TransportType.GIANTPIG, Region.FOREST), 1},
            { new KeyValuePair<TransportType, Region>(TransportType.ELFCYCLE, Region.PLAINS), 1},
            { new KeyValuePair<TransportType, Region>(TransportType.ELFCYCLE, Region.FOREST), 1},
            { new KeyValuePair<TransportType, Region>(TransportType.ELFCYCLE, Region.MOUNTAIN), 2},
            { new KeyValuePair<TransportType, Region>(TransportType.MAGICCLOUD, Region.PLAINS), 2},
            { new KeyValuePair<TransportType, Region>(TransportType.MAGICCLOUD, Region.FOREST), 2},
            { new KeyValuePair<TransportType, Region>(TransportType.MAGICCLOUD, Region.MOUNTAIN), 1},
            { new KeyValuePair<TransportType, Region>(TransportType.UNICORN, Region.FOREST), 1},
            { new KeyValuePair<TransportType, Region>(TransportType.UNICORN, Region.DESERT), 2},
            { new KeyValuePair<TransportType, Region>(TransportType.UNICORN, Region.MOUNTAIN), 1},
            { new KeyValuePair<TransportType, Region>(TransportType.TROLLWAGON, Region.PLAINS), 1},
            { new KeyValuePair<TransportType, Region>(TransportType.TROLLWAGON, Region.FOREST), 2},
            { new KeyValuePair<TransportType, Region>(TransportType.TROLLWAGON, Region.DESERT), 2},
            { new KeyValuePair<TransportType, Region>(TransportType.TROLLWAGON, Region.MOUNTAIN), 2},
            { new KeyValuePair<TransportType, Region>(TransportType.DRAGON, Region.PLAINS), 1},
            { new KeyValuePair<TransportType, Region>(TransportType.DRAGON, Region.FOREST), 2},
            { new KeyValuePair<TransportType, Region>(TransportType.DRAGON, Region.DESERT), 1},
            { new KeyValuePair<TransportType, Region>(TransportType.DRAGON, Region.MOUNTAIN), 1},
            { new KeyValuePair<TransportType, Region>(TransportType.RAFT, Region.LAKE), 2},
            { new KeyValuePair<TransportType, Region>(TransportType.RAFT, Region.RIVER), 1},
        };

        private static readonly Dictionary<string, int> baseGameTowns = new Dictionary<string, int>()
        {
            {"Elvenhold", 0}, {"Rivinia", 3}, {"Feodor", 4}, {"Al'Baran", 7}, {"Dag'Amura", 4}, {"Kihromah", 6},
            {"Lapphalya", 2}, {"Parundia", 4}, {"Wylhien", 3}, {"Usselen", 4}, {"Yttar", 4},
            {"Grangor", 5}, {"Mah'Davikia", 5}, {"Ixara", 3}, {"Virst", 3}, {"Strykhaven", 4}, {"Beata", 2},
            {"Erg'Eren", 5}, {"Tichih", 3}, {"Throtmanni", 3}, {"Jaccaranda", 5}
        };

        private static readonly List<(string, string, Region)> baseGameRoads = new List<(string, string, Region)>()
        {
            ("Wylhien", "Usselen",  Region.PLAINS), ("Wylhien", "Usselen",  Region.RIVER), ("Wylhien", "Parundia",  Region.PLAINS), ("Wylhien", "Al'Baran",  Region.DESERT),
            ("Wylhien", "Jaccaranda",  Region.MOUNTAIN), ("Usselen", "Parundia",  Region.FOREST), ("Usselen", "Yttar",  Region.FOREST), ("Yttar",  "Parundia", Region.LAKE),
            ("Yttar",  "Grangor", Region.LAKE), ("Yttar",  "Grangor", Region.MOUNTAIN), ("Grangor", "Parundia",  Region.LAKE), ("Grangor", "Mah'Davikia",  Region.MOUNTAIN),
            ("Mah'Davikia", "Grangor",  Region.RIVER), ("Mah'Davikia", "Dag'Amura",  Region.MOUNTAIN), ("Mah'Davikia", "Ixara",  Region.MOUNTAIN), ("Ixara", "Mah'Davikia", Region.RIVER),
            ("Ixara", "Dag'Amura", Region.FOREST), ("Ixara", "Lapphalya", Region.FOREST), ("Ixara", "Virst", Region.PLAINS), ("Virst", "Ixara", Region.RIVER),
            ("Virst", "Lapphalya", Region.PLAINS), ("Virst", "Strykhaven", Region.MOUNTAIN), ("Virst", "Strykhaven", Region.LAKE), ("Virst", "Elvenhold", Region.LAKE),
            ("Strykhaven", "Elvenhold", Region.LAKE), ("Strykhaven", "Beata", Region.PLAINS), ("Beata", "Elvenhold", Region.PLAINS), ("Beata", "Elvenhold", Region.RIVER),
            ("Elvenhold", "Erg'Eren", Region.FOREST), ("Elvenhold", "Lapphalya", Region.PLAINS), ("Elvenhold", "Rivinia", Region.RIVER), ("Lapphalya", "Rivinia", Region.FOREST),
            ("Lapphalya", "Feodor", Region.FOREST), ("Lapphalya", "Dag'Amura", Region.FOREST), ("Dag'Amura", "Kihromah", Region.FOREST), ("Dag'Amura", "Al'Baran", Region.DESERT),
            ("Dag'Amura", "Feodor", Region.DESERT), ("Al'Baran", "Parundia", Region.DESERT), ("Al'Baran", "Feodor", Region.DESERT), ("Al'Baran", "Throtmanni", Region.DESERT),
            ("Throtmanni", "Feodor", Region.DESERT), ("Throtmanni", "Jaccaranda", Region.MOUNTAIN), ("Throtmanni", "Tichih", Region.PLAINS), ("Throtmanni", "Rivinia", Region.FOREST),
            ("Rivinia", "Feodor", Region.FOREST), ("Rivinia", "Tichih", Region.RIVER), ("Tichih", "Erg'Eren", Region.FOREST), ("Tichih", "Jaccaranda", Region.MOUNTAIN)
        };

        private static readonly List<(TransportType, int)> ELCounterPile = new List<(TransportType, int)>()
        {
            (TransportType.DRAGON, 8), (TransportType.UNICORN, 8), (TransportType.TROLLWAGON, 8), (TransportType.ELFCYCLE, 8), (TransportType.MAGICCLOUD, 8), (TransportType.GIANTPIG, 8)
        };

        private static readonly List<(TransportType, int)> EGCounterPile = new List<(TransportType, int)>()
        {
            (TransportType.DRAGON, 4), (TransportType.UNICORN, 5), (TransportType.TROLLWAGON, 8), (TransportType.ELFCYCLE, 8), (TransportType.MAGICCLOUD, 4), (TransportType.GIANTPIG, 9)
        };

        /// <summary>
        /// How many cards of each transport type in variant ElfenLand
        /// </summary>
        private static readonly List<(TransportType, int)> ELCardDeck = new List<(TransportType, int)>()
        {
            (TransportType.DRAGON, 10), (TransportType.UNICORN, 10), (TransportType.TROLLWAGON, 10), (TransportType.ELFCYCLE, 10), (TransportType.MAGICCLOUD, 10), (TransportType.GIANTPIG, 10), (TransportType.RAFT, 12)
        };

        /// <summary>
        /// How many cards of each transport type in variant Elfengold
        /// </summary>
        private static readonly List<(TransportType, int)> EGCardDeck = new List<(TransportType, int)>()
        {
            (TransportType.DRAGON, 9), (TransportType.UNICORN, 9), (TransportType.TROLLWAGON, 9), (TransportType.ELFCYCLE, 9), (TransportType.MAGICCLOUD, 9), (TransportType.GIANTPIG, 9), (TransportType.RAFT, 9)
        };

        public static readonly List<string> possibleDestinations = new List<string>() 
            {
                "Beata", "Erg'Eren", "Grangor", "Ixara", "Jaccaranda", "Mah'Davikia", "Strykhaven", "Tichih", "Usselen", "Virst", "Wylhien", "Yttar"
            };

        public static Variant variant;
        public static int numRounds;
        public static int curRound = 1;
        public static GamePhase phase;
        public static Player currentPlayer = null;
        public static List<Player> participants = new List<Player>();
        public static List<Color> ChosenColors = new List<Color>();
        public static List<Player> finishedPhase = new List<Player>();
        public static List<Counter> faceUpCounters = new List<Counter>();
        public static List<Counter> counterPile = new List<Counter>();
        public static List<Card> discardPile = new List<Card>();
        public static List<Card> cardDeck = new List<Card>();
        public static List<Card> faceUpCards = new List<Card>();
        public static List<Card> goldDeck = new List<Card>(); // deck of gold cards - might be better to represent as int?
        public static List<Road> roads = new List<Road>(); // All game roads
        public static Dictionary<string, Town> towns = new Dictionary<string, Town>();
        public static Player winner = null;
        public static bool winnerDeclared = false;
        public static bool witchEnabled;
        public static bool randomGold;
        public static bool randomDest;
        public static Auction auction;

        /// <summary> Returns the player with the given username </summary>
        public static Player GetPlayerFromName(string username)
        {
            for (int i = 0; i < participants.Count; i++)
            {
                if (participants[i].GetName() == username)
                    return participants[i];
            }
            return null;
        }

        public static Town GetTownFromName(string townName)
        {
            Town foundTown;
            towns.TryGetValue(townName, out foundTown);
            return foundTown;
        }

        /// <summary> Returns a list of all the towns that neighbor the given town </summary>
        public static List<Town> GetNeighboringTowns(Town t)
        {
            List<Town> neighbors = new List<Town>();

            foreach (Road r in roads)
            {
                if (r.source.getName() == t.getName())
                    neighbors.Add(r.dest);
                else if (r.dest.getName() == t.getName())
                    neighbors.Add(r.source);
            }
            return neighbors;
        }

        /// <summary> Generates the town objects, the card deck, counter pile, and the roads </summary>
        public static void InstantiateELObjects()
        {
            GenerateTowns(false);
            GenerateDeck(Variant.ELFENLAND);
            GeneratePile(Variant.ELFENLAND);
            GenerateRoads();
        }

        /// <summary> Generates the town objects, the card deck, counter pile, and the roads </summary>
        public static void InstantiateEGObjects()
        {
            GenerateTowns(randomGold);
            GenerateDeck(Variant.ELFENGOLD);
            // check and add witch
            GeneratePile(Variant.ELFENGOLD);
            GenerateRoads();
        }

        /// <summary> returns if the player passed is the current player </summary>
        public static bool IsCurrentPlayer(Player p)
        {
            return p.GetName() == currentPlayer.GetName();
        }

        public static int getNoPlayers()
        {
            return participants.Count;
        }

        public static bool IsDrawCardPhase()
        { return phase == GamePhase.DrawCardOnePhase || phase == GamePhase.DrawCardTwoPhase || phase == GamePhase.DrawCardThreePhase; }

        public static bool IsDrawCounterPhase()
        { return phase == GamePhase.DrawCounterOnePhase || phase == GamePhase.DrawCounterTwoPhase || phase == GamePhase.DrawCounterThreePhase; }

        public static bool IsAuctionPhase()
        { return phase == GamePhase.Auction; }

        /// <summary> Returns whether or not the Counter given is in the faceupCounters (i.e. can be drawn by type) </summary>
        public static bool FaceUpCounterContains(Counter c)
        {
            foreach (Counter faceup in faceUpCounters)
            {
                if (faceup.SameType(c))
                    return true;
            }
            return false;
        }

        /// <summary> Returns whether or not the Counter given is in the faceupCards (i.e. can be drawn by type) </summary>
        public static bool FaceUpCardContains(Card c)
        {
            foreach (Card faceup in faceUpCards)
            {
                if (faceup.SameType(c))
                    return true;
            }
            return false;
        }

        /// <summary> Shuffles a list in a random order </summary>
        public static List<T> Shuffle<T>(List<T> _list)
        {
            Random rng = new Random();
            return _list.OrderBy(a => rng.Next()).ToList();
        }

        /// <summary>
        /// Shuffle the Card Deck
        /// </summary>
        public static void ShuffleDeck()
        { cardDeck = Shuffle(cardDeck); }

        public static void RemoveFirstCard()
        {
            cardDeck.RemoveAt(0);
            // in Elfengold, when the supply is exhausted, use the discard pile as a new supply
            if (cardDeck.Count == 0)
            {
                CreateSupplyFromDiscardPile();
            }
        }

        public static void CreateSupplyFromDiscardPile()
        {
            for (int i = 0; i < discardPile.Count; i++)
            {
                cardDeck.Add(discardPile[i]);
            }
            ShuffleDeck();
            discardPile.Clear();
        }

        public static void ShuffleCounterPile()
        { counterPile = Shuffle(counterPile); }

        /// <summary> Sets all the locations of all participants to the town with the name townName </summary>
        public static void SetStartingLocation(string townName)
        {
            Town t;
            towns.TryGetValue(townName, out t);
            if (t == null)
                return;

            // set starting locations
            foreach (Player p in participants) {
                p.SetLocation(t);
                p.visited.Add(t);
            }
        }

        /// <summary> Creates Town objects for each town described in baseGameTowns. If randomGold is true then the gold values are ignored and a random value is given </summary>
        private static void GenerateTowns(bool randomGold)
        {
            Random rng = new Random();
            foreach (KeyValuePair<string, int> encodedCity in baseGameTowns)
            {
                if (randomGold)
                    towns.Add(encodedCity.Key, new Town(encodedCity.Key, baseGameTowns.Values.ToList().ElementAt(rng.Next(baseGameTowns.Count))));
                else
                    towns.Add(encodedCity.Key, new Town(encodedCity.Key, encodedCity.Value));
            }
        }

        /// <summary> Creates Road objects for each road described in baseGameRoads </summary>
        private static void GenerateRoads()
        {
            foreach ((string, string, Region) tup in baseGameRoads)
            {
                Town src;
                Town dst;
                towns.TryGetValue(tup.Item1, out src);
                towns.TryGetValue(tup.Item2, out dst);

                if (src == null || dst == null)
                    System.Console.Write("ERROR, towns not found");
                else
                {
                    roads.Add(new Road(src, dst, tup.Item3));
                }
            }
        }

        /// <summary> Populates the Deck with cards based on the variant provided. Card numbers are defined in EL/EG CardDeck. NOTE: As of now, whether we are using the witch should be defined ahead of time </summary>
        private static void GenerateDeck(Variant variant)
        {
            if (variant == Variant.ELFENLAND)
            {
                foreach ((TransportType, int) tup in ELCardDeck)
                {
                    for (int i = 0; i < tup.Item2; i++)
                    {
                        cardDeck.Add(new Card(tup.Item1));
                    }
                }
            }
            else
            {
                foreach ((TransportType, int) tup in EGCardDeck)
                {
                    for (int i = 0; i < tup.Item2; i++)
                    {
                        cardDeck.Add(new Card(tup.Item1));
                    }
                }

                if (witchEnabled)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        cardDeck.Add(new Card(Card.CardType.WITCH));
                    }
                }
            }
        }

        /// <summary> THIS IS PROBABLY INCORRECT AS IT DOES NOT TAKE INTO ACCOUNT ELFENLAND. not sure tho </summary>
        public static void FinishDeckInstantiation()
        {
            //draw the faceup cards
            for (int i = 0; i < 3; i++)
            {
                faceUpCards.Add(cardDeck[0]);
                cardDeck.RemoveAt(0);
            }

            //add the gold cards
            for (int i = 0; i < 7; i++)
            {
                cardDeck.Add(new Card(Card.CardType.GOLD));
            }
        }

        /// <summary> Generates the counter pile based on the values in EL/EG CounterPile </summary>
        private static void GeneratePile(Variant variant)
        {
            if (variant == Variant.ELFENLAND)
            {
                foreach ((TransportType, int) tup in ELCounterPile)
                {
                    for (int i = 0; i < tup.Item2; i++)
                    {
                        counterPile.Add(new Counter(tup.Item1));
                    }
                }

                //shuffle pile so that starting counters are random
                ShuffleCounterPile();

                // initiate face-up counters
                for (int i = 0; i < 5; i++)
                {
                    faceUpCounters.Add(counterPile[0]);
                    counterPile.RemoveAt(0);
                }
            }
            else
            {
                foreach ((TransportType, int) tup in EGCounterPile)
                {
                    for (int i = 0; i < tup.Item2; i++)
                    {
                        counterPile.Add(new Counter(tup.Item1));
                    }
                }

                // add the obstacles and magic spells
                for (int i = 0; i < 2; i++)
                {
                    counterPile.Add(new Counter(Counter.CounterType.TREEOBS));
                    counterPile.Add(new Counter(Counter.CounterType.SEAOBS));
                    counterPile.Add(new Counter(Counter.CounterType.GOLD));
                    counterPile.Add(new Counter(Counter.CounterType.DOUBLESPELL));
                    counterPile.Add(new Counter(Counter.CounterType.EXCHANGESPELL));
                }
            }
        }

        /// <summary> Returns the list of roads connecting the source and destination town </summary>
        public static List<Road> GetRoadsBetween(Town source, Town dest)
        {
            List<Road> foundRoads = new List<Road>();
            foreach (Road r in roads)
            {
                if ((r.source.getName() == source.getName() && r.dest.getName() == dest.getName()) || (r.dest.getName() == source.getName() && r.source.getName() == dest.getName()))
                    foundRoads.Add(r);
            }

            return foundRoads;
        }

        public static Road GetRoad(string srcName, string destName, Region region)
        {
            foreach (Road r in roads)
            {
                if (r.source.getName() == srcName && r.dest.getName() == destName && r.region == region)
                    return r;
            }
            return null;
        }

        /// <summary> Returns the Player that is currently in the lead </summary>
        public static Player GetCurrentWinner()
        {
            int maxTownsVisited = int.MinValue;
            List<Player> firstPlacePlayers = new List<Player>(); // list of players tied for first based on towns visited
            foreach (Player p in participants)
            {
                int visitedTownValue = p.NumOfTownVisited();
                if (Game.randomDest)
                {
                    visitedTownValue -= Game.DijkstraShortestPath(p.GetLocation(), p.GetDestination());
                }

                if (visitedTownValue > maxTownsVisited)
                {
                    maxTownsVisited = visitedTownValue;
                    firstPlacePlayers.Clear();
                    firstPlacePlayers.Add(p);
                }
                else if (visitedTownValue == maxTownsVisited)
                {
                    firstPlacePlayers.Add(p);
                }
            }

            if (firstPlacePlayers.Count == 1)
                return firstPlacePlayers[0];

            if (Game.variant == Variant.ELFENLAND)
            {
                int maxCards = int.MinValue;
                Player maxCardsHeld = null;
                foreach (Player p in firstPlacePlayers)
                {
                    if (p.GetCards().Count > maxCards)
                    {
                        maxCards = p.GetCards().Count;
                        maxCardsHeld = p;
                    }
                }

                return maxCardsHeld;
            }
            else // Elfengold
            {
                int maxGold = int.MinValue;
                Player maxGoldHeld = null;
                foreach (Player p in firstPlacePlayers)
                {
                    if (p.amountOfGold() > maxGold)
                    {
                        maxGold = p.amountOfGold();
                        maxGoldHeld = p;
                    }
                }

                return maxGoldHeld;
            }
        }

        public static int getPlayerIndex(Player player)
        {
            for (int i = 0; i < participants.Count; i++)
            {
                if (player.GetName() == participants[i].GetName())
                    return i;
            }
            return -1;
        }

        /// <summary> Uses dijkstra to find shortest path between 2 towns. Used to calculate random dest winner </summary>
        public static int DijkstraShortestPath(Town src, Town dest)
        {
            Dictionary<string, int> dist = new Dictionary<string, int>();
            List<string> Q = new List<string>();
            foreach (string t in towns.Keys)
            {
                dist.Add(t, int.MaxValue);
                Q.Add(t);
            }
            dist[src.getName()] = 0;

            while (Q.Count > 0)
            {
                int minDist = int.MaxValue;
                string minTown = "UNDEFINED";
                // find min dist node in Q
                foreach (string t in Q)
                {
                    if (dist[t] < minDist)
                    {
                        minDist = dist[t];
                        minTown = t;
                    }
                }
                Q.Remove(minTown);

                if (minTown == dest.getName())
                    return minDist;
                else
                {
                    List<Town> neighbors = GetNeighboringTowns(towns[minTown]);
                    List<Town> toRemove = new List<Town>();
                    foreach (Town t in neighbors)
                    {
                        if (!Q.Contains(t.getName()))
                            toRemove.Add(t);
                    }
                    foreach (Town removedTown in toRemove)
                    {
                        neighbors.Remove(removedTown);
                    }

                    foreach (Town t in neighbors)
                    {
                        int alt = dist[minTown] + 1;
                        if (alt < dist[t.getName()])
                        {
                            dist[t.getName()] = alt;
                        }
                    }
                }
            }

            return dist[dest.getName()];
        }
    }
};