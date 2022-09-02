using Elfencore.Shared.GameState;
using Elfencore.Shared.Messages;
using ClientToServer = Elfencore.Shared.Messages.ClientToServer;
using CoreToServer = Elfencore.Shared.Messages.CoreToServer;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Elfencore.Session
{
    public class Save
    {
        public string gameService;
        public List<string> players;
        public GameStateMsg data;
    }
    public class Session
    {
        public ObservableCollection<CoreToServer.OpBroadcast> OutBroadcast;
        public ObservableCollection<CoreToServer.OpSingle> OutSingle;
        public ObservableCollection<CoreToServer.OpSave> OutSave;
        public ObservableCollection<CoreToServer.OpTerminate> OutTerminate;
        public string filePath;
        private ISet<string> joinable;
        private ISet<string> joined;
        private bool needToCreateGameObjects;

        private static string SerializeStaticClass(System.Type a_Type)
        {
            var TypeBlob = a_Type.GetFields().ToDictionary(x => x.Name, x => x.GetValue(null));
            return JsonConvert.SerializeObject(TypeBlob);
        }

        public Session(
            Variant variant,
            int numRounds,
            bool randomDest,
            bool witch,
            bool randomGold,
            string filePath
        )
        {
            Game.variant = variant;
            Game.numRounds = numRounds;
            Game.randomDest = randomDest;
            Game.witchEnabled = witch;
            Game.randomGold = randomGold;
            this.OutBroadcast = new ObservableCollection<CoreToServer.OpBroadcast>();
            this.OutSingle = new ObservableCollection<CoreToServer.OpSingle>();
            this.OutSave = new ObservableCollection<CoreToServer.OpSave>();
            this.OutTerminate = new ObservableCollection<CoreToServer.OpTerminate>();
            this.filePath = filePath;
            this.joinable = new HashSet<string>();
            this.joined = new HashSet<string>();
            needToCreateGameObjects = true;
        }

        public Session(string savegame, string filePath)
        {
            JsonSerializerSettings jsSettings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
            };
            Save save = JsonConvert.DeserializeObject<Save>(savegame, jsSettings);
            GameStateMsg msg = save.data;
            GameStateMsg.ReadMsg(msg);
            this.OutBroadcast = new ObservableCollection<CoreToServer.OpBroadcast>();
            this.OutSingle = new ObservableCollection<CoreToServer.OpSingle>();
            this.OutSave = new ObservableCollection<CoreToServer.OpSave>();
            this.OutTerminate = new ObservableCollection<CoreToServer.OpTerminate>();
            this.filePath = filePath;
            this.joinable = new HashSet<string>();
            this.joined = new HashSet<string>();
            needToCreateGameObjects = false;
        }

        private void broadcastGame()
        {
            this.OutBroadcast.Add(new CoreToServer.OpBroadcast(JsonConvert.SerializeObject(
                new Message("BroadcastGameState", JsonConvert.SerializeObject(GameStateMsg.CreateMsg()))
            )));

        }

        private void saveGame()
        {
            Save newSave = new Save();
            string gameServiceName = "";
            if (Game.variant == Variant.ELFENLAND)
            {
                gameServiceName = "land:" + Game.numRounds.ToString()
                    + ":" + Game.randomDest.ToString().ToLower();
            }
            else
            {
                gameServiceName = "gold:" + Game.numRounds.ToString()
                    + ":" + Game.randomDest.ToString().ToLower()
                    + ":" + Game.witchEnabled.ToString().ToLower()
                    + ":" + Game.randomGold.ToString().ToLower();
            }
            newSave.gameService = gameServiceName;
            List<string> usernames = new List<string>();
            foreach (Player p in Game.participants)
            {
                usernames.Add(p.username);
            }
            newSave.players = usernames;
            newSave.data = GameStateMsg.CreateMsg();

            File.WriteAllText(filePath, JsonConvert.SerializeObject(newSave));

            this.OutSave.Add(
                new CoreToServer.OpSave(Game.participants.Select(p => p.username))
            );
        }

        public void reject(string user, string tag)
        {
            this.OutSingle.Add(new CoreToServer.OpSingle(
                user,
                JsonConvert.SerializeObject(new Message(
                    "SendErrorMessage",
                    tag
                ))
            ));
        }

        public void HandleConnected(string user)
        {
            Console.WriteLine("CONNECTING " + user);
            if (this.joinable.Contains(user))
            {
                Console.WriteLine("joinable contains");
                if (!this.joined.Contains(user))
                {
                    Console.WriteLine("Joined does not contain");
                    this.joined.Add(user);
                }
            }
            Console.WriteLine("!! joined " + this.joined);
            Console.WriteLine("!! joinable " + this.joinable);

            // everyone's here
            if (this.joinable.SetEquals(this.joined))
            {
                Console.WriteLine("everyone's here");
                this.OutBroadcast.Add(new CoreToServer.OpBroadcast(JsonConvert.SerializeObject(
                    new Message("StartGame", JsonConvert.SerializeObject(GameStateMsg.CreateMsg()))
                )));
            }
            else
            {
                Console.WriteLine("not everyone's here");
            }
        }

        public void HandleDisconnected(string user)
        {
            joined.Remove(user);

            if (joined.Count == 0)
            {
                this.OutTerminate.Add(new CoreToServer.OpTerminate());
                Environment.Exit(0);
            }
        }

        public void HandleStartGame(List<string> users)
        {
            if (needToCreateGameObjects)
            {
                Console.WriteLine("====================RECEIVING USERS====================");
                foreach (string name in users)
                {
                    Console.WriteLine(name);
                }
                Game.participants.AddRange(users.Select((u) =>
                {
                    return new Player(u);
                }));

                if (Game.variant == Variant.ELFENLAND)
                    Game.InstantiateELObjects();
                else
                    Game.InstantiateEGObjects();

                // shuffle decks
                Game.ShuffleDeck();
                Game.ShuffleCounterPile();

                Game.SetStartingLocation("Elvenhold");

                // Add destinarion cards
                if (Game.randomDest)
                {
                    List<string> possibleDests = Game.possibleDestinations;
                    Random random = new Random();
                    foreach (Player p in Game.participants)
                    {
                        int index = random.Next(possibleDests.Count);
                        p.destination = Game.GetTownFromName(possibleDests[index]);
                        possibleDests.RemoveAt(index);
                    }
                }

                // Finish Game setup
                if (Game.variant == Variant.ELFENLAND)
                {
                    // deal 8 cards to each player
                    for (int i = 0; i < 8; i++)
                    {
                        foreach (Player p in Game.participants)
                        {
                            Card cardDealt = Game.cardDeck[0];
                            p.AddCard(cardDealt);
                            Game.RemoveFirstCard();

                            // TODO Send message to add card to player
                        }
                    }

                    // give out hidden counter
                    foreach (Player p in Game.participants)
                    {
                        Counter counterDealt = Game.counterPile[0];
                        counterDealt.SetVisible(false);
                        p.AddCounter(counterDealt);
                        Game.counterPile.RemoveAt(0);

                        // give out obstacle
                        p.AddCounter(new Counter(Counter.CounterType.TREEOBS));
                    }
                }
                else
                {
                    // deal 5 cards each
                    for (int i = 0; i < 5; i++)
                    {
                        foreach (Player p in Game.participants)
                        {
                            Card cardDealt = Game.cardDeck[0];
                            p.AddCard(cardDealt);
                            Game.RemoveFirstCard();

                            // TODO Send message to add card to player
                        }
                    }
                    // tell the game it can finish deck setup
                    Game.FinishDeckInstantiation();
                    Game.ShuffleDeck();

                    // add gold to each player
                    foreach (Player p in Game.participants)
                    {
                        p.AddGold(12);
                    }
                }
                Game.currentPlayer = Game.participants[0];
            }

            this.joinable = new HashSet<string>(users);
            Console.WriteLine("setting joinable " + this.joinable);
        }

        public async void HandlePayload(string user, string rawPayload)
        {
            try
            {
                var deserialized = JsonConvert.DeserializeObject<Message>(rawPayload);
                var messageType = Type.GetType(deserialized.Tag);
                var payload = JsonConvert.DeserializeObject(deserialized.Content, messageType);
                Console.WriteLine(deserialized.Tag);
                Console.WriteLine(payload);
                Player player = Game.GetPlayerFromName(user);
                var tag = deserialized.Tag;
                // typecast as necessary, see ClientToServer.cs
                switch (tag)
                {
                    case "Elfencore.Shared.Messages.ClientToServer.ChooseCounterToKeep":
                        {
                            var msg = JsonConvert.DeserializeObject<ClientToServer.ChooseCounterToKeep>(deserialized.Content);
                            // work with msg
                            if (Verification.VerifyChooseCounterToKeep(player, msg.Counter, this))
                            {
                                // In case of verification success, update GameState
                                player.RemoveCountersButOne(msg.Counter);
                                Game.finishedPhase.Add(player);
                                // Q: are counters secret to each player, or can everyone know?

                                if (Game.finishedPhase.Count == Game.participants.Count)
                                {
                                    // in case of new round
                                    NewRound();
                                }
                                else
                                {
                                    int pIndex = Game.participants.FindIndex(Game.IsCurrentPlayer);
                                    pIndex += 1;
                                    Game.currentPlayer = Game.participants[pIndex];
                                }
                            }
                            // ...
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.ChooseToGetGoldForTravel":
                        {
                            var msg = JsonConvert.DeserializeObject<ClientToServer.ChooseToGetGoldForTravel>(deserialized.Content);
                            Console.WriteLine(Game.finishedPhase.FindIndex(pl => pl.GetName() == player.GetName()));
                            Console.WriteLine(Game.finishedPhase.Count);
                            if (Verification.VerifyChooseToGetGoldForTravel(player, this))
                            {
                                Game.finishedPhase.Add(player);
                                // if the player wants gold, give them gold
                                if (msg.gold)
                                {
                                    player.AddGold(player.goldThisTurn);
                                }
                                // else deal them two travel cards
                                else
                                {
                                    for (int i = 0; i < 2; i++)
                                    {
                                        Card CardDealt = Game.cardDeck[0];
                                        while (CardDealt.type == Card.CardType.GOLD)
                                        {
                                            Game.goldDeck.Add(CardDealt);
                                            Game.RemoveFirstCard();
                                            CardDealt = Game.cardDeck[0];
                                        }
                                        player.AddCard(CardDealt);
                                        Game.RemoveFirstCard();
                                    }
                                }
                                player.goldThisTurn = 0;

                                if (Game.finishedPhase.Count == Game.participants.Count)
                                {
                                    Game.currentPlayer = Game.participants[0];
                                    Game.phase = GamePhase.EndOfRound;
                                    foreach (Player p in Game.participants)
                                    {
                                        if (p.NumOfCountersHeld() > 1)
                                        {
                                            Game.finishedPhase.Remove(p);
                                        }
                                    }
                                    if (Game.finishedPhase.Count == Game.participants.Count)
                                    {
                                        NewRound();
                                    }
                                }
                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.DrawCard":
                        {
                            var msg = JsonConvert.DeserializeObject<ClientToServer.DrawCard>(deserialized.Content);
                            // work with msg
                            if (Verification.VerifyDrawCard(player, msg.Card, this))
                            {
                                Game.faceUpCards.Remove(Game.faceUpCards.Where(item => item.type == msg.Card.type).First());
                                if (msg.Card.type != Card.CardType.GOLD)
                                {
                                    player.AddCard(msg.Card);
                                }
                                else
                                {
                                    Game.goldDeck.Add(msg.Card);
                                    Game.faceUpCards.Add(Game.cardDeck[0]);
                                    Game.RemoveFirstCard();
                                    break;
                                }
                                Game.faceUpCards.Add(Game.cardDeck[0]);
                                Game.RemoveFirstCard();

                                Game.finishedPhase.Add(player);
                                if (Game.finishedPhase.Count == Game.participants.Count)
                                {
                                    if (Game.phase == GamePhase.DrawCardOnePhase)
                                    {
                                        Game.phase = GamePhase.DrawCardTwoPhase;
                                    }
                                    else if (Game.phase == GamePhase.DrawCardTwoPhase)
                                    {
                                        Game.phase = GamePhase.DrawCardThreePhase;
                                    }
                                    else
                                    {
                                        for (int i = 0; i < Game.participants.Count; i++)
                                        {
                                            Game.participants[i].AddGold(2);

                                            //deal the two counters for the next phase
                                            Counter CounterDealt = Game.counterPile[0];
                                            CounterDealt.SetVisible(false);
                                            Game.counterPile.RemoveAt(0);
                                            Game.participants[i].schrodingerCounters.Add(CounterDealt);
                                            Counter CounterDealt2 = Game.counterPile[0];
                                            CounterDealt2.SetVisible(false);
                                            Game.participants[i].schrodingerCounters.Add(CounterDealt2);
                                            Game.counterPile.RemoveAt(0);
                                        }
                                        Game.phase = GamePhase.ChooseCounterPhase;
                                    }
                                    Game.currentPlayer = Game.participants[0];
                                    Game.finishedPhase.Clear();
                                }
                                else
                                {
                                    int pIndex = Game.participants.FindIndex(Game.IsCurrentPlayer);
                                    pIndex += 1;
                                    Game.currentPlayer = Game.participants[pIndex];
                                }
                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.DrawCounter":
                        {
                            // Q: is this visible to everyone?

                            var msg = JsonConvert.DeserializeObject<ClientToServer.DrawCounter>(deserialized.Content);
                            // work with msg
                            if (Verification.VerifyDrawCounter(player, msg.Counter, this))
                            {
                                Game.faceUpCounters.Remove(Game.faceUpCounters.Where(item => item.type == msg.Counter.type).First());
                                msg.Counter.SetVisible(true);
                                player.AddCounter(msg.Counter);
                                Game.faceUpCounters.Add(Game.counterPile[0]);
                                Game.counterPile.RemoveAt(0);

                                Game.finishedPhase.Add(player);
                                if (Game.finishedPhase.Count == Game.participants.Count)
                                {
                                    if (Game.phase == GamePhase.DrawCounterOnePhase)
                                    {
                                        Game.phase = GamePhase.DrawCounterTwoPhase;
                                    }
                                    else if (Game.phase == GamePhase.DrawCounterTwoPhase)
                                    {
                                        Game.phase = GamePhase.DrawCounterThreePhase;
                                    }
                                    else
                                    {
                                        Game.phase = GamePhase.PlaceCounter;
                                    }
                                    Game.currentPlayer = Game.participants[0];
                                    Game.finishedPhase.Clear();
                                }
                                else
                                {
                                    int pIndex = Game.participants.FindIndex(Game.IsCurrentPlayer);
                                    pIndex += 1;
                                    Game.currentPlayer = Game.participants[pIndex];
                                }

                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.DrawRandomCard":
                        {
                            var msg = JsonConvert.DeserializeObject<ClientToServer.DrawRandomCard>(deserialized.Content);
                            // work with msg
                            if (Verification.VerifyDrawRandomCard(player, this))
                            {

                                Card CardDealt = Game.cardDeck[0];
                                if (CardDealt.type != Card.CardType.GOLD)
                                {
                                    player.AddCard(CardDealt);
                                }
                                else
                                {
                                    Game.goldDeck.Add(CardDealt);
                                    Game.RemoveFirstCard();
                                    break;
                                }
                                Game.RemoveFirstCard();

                                Game.finishedPhase.Add(player);
                                if (Game.finishedPhase.Count == Game.participants.Count)
                                {
                                    if (Game.phase == GamePhase.DrawCardOnePhase)
                                    {
                                        Game.phase = GamePhase.DrawCardTwoPhase;
                                    }
                                    else if (Game.phase == GamePhase.DrawCardTwoPhase)
                                    {
                                        Game.phase = GamePhase.DrawCardThreePhase;
                                    }
                                    else
                                    {
                                        for (int i = 0; i < Game.participants.Count; i++)
                                        {
                                            Game.participants[i].AddGold(2);

                                            //deal the two counters for the next phase
                                            Counter CounterDealt = Game.counterPile[0];
                                            CounterDealt.SetVisible(false);
                                            Game.counterPile.RemoveAt(0);
                                            Game.participants[i].schrodingerCounters.Add(CounterDealt);
                                            Counter CounterDealt2 = Game.counterPile[0];
                                            CounterDealt2.SetVisible(false);
                                            Game.participants[i].schrodingerCounters.Add(CounterDealt2);
                                            Game.counterPile.RemoveAt(0);
                                        }
                                        Game.phase = GamePhase.ChooseCounterPhase;
                                    }
                                    Game.currentPlayer = Game.participants[0];
                                    Game.finishedPhase.Clear();
                                }
                                else
                                {
                                    int pIndex = Game.participants.FindIndex(Game.IsCurrentPlayer);
                                    pIndex += 1;
                                    Game.currentPlayer = Game.participants[pIndex];
                                }
                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.DrawRandomCounter":
                        {

                            var msg = JsonConvert.DeserializeObject<ClientToServer.DrawRandomCounter>(deserialized.Content);
                            // work with msg
                            if (Verification.VerifyDrawRandomCounter(player, this))
                            {
                                Game.ShuffleCounterPile();
                                Game.counterPile[0].SetVisible(true);
                                player.AddCounter(Game.counterPile[0]);
                                Game.counterPile.RemoveAt(0);

                                Game.finishedPhase.Add(player);
                                if (Game.finishedPhase.Count == Game.participants.Count)
                                {
                                    if (Game.phase == GamePhase.DrawCounterOnePhase)
                                    {
                                        Game.phase = GamePhase.DrawCounterTwoPhase;
                                    }
                                    else if (Game.phase == GamePhase.DrawCounterTwoPhase)
                                    {
                                        Game.phase = GamePhase.DrawCounterThreePhase;
                                    }
                                    else
                                    {
                                        Game.phase = GamePhase.PlaceCounter;
                                    }
                                    Game.currentPlayer = Game.participants[0];
                                    Game.finishedPhase.Clear();
                                }
                                else
                                {
                                    int pIndex = Game.participants.FindIndex(Game.IsCurrentPlayer);
                                    pIndex += 1;
                                    Game.currentPlayer = Game.participants[pIndex];
                                }
                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.EndTurn":
                        {

                            var msg = JsonConvert.DeserializeObject<ClientToServer.EndTurn>(deserialized.Content);
                            // work with msg
                            if (Verification.VerifyEndTurn(player, this))
                            {
                                Game.finishedPhase.Add(player);
                                if (Game.finishedPhase.Count == Game.participants.Count)
                                {
                                    bool winnerExists = false;
                                    foreach (Player p in Game.participants)
                                    {
                                        if (p.NumOfTownVisited() == 21)
                                        {
                                            winnerExists = true;
                                        }
                                    }
                                    if (Game.curRound < Game.numRounds && !winnerExists)
                                    {
                                        Game.currentPlayer = Game.participants[0];
                                        if (Game.variant == Variant.ELFENGOLD)
                                        {
                                            Game.phase = GamePhase.EndOfMoveBoot;
                                            Game.finishedPhase.Clear();
                                        }
                                        else
                                        {
                                            Game.phase = GamePhase.EndOfRound;
                                            foreach (Player p in Game.participants)
                                            {
                                                if (p.NumOfCountersHeld() > 0)
                                                {
                                                    Game.finishedPhase.Remove(p);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        FinishGame();
                                    }
                                }
                                else
                                {
                                    int pIndex = Game.participants.FindIndex(Game.IsCurrentPlayer);
                                    pIndex += 1;
                                    Game.currentPlayer = Game.participants[pIndex];
                                }
                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.PassTurn":
                        {
                            var msg = JsonConvert.DeserializeObject<ClientToServer.PassTurn>(deserialized.Content);
                            // work with msg
                            if (Verification.VerifyPassTurn(player, this))
                            {
                                if (Game.phase == GamePhase.Auction)
                                {
                                    Game.auction.passPlayer(player);

                                    // If no more players are left in Auction, give card to winner and make transaction
                                    if (Game.auction.playersInAuction.Count == 1 && Game.auction.currentBid > 0)
                                    {
                                        Game.auction.leadingBidPlayer.AddCounter(Game.auction.upForAuction.Dequeue());
                                        Game.auction.leadingBidPlayer.gold -= Game.auction.currentBid;
                                        Game.auction.refreshAuction();
                                    }
                                    else if (Game.auction.playersInAuction.Count == 0)
                                    {
                                        Game.counterPile.Add(Game.auction.upForAuction.Dequeue());
                                        Game.auction.refreshAuction();
                                    }
                                    Game.currentPlayer = Game.auction.playersInAuction.Peek();

                                    // If no more cards to auction, go to PlaceCounter phase
                                    if (Game.auction.upForAuction.Count == 0)
                                    {
                                        Game.phase = GamePhase.PlaceCounter;
                                        Game.currentPlayer = Game.participants[0];
                                    }
                                }
                                else
                                {
                                    Game.finishedPhase.Add(player);
                                    if (Game.finishedPhase.Count == Game.participants.Count)
                                    {
                                        Game.phase = GamePhase.MoveBoot;
                                        Game.finishedPhase.Clear();
                                        Game.currentPlayer = Game.participants[0];
                                    }
                                    else
                                    {
                                        int pIndex = Game.participants.FindIndex(Game.IsCurrentPlayer);
                                        if (Game.participants.Count - 1 == pIndex)
                                        {
                                            Game.currentPlayer = Game.participants[0];
                                        }
                                        else
                                        {
                                            pIndex += 1;
                                            Game.currentPlayer = Game.participants[pIndex];
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.PlaceBid":
                        {
                            // TODO implement for ELFENGOLD, not necessary for ELFENLAND

                            var msg = JsonConvert.DeserializeObject<ClientToServer.PlaceBid>(deserialized.Content);
                            // work with msg
                            if (Verification.VerifyPlaceBid(player, msg.bid, this))
                            {
                                Game.auction.SetNewBid(msg.bid, player);
                                if (Game.auction.playersInAuction.Count == 1)
                                {
                                    Game.auction.leadingBidPlayer.AddCounter(Game.auction.upForAuction.Dequeue());
                                    Game.auction.leadingBidPlayer.gold -= Game.auction.currentBid;
                                    Game.auction.refreshAuction();

                                    // If no more cards to auction, go to PlaceCounter phase
                                    if (Game.auction.upForAuction.Count == 0)
                                    {
                                        Game.phase = GamePhase.PlaceCounter;
                                    }
                                }
                                else
                                {
                                    Game.auction.playersInAuction.Dequeue();
                                    Game.auction.playersInAuction.Enqueue(player);
                                }
                                Game.currentPlayer = Game.auction.playersInAuction.Peek();
                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.PlaceCounter":
                        {

                            var msg = JsonConvert.DeserializeObject<ClientToServer.PlaceCounter>(deserialized.Content);
                            // work with msg
                            if (Verification.VerifyPlaceCounter(player, msg.Road, msg.Counter, this))
                            {
                                Game.finishedPhase.Clear();
                                player.RemoveCounter(msg.Counter);
                                Road road = Game.GetRoad(msg.Road.source.townName, msg.Road.dest.townName, msg.Road.region);
                                if (road == null)
                                {
                                    Console.Error.WriteLine("Invalid Road");
                                    reject(user, tag);
                                }

                                road.AddCounter(msg.Counter);

                                int pIndex = Game.participants.FindIndex(Game.IsCurrentPlayer);
                                if (Game.participants.Count - 1 == pIndex)
                                {
                                    Game.currentPlayer = Game.participants[0];
                                }
                                else
                                {
                                    pIndex += 1;
                                    Game.currentPlayer = Game.participants[pIndex];
                                }

                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.PlayDoubleSpell":
                        {
                            var msg = JsonConvert.DeserializeObject<ClientToServer.PlayDoubleSpell>(deserialized.Content);
                            // work with msg
                            if (Verification.VerifyPlayDoubleSpell(player, msg.Counter, msg.Road, this))
                            {
                                Road road = Game.GetRoad(msg.Road.source.townName, msg.Road.dest.townName, msg.Road.region);
                                if (road == null)
                                {
                                    Console.Error.WriteLine("Invalid Road");
                                    reject(user, tag);
                                }
                                Counter DoubleSpell = new Counter(Counter.CounterType.DOUBLESPELL);
                                player.RemoveCounter(DoubleSpell);
                                player.RemoveCounter(msg.Counter);

                                road.AddCounter(DoubleSpell);
                                road.AddCounter(msg.Counter);

                                Game.finishedPhase.Clear();

                                int pIndex = Game.participants.FindIndex(Game.IsCurrentPlayer);
                                if (Game.participants.Count - 1 == pIndex)
                                {
                                    Game.currentPlayer = Game.participants[0];
                                }
                                else
                                {
                                    pIndex += 1;
                                    Game.currentPlayer = Game.participants[pIndex];
                                }
                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.PlayExchangeSpell":
                        {
                            var msg = JsonConvert.DeserializeObject<ClientToServer.PlayExchangeSpell>(deserialized.Content);
                            // work with msg
                            if (Verification.VerifyPlayExchangeSpell(player, msg.First, msg.Second, msg.CounterOne, msg.CounterTwo, this))
                            {
                                Road first = Game.GetRoad(msg.First.source.townName, msg.First.dest.townName, msg.First.region);
                                if (first == null)
                                {
                                    Console.Error.WriteLine("Invalid Road");
                                    reject(user, tag);
                                }

                                Road second = Game.GetRoad(msg.Second.source.townName, msg.Second.dest.townName, msg.Second.region);
                                if (second == null)
                                {
                                    Console.Error.WriteLine("Invalid Road");
                                    reject(user, tag);
                                }
                                Counter ExchangeSpell = new Counter(Counter.CounterType.EXCHANGESPELL);
                                Game.counterPile.Add(ExchangeSpell);
                                Game.ShuffleCounterPile();
                                player.RemoveCounter(ExchangeSpell);

                                first.RemoveCounter(msg.CounterOne);
                                first.AddCounter(msg.CounterTwo);
                                second.RemoveCounter(msg.CounterTwo);
                                second.AddCounter(msg.CounterOne);

                                Game.finishedPhase.Clear();

                                int pIndex = Game.participants.FindIndex(Game.IsCurrentPlayer);
                                if (Game.participants.Count - 1 == pIndex)
                                {
                                    Game.currentPlayer = Game.participants[0];
                                }
                                else
                                {
                                    pIndex += 1;
                                    Game.currentPlayer = Game.participants[pIndex];
                                }
                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.TravelOnRoad":
                        {

                            var msg = JsonConvert.DeserializeObject<ClientToServer.TravelOnRoad>(deserialized.Content);
                            // work with msg
                            if (Verification.VerifyMoveBoot(player, msg.Road, msg.Cards, msg.isCaravan, this))
                            {
                                bool hasGold = false;
                                Road r = Game.GetRoad(msg.Road.source.getName(), msg.Road.dest.getName(), msg.Road.region);
                                List<Road> roads = Game.GetRoadsBetween(r.dest, r.source);
                                if (r.ContainsGold())
                                {
                                    hasGold = true;
                                }
                                if (r.source.getName() == player.GetLocation().getName())
                                {
                                    player.MoveTo(r.dest, hasGold);
                                }
                                else
                                {
                                    player.MoveTo(r.source, hasGold);
                                }
                                foreach (Card card in msg.Cards)
                                {
                                    Game.discardPile.Add(card);
                                }
                                player.RemoveCards(msg.Cards);
                                foreach (Road road in roads)
                                {
                                    Road gameRoad = Game.GetRoad(road.source.getName(), road.dest.getName(), road.region);
                                    gameRoad.witchUsed = false;
                                }

                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.UseWitchForFlight":
                        {
                            var msg = JsonConvert.DeserializeObject<ClientToServer.UseWitchForFlight>(deserialized.Content);
                            Town gameTown = Game.towns[msg.Town.getName()];
                            if (gameTown == null)
                            {
                                reject(user, "The town name does not exist on server");
                                return;
                            }

                            // work with msg
                            if (Verification.VerifyUseWitchForFlight(player, this))
                            {
                                Card witch = new Card(Card.CardType.WITCH);
                                Game.discardPile.Add(witch);
                                player.RemoveCard(witch);
                                player.SetLocation(msg.Town);
                                player.visited.Add(msg.Town);
                                player.gold -= 3;
                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.UseWitchForObstacle":
                        {
                            var msg = JsonConvert.DeserializeObject<ClientToServer.UseWitchForObstacle>(deserialized.Content);
                            Road road = Game.GetRoad(msg.Road.source.getName(), msg.Road.dest.getName(), msg.Road.region);


                            // work with msg
                            if (Verification.VerifyUseWitchForObstacle(player, road, this))
                            {
                                Card witch = new Card(Card.CardType.WITCH);
                                Game.discardPile.Add(witch);
                                player.RemoveCard(witch);
                                player.gold--;
                                road.witchUsed = true;
                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.ChooseBoot":
                        {

                            var msg = JsonConvert.DeserializeObject<ClientToServer.ChooseBoot>(deserialized.Content);
                            // work with msg
                            if (Verification.VerifyChooseBoot(player, msg.Color, this))
                            {
                                Game.ChosenColors.Add(msg.Color);
                                player.SetColor(msg.Color);
                                player.selectedBoot = true;
                                if (Game.ChosenColors.Count == Game.participants.Count)
                                {
                                    Game.currentPlayer = Game.participants[0];

                                    if (Game.variant == Variant.ELFENLAND)
                                    {
                                        Game.phase = GamePhase.DrawCounterOnePhase;
                                    }
                                    else
                                    {
                                        for (int i = 0; i < Game.participants.Count; i++)
                                        {
                                            //deal the two counters for the next phase
                                            Counter CounterDealt = Game.counterPile[0];
                                            CounterDealt.SetVisible(false);
                                            Game.counterPile.RemoveAt(0);
                                            Game.participants[i].schrodingerCounters.Add(CounterDealt);
                                            Counter CounterDealt2 = Game.counterPile[0];
                                            CounterDealt2.SetVisible(false);
                                            Game.participants[i].schrodingerCounters.Add(CounterDealt2);
                                            Game.counterPile.RemoveAt(0);
                                        }
                                        Game.phase = GamePhase.ChooseCounterPhase;
                                    }
                                }

                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.TakeGoldDeck":
                        {
                            if (Verification.VerifyTakeGoldDeck(player, this))
                            {
                                for (int i = 0; i < Game.goldDeck.Count; i++)
                                {
                                    player.AddGold(3);
                                    Game.discardPile.Add(Game.goldDeck[i]);
                                }
                                Game.goldDeck.Clear();

                                Game.finishedPhase.Add(player);
                                if (Game.finishedPhase.Count == Game.participants.Count)
                                {
                                    if (Game.phase == GamePhase.DrawCardOnePhase)
                                    {
                                        Game.phase = GamePhase.DrawCardTwoPhase;
                                    }
                                    else if (Game.phase == GamePhase.DrawCardTwoPhase)
                                    {
                                        Game.phase = GamePhase.DrawCardThreePhase;
                                    }
                                    else
                                    {
                                        for (int i = 0; i < Game.participants.Count; i++)
                                        {
                                            Game.participants[i].AddGold(2);

                                            //deal the two counters for the next phase
                                            Counter CounterDealt = Game.counterPile[0];
                                            Game.counterPile.RemoveAt(0);
                                            CounterDealt.SetVisible(false);
                                            Game.participants[i].schrodingerCounters.Add(CounterDealt);
                                            Counter CounterDealt2 = Game.counterPile[0];
                                            CounterDealt2.SetVisible(false);
                                            Game.participants[i].schrodingerCounters.Add(CounterDealt2);
                                            Game.counterPile.RemoveAt(0);
                                        }
                                        Game.phase = GamePhase.ChooseCounterPhase;
                                    }
                                    Game.currentPlayer = Game.participants[0];
                                    Game.finishedPhase.Clear();
                                }
                                else
                                {
                                    int pIndex = Game.participants.FindIndex(Game.IsCurrentPlayer);
                                    pIndex += 1;
                                    Game.currentPlayer = Game.participants[pIndex];
                                }
                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.ChooseCounterToMakeHidden":
                        {
                            var msg = JsonConvert.DeserializeObject<ClientToServer.ChooseCounterToMakeHidden>(deserialized.Content);

                            if (Verification.VerifyChooseCounterToMakeHidden(player, msg.Invis, this))
                            {
                                msg.Invis.SetVisible(false);
                                player.schrodingerCounters.Remove(player.schrodingerCounters.Where(item => item.type == msg.Invis.type).First());
                                player.AddCounter(msg.Invis);
                                foreach (Counter c in player.schrodingerCounters)
                                {
                                    c.SetVisible(true);
                                    player.AddCounter(c);
                                }
                                player.schrodingerCounters.Clear();

                                Game.finishedPhase.Add(player);
                                if (Game.finishedPhase.Count == Game.participants.Count)
                                {
                                    Game.phase = GamePhase.Auction;
                                    Game.currentPlayer = Game.participants[0];
                                    Game.auction = new Auction();
                                    Game.auction.SetupNewAuction();
                                    Game.finishedPhase.Clear();
                                }
                            }
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.RequestSave":
                        {
                            Console.WriteLine("SAVING=====================");
                            saveGame();
                            break;
                        }
                    case "Elfencore.Shared.Messages.ClientToServer.CountersToKeep":
                        {
                            var msg = JsonConvert.DeserializeObject<ClientToServer.CountersToKeep>(deserialized.Content);
                            // work with msg
                            if (Verification.VerifyCountersToKeep(player, this, msg.Counters))
                            {
                                // In case of verification success, update GameState
                                player.RemoveCountersExcept(msg.Counters);
                                Game.finishedPhase.Add(player);
                                // Q: are counters secret to each player, or can everyone know?

                                if (Game.finishedPhase.Count == Game.participants.Count)
                                {
                                    // in case of new round
                                    NewRound();
                                }
                                else
                                {
                                    int pIndex = Game.participants.FindIndex(Game.IsCurrentPlayer);
                                    pIndex += 1;
                                    Game.currentPlayer = Game.participants[pIndex];
                                }
                            }
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Invalid message");
                            // invalid msg
                            break;
                        }
                }
                broadcastGame();
            }
            catch (Exception e)
            {
                Console.WriteLine("error parsing payload message");
                Console.WriteLine(e);
            }
        }

        public static void NewRound()
        {
            Game.finishedPhase.Clear();

            // cycle starting player
            Player tmp = Game.participants[0];
            Game.participants.RemoveAt(0);
            Game.participants.Add(tmp);

            Game.currentPlayer = Game.participants[0];
            // clear roads
            foreach (Road road in Game.roads)
            {
                List<Counter> counters = road.GetCounters();
                foreach (Counter counter in counters)
                {
                    if (counter.IsTrasportCounter())
                    {
                        Game.counterPile.Add(counter);
                    }

                }
                road.RemoveCounters();
            }
            if (Game.variant == Variant.ELFENLAND)
            {
                // draw cards for players
                foreach (Player player in Game.participants)
                {
                    while (player.NumOfCardsHeld() < 8)
                    {
                        Card cardDealt = Game.cardDeck[0];
                        player.AddCard(cardDealt);
                        Game.RemoveFirstCard();
                    }
                }

                // give out a secret Counter
                foreach (Player p in Game.participants)
                {
                    Counter counterDealt = Game.counterPile[0];
                    counterDealt.SetVisible(false);
                    p.AddCounter(counterDealt);
                    Game.counterPile.RemoveAt(0);
                }

                // increment round and change phase
                Game.curRound++;
                Game.phase = GamePhase.DrawCounterOnePhase;
            }
            else
            {
                Game.curRound++;
                Game.phase = GamePhase.DrawCardOnePhase;
            }
        }

        private static void FinishGame()
        {
            Player winner = Game.GetCurrentWinner();
            Game.winner = winner;
            Game.winnerDeclared = true;
            Game.phase = GamePhase.EndOfRound;
            Console.WriteLine("WINNER: " + winner.GetName());
        }
    }
}