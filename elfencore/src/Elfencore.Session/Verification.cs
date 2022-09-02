using System;
using Elfencore.Shared.GameState;

namespace Elfencore.Session {
    public static class Verification {
        /// <summary> Checks that the player is the current player and also has the necessary counter </summary>
        public static bool VerifyChooseCounterToKeep(Player p, Counter c, Session s) {
            if (Game.phase != GamePhase.EndOfRound) {
                s.reject(p.GetName(), "Can't keep counter: We are not in the EndOfRoundPhase");
                return false;
            } else if (!p.ContainsCounterType(c)) {
                s.reject(p.GetName(), "Can't keep counter: " + p.GetName() + " does not have counter of type " + c.type.ToString());
                return false;
            } else if (Game.finishedPhase.FindIndex(pl => pl.GetName() == p.GetName()) != -1) {
                s.reject(p.GetName(), "Can't keep counter: " + p.GetName() + " already finished the phase");
                return false;
            } else if (Game.variant != Variant.ELFENLAND) {
                s.reject(p.GetName(), "Can't keep counter: We are not playing Elfenland");
                return false;
            } else
                return true;
        }
        /// <summary> Verifies that the chooseToGetGoldForTravel request is valid.
        /// Checks that the variant is Elfengold, the player is the current player, the game phase is MoveBoot </summary>
        public static bool VerifyChooseToGetGoldForTravel(Player p, Session s) {
            if (Game.variant != Variant.ELFENGOLD) {
                s.reject(p.GetName(), "Can't choose get gold for travel: We are not playing Elfengold");
                return false;
            } else if (Game.phase != GamePhase.EndOfMoveBoot) {
                s.reject(p.GetName(), "Can't choose get gold for travel: It is not currently the end of the move boot phase");
                return false;
            } else if (Game.finishedPhase.FindIndex(pl => pl.GetName() == p.GetName()) != -1) {
                s.reject(p.GetName(), "Can't choose get gold for travel: " + p.GetName() + " already finished the phase");
                return false;
            } else
                return true;
        }
        /// <summary> Verifies that the drawCard request is valid.
        /// Checks that the variant is Elfengold (no draw choice in elfenland), the player is the current player, the game is in a drawCardPhase, and that the card chosen is face-up </summary>
        public static bool VerifyDrawCard(Player p, Card c, Session s) {
            if (Game.variant != Variant.ELFENGOLD) {
                s.reject(p.GetName(), "Can't draw card: We are not playing Elfengold");
                return false;
            } else if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't draw card: " + p.GetName() + " is not the current player");
                return false;
            } else if (!Game.IsDrawCardPhase()) {
                s.reject(p.GetName(), "Can't draw card: it is not a draw card phase");
                return false;
            } else if (!Game.FaceUpCardContains(c)) {
                s.reject(p.GetName(), "Can't draw card: the game does not have a faceup counter of type " + c.type.ToString());
                return false;
            } else
                return true;
        }
        /// <summary>  Verifies that the drawCounter request is valid.
        /// Checks that the player is the current player, the game is in a drawCounterPhase, 
        /// the player has not surpassed their max number of counters (5), and that the Counter requested is faceup </summary>
        public static bool VerifyDrawCounter(Player p, Counter c, Session s) {
            if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't draw counter: " + p.GetName() + " is not the current player");
                return false;
            } else if (!Game.IsDrawCounterPhase()) {
                s.reject(p.GetName(), "Can't draw counter: it is not a draw counter phase");
                return false;
            } else if (!(p.NumOfCountersHeld() <= 5)) {
                s.reject(p.GetName(), "Can't draw counter: " + p.GetName() + " already has 5 counters");
                return false;
            } else if (!Game.FaceUpCounterContains(c)) {
                s.reject(p.GetName(), "Can't draw counter: the game state does not have a face up counter of type " + c.type.ToString());
                return false;
            } else
                return true;
        }
        /// <summary> Verifies that the drawRandomCard request is valid.
        /// Checks that the variant is Elfengold (no draw choice in elfenland), the player is the current player, and the game is in a drawCardPhase </summary>
        public static bool VerifyDrawRandomCard(Player p, Session s) {
            if (Game.variant != Variant.ELFENGOLD) {
                s.reject(p.GetName(), "Can't draw card: We are not playing Elfengold");
                return false;
            } else if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't draw card: " + p.GetName() + " is not the current player");
                return false;
            } else if (!Game.IsDrawCardPhase()) {
                s.reject(p.GetName(), "Can't draw random card: it is not a draaw card phase");
                return false;
            } else
                return true;
        }
        /// <summary> Verifies that the drawRandomCounter request is valid.
        /// Checks that the player is the current player, the game is in a drawCounterPhase, and the player doesnt have more than the max counters (5) </summary>
        public static bool VerifyDrawRandomCounter(Player p, Session s) {
            if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't draw counter: " + p.GetName() + " is not the current player");
                return false;
            } else if (!Game.IsDrawCounterPhase()) {
                s.reject(p.GetName(), "Can't draw random counter: it is not a counter draw phase");
                return false;
            } else if (p.NumOfCountersHeld() > 5) {
                s.reject(p.GetName(), "Player " + p.GetName() + " already has 5 counters held!");
                return false;
            } else
                return true;
        }
        /// <summary> Verifies that the endTurn request is valid.
        /// Checks that the game phase is MoveBoot, and the player is the current player</summary>
        public static bool VerifyEndTurn(Player p, Session s) {
            if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't draw counter: " + p.GetName() + " is not the current player");
                return false;
            } else if (Game.phase != GamePhase.MoveBoot) {
                s.reject(p.GetName(), "Can't end turn:" + p.GetName() + "haven't moved their boot");
                return false;
            } else
                return true;
        }
        /// <summary> Verifies that the moveBoot request is valid. </summary>
        /// <param name="p"></param>
        public static bool VerifyMoveBoot(Player p, Road r, List<Card> cardsUsed, bool isCaravan, Session s) {
            bool atSrc = p.GetLocation().getName() == r.source.getName(); // for rivers
            int cardsNeeded = 0;

            if (Game.phase != GamePhase.MoveBoot) {
                s.reject(p.GetName(), "Can't move boot: " + p.GetName() + " in the move boot phase.");
                return false;
            } else if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't move boot: Not Player's turn (" + p.GetName() + ")");
                return false;
            } else if (!atSrc && p.GetLocation().getName() != r.dest.getName()) {
                s.reject(p.GetName(), "Can't move boot: " + p.GetName() + " is not at the source or the destination is incorrect");
                return false;
            }else if (!p.CardsRemovable(cardsUsed))
            {
                s.reject(p.GetName(), "Can't move boot: Not enough cards in hand");
                return false;
            }

            // apply obstacle contributions
            if (r.ContainsObstacle())
                cardsNeeded++;

            // special rules for waterways
            if (r.region == Region.RIVER || r.region == Region.LAKE) {
                if (!allSameType(cardsUsed)) {
                    s.reject(p.GetName(), "Not all card are of the same type.");
                    return false;
                }
                if (r.region == Region.LAKE) {
                    cardsNeeded += 2;
                } else { // river
                    if (atSrc) {
                        cardsNeeded += 1;
                    } else {
                        cardsNeeded += 2;
                    }
                }
            } else {
                if (isCaravan) { // means forced to use caravan
                    cardsNeeded += 3; // IMPORTANT: I am assuming that caravans do not require a transport counter
                } else { // all the same types of cards
                         // roads must countain a transportCounter identical to the cards
                    if (!r.ContainsTransportCounter() || !r.ValidTransportType(cardsUsed[0].GetTransportType())) {
                        s.reject(p.GetName(), "Road does not contain a Transport Counter identical to the cards");
                        return false;
                    }
                    // get the cards needed for this trasportType / region pair
                    KeyValuePair<TransportType, Region> key = new KeyValuePair<TransportType, Region>(cardsUsed[0].GetTransportType(), r.region);
                    if (!Game.travelValues.ContainsKey(key)) {
                        s.reject(p.GetName(), "Not such card for this (transportType, Region) pair");
                        return false;
                    } else {
                        cardsNeeded += Game.travelValues.GetValueOrDefault(key, 0);
                    }
                }
            }

            if (r.witchUsed) {
                cardsNeeded--;
            }

            if (cardsUsed.Count < cardsNeeded) {
                s.reject(p.GetName(), "Not enough cards!");
                return false;
            } else
                return true;
        }

        private static bool allSameType(List<Card> cards) {
            if (cards.Count == 0)
                return true;

            foreach (Card c in cards) {
                if (!c.SameType(cards[0])) {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Stub: Assume that the player is always allowed to pass their turn if they are current player, might not be true
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool VerifyPassTurn(Player p, Session s) {
            if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't pass turn: Not Players turn");
                return false;
            } else if (!(Game.phase == GamePhase.Auction || Game.phase == GamePhase.PlaceCounter)) {
                s.reject(p.GetName(), "Can't pass turn: unskippable phase");
                return false;
            }
            return true;
        }
        /// <summary> Verifies that the placeBid request is valid.
        /// Checks that the variant is Elfengold, the player is the current player, the game phase is Auction, the bid placed is higher than the current bid, and that the player has enough gold to place their bid </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool VerifyPlaceBid(Player p, int bid, Session s) {
            if (Game.variant != Variant.ELFENGOLD) {
                s.reject(p.GetName(), "Can't bid: Not playing Elfengold. BUY THE EXPANSION FOR 99.99$ IN AL DEPANNEURS");
                return false;
            } else if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't bid (" + p.GetName() + "): Not players turn");
                return false;
            } else if (Game.phase != GamePhase.Auction) {
                s.reject(p.GetName(), "Can't bid: Not Auction phase");
                return false;
            } else if (Game.auction.GetCurrentBid() >= bid) {
                s.reject(p.GetName(), "Can't bid: Bid amount is not larger than current Bid.");
                return false;
            } else if (bid > p.amountOfGold()) {
                s.reject(p.GetName(), "Can't bid: " + p.GetName() + " doesn't have enough gold");
                return false;
            } else
                return true;
        }
        /// <summary> Verifies that the placeCounter request is valid. </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool VerifyPlaceCounter(Player p, Road r, Counter c, Session s) {
            if (Game.phase != GamePhase.PlaceCounter) {
                s.reject(p.GetName(), "Can't place counter: Incorrect phase");
                return false;
            } else if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't place counter: Not players turn");
            }




            if (c.IsTrasportCounter()) {
                if (r.ContainsTransportCounter()) {
                    s.reject(p.GetName(), "Road already contains a transport counter!");
                    return false;
                } else if (!c.CanTravel(r.region)) {
                    s.reject(p.GetName(), "Can't place Counter: Incompatible road region");
                    return false;
                } else if (!p.ContainsCounterType(c)) {
                    s.reject(p.GetName(), "Can't place Counter: No such counter");
                } else
                    return true;
            } else if (c.type == Counter.CounterType.SEAOBS) {
                if (r.ContainsObstacle())
                {
                    s.reject(p.GetName(), "Can't place Counter: Road contains an obstacle");
                    return false;
                }
                else if (r.ContainsGold())
                {
                    s.reject(p.GetName(), "Can't place Counter: Road contains Gold Counter");
                    return false;
                }
                else if (!c.ValidHere(r.region))
                {
                    s.reject(p.GetName(), "Can't place Counter: Invalid region");
                    return false;
                }
                else if (!p.ContainsCounterType(c))
                {
                    s.reject(p.GetName(), "Can't place Counter: Player does not have this Counter Type");
                    return false;

                }
                else
                    return true;
            }
            else if (c.type == Counter.CounterType.TREEOBS)
            {
                if (r.ContainsObstacle())
                {
                    s.reject(p.GetName(), "Can't place Counter: Road contains an obstacle");
                    return false;
                }
                else if (r.ContainsGold())
                {
                    s.reject(p.GetName(), "Can't place Counter: Road contains Gold Counter");
                    return false;
                }
                else if (!c.ValidHere(r.region))
                {
                    s.reject(p.GetName(), "Can't place Counter: Invalid region");
                    return false;
                }
                else if (!r.ContainsTransportCounter())
                {
                    s.reject(p.GetName(), "Can't place Counter: Road doesn't contain Transportation Counter"); // ??
                    return false;
                }
                else if (!p.ContainsCounterType(c))
                {
                    s.reject(p.GetName(), "Can't place Counter: Player does not have this Counter Type");
                    return false;

                }
                else
                    return true;
            }
            else if (c.IsGold())
            {
                if (r.ContainsObstacle())
                {
                    s.reject(p.GetName(), "Can't place Counter: Road contains an obstacle");
                    return false;
                }
                else if (r.ContainsGold())
                {
                    s.reject(p.GetName(), "Can't place Counter: Road contains Gold Counter");
                    return false;
                }
                else if (!r.ContainsTransportCounter())
                {
                    s.reject(p.GetName(), "Can't place Counter: Road doesn't contain Transportation Counter"); // ??
                    return false;
                }
                else if (!p.ContainsCounterType(c))
                {
                    s.reject(p.GetName(), "Can't place Counter: Player does not have this Counter Type");
                    return false;

                }
                else
                    return true;
            }

            return true;
        }
        /// <summary>
        /// Checks that the player is the current player, the game phase is PlaceCounter, the variant is ELFENGOLD, that the player owns a gold counter, the road does not already have a gold counter/obstacle, and the road has a transportation counter placed
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool VerifyPlaceGoldCounter(Player p, Road r, Counter c, Session s) {
            if (Game.phase != GamePhase.PlaceCounter) {
                s.reject(p.GetName(), "Can't place Gold Counter: Not place Counter phase");
                return false;
            } else if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't place Gold Counter: Not players turn");
                return false;
            } else if (Game.variant != Variant.ELFENGOLD) {
                s.reject(p.GetName(), "Can't place Gold Counter: Not playing ELFENGOLD. BUY IT NOW (99.99$)");
                return false;
            } else {
                if (!p.ContainsCounterType(c)) {
                    s.reject(p.GetName(), "Can't place Gold Counter: Player does not have a corresponding Counter");
                    return false;
                } else if (r.ContainsObstacle()) {
                    s.reject(p.GetName(), "Can't place Gold Counter: Road contains an Obstacle");
                    return false;
                } else if (r.ContainsGold()) {
                    s.reject(p.GetName(), "Can't place Gold Counter: Road already contains a Gold Counter");
                    return false;
                } else if (!r.ContainsTransportCounter()) {
                    s.reject(p.GetName(), "Can't place Gold Counter: Road already contains a Counter");
                    return false;
                } else
                    return true;
            }
        }
        /// <summary> Verifies that the playDoubleSpell request is valid.
        /// Checks that the variant is Elfengold, the player is the current player, the game phase is PlaceCounter, that the player owns both a double spell and the additional counter they want to place, the counter is a TransportationCounter, the road already has a travel counter, and that the counter can be validly placed on the road</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool VerifyPlayDoubleSpell(Player p, Counter counterToAdd, Road r, Session s) {
            if (!counterToAdd.IsTrasportCounter()) {
                s.reject(p.GetName(), "Can't play DoubleSpell: Not a transportation counter!");
                return false;
            } else {
                if (Game.variant != Variant.ELFENGOLD) {
                    s.reject(p.GetName(), "Can't play DoubleSpell: Not playing ELFENGOLD");
                    return false;
                } else if (!Game.IsCurrentPlayer(p)) {
                    s.reject(p.GetName(), "Can't play DoubleSpell: Not your turn");
                    return false;
                } else if (Game.phase != GamePhase.PlaceCounter) {
                    s.reject(p.GetName(), "Can't play DoubleSpell: Not place Counter phase");
                    return false;
                } else if (!p.ContainsCounterType(counterToAdd)) {
                    s.reject(p.GetName(), "Can't play DoubleSpell: Not place Counter phase");
                    return false;
                } else if (!r.ContainsTransportCounter()) {
                    s.reject(p.GetName(), "Can't play DoubleSpell: Road does not contain a Transportaion Counter");
                    return false;
                } else if (!counterToAdd.CanTravel(r.region)) {
                    s.reject(p.GetName(), "Can't play DoubleSpell: Counter is not coompatible to Road region");
                    return false;
                } else
                    return true;
            }
        }
        /// <summary> Verifies that the playExchangeSpell request is valid.
        /// Checks that the variant is Elfengold, the player is the current player, the game phase is PlaceCounter, that the player owns an exchange spell, both roads already have a travel counter, and that the counters can be validly placed on the other road</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool VerifyPlayExchangeSpell(Player p, Road road1, Road road2, Counter counter1, Counter counter2, Session s) {

            if (Game.variant != Variant.ELFENGOLD) {
                s.reject(p.GetName(), "Can't play ExchangeSpell: Not playing ELFENGOLD");
                return false;
            } else if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't play ExchangeSpell: Not your turn");
                return false;
            } else if (Game.phase != GamePhase.PlaceCounter) {
                s.reject(p.GetName(), "Can't play ExchangeSpell: Not Place Counter Phase");
                return false;
            } else if (!road1.ContainsTransportCounter()) {
                s.reject(p.GetName(), "Can't play ExchangeSpell: Road1 contains a Counter");
                return false;
            } else if (!road2.ContainsTransportCounter()) {
                s.reject(p.GetName(), "Can't play ExchangeSpell: Road2 contains a Counter");
                return false;
            } else if (!counter1.CanTravel(road2.region)) {
                s.reject(p.GetName(), "Can't play ExchangeSpell: Road2 incompatible with Counter");
                return false;
            } else if (!counter2.CanTravel(road1.region)) {
                s.reject(p.GetName(), "Can't play ExchangeSpell: Road1 incompatible with Counter");
                return false;
            } else
                return true;
        }
        /// <summary> Verifies that the selectFaceDown request is valid.
        /// Checks that the variant is Elfengold, the player is the current player, the game is in a drawCounterPhase</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool VerifySelectFaceDown(Player p, Session s) {
            if (Game.variant != Variant.ELFENGOLD) {
                s.reject(p.GetName(), "Can't select face down Card: Not playing ELFENGOLD");
                return false;
            } else if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't select face down Card: Not your turn");
                return false;
            } else if (Game.phase != GamePhase.PlaceCounter) {
                s.reject(p.GetName(), "Can't select face down Card: Not Draw Counter phase");
                return false;
            } else
                return true;
        }
        /// <summary> Verifies that the undoTurn request is valid.
        /// Checks that the game phase is MoveBoot, and the player is the current player</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool VerifyUndoTurn(Player p, Session s) {
            if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't undo turn: Not your turn");
                return false;
            } else if (Game.phase != GamePhase.MoveBoot) {
                s.reject(p.GetName(), "Can't undo turn: Not Move Boot Phase");
                return false;
            } else
                return true;
        }
        /// <summary> Verifies that the useWitchForFlight request is valid.
        /// Checks that the game phase is MoveBoot, the player is the current player, and that the player has enough gold for the flight</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool VerifyUseWitchForFlight(Player p, Session s) {
            if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't fly with Witch: Not your turn");
                return false;
            } else if (Game.phase != GamePhase.MoveBoot) {
                s.reject(p.GetName(), "Can't fly with Witch: Not Move Boot Phase");
                return false;
            } else if (p.amountOfGold() < 3) {
                s.reject(p.GetName(), "Can't fly with Witch: not enough Gold");
                return false;
            } else
                return true;

        }
        /// <summary> Verifies that the useWitchForObstacle request is valid.
        /// Checks that the game phase is MoveBoot, the player is the current player, the road selected has an obstacle, and the player has enough gold to bypass the obstacle</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool VerifyUseWitchForObstacle(Player p, Road r, Session s) {
            if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't use Witch: Not your turn");
                return false;
            } else if (Game.phase != GamePhase.MoveBoot) {
                s.reject(p.GetName(), "Can't use Witch: Not Move Boot Phase");
                return false;
            } else if (!r.ContainsObstacle()) {
                s.reject(p.GetName(), "Can't use Witch: Road does not haave an obstacle");
                return false;
            } else if (p.amountOfGold() < 1) {
                s.reject(p.GetName(), "Can't use Witch: not enough Gold");
                return false;
            } else
                return true;
        }

        public static bool VerifyChooseBoot(Player p, Color c, Session s) {
            if (Game.ChosenColors.Contains(c)) {
                s.reject(p.GetName(), "Can't choose boot color: color already chosen");
                return false;
            }
            return true;
        }

        public static bool VerifyTakeGoldDeck(Player p, Session s) {
            if (Game.variant != Variant.ELFENGOLD) {
                s.reject(p.GetName(), "Can't take Gold Deck: Not playing ELFENGOLD");
                return false;
            } else if (!Game.IsCurrentPlayer(p)) {
                s.reject(p.GetName(), "Can't take Gold Deck: Not your turn");
                return false;
            } else if (Game.phase != GamePhase.DrawCardOnePhase && Game.phase != GamePhase.DrawCardTwoPhase && Game.phase != GamePhase.DrawCardThreePhase) {
                s.reject(p.GetName(), "Can't take Gold Deck: Not Draw Card phase");
                return false;
            } else if (Game.goldDeck.Count == 0) {
                s.reject(p.GetName(), "Can't take Gold Deck: No cards in Gold Deck");
                return false;
            } else
                return true;
        }

        public static bool VerifyChooseCounterToMakeHidden(Player p, Counter hidden, Session s) {
            if (Game.variant != Variant.ELFENGOLD) {
                s.reject(p.GetName(), "Can't hide Counter: Not playing ELFENGOLD. BUY IT NOW");
                return false;
            } else if (Game.phase != GamePhase.ChooseCounterPhase) {
                s.reject(p.GetName(), "Can't hide Counter: Not Choose Counter Phase");
                return false;
            } else
                return true;
        }

        public static bool VerifyCountersToKeep(Player p, Session s, List<Counter> counters) {
            if (Game.variant != Variant.ELFENGOLD) {
                s.reject(p.GetName(), "Can't keep Counter: Not playing ELFENGOLD. BUY IT NOW");
                return false;
            } else if (Game.phase != GamePhase.EndOfRound) {
                s.reject(p.GetName(), "Can't keep Counteer: Not End of Round Phase");
                return false;
            } else if (Game.finishedPhase.FindIndex(pl => pl.GetName() == p.GetName()) != -1) {
                s.reject(p.GetName(), "Can't keep Counter: Player out of the round");
                return false;
            } else if (!p.CountersRemovable(counters))
            {
                s.reject(p.GetName(), "Selected the same counter twice");
                return false;
            }
            else
                return true;
        }
    }
}

