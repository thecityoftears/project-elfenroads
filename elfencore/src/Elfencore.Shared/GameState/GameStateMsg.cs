using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

namespace Elfencore.Shared.GameState
{

    /// <summary> The conceptual container of game. Should not contain any verification. Methods should run as if they are valid. 
    /// The responsibility of verification lays withing another set of classes. </summary>
    public class GameStateMsg
    {
        public Variant variant;
        public int numRounds;
        public int curRound = 1;
        public GamePhase phase;
        public Player currentPlayer = null;
        public List<Player> participants = new List<Player>();
        public List<Color> ChosenColors = new List<Color>();
        public List<Player> finishedPhase = new List<Player>();
        public List<Counter> faceUpCounters = new List<Counter>();
        public List<Counter> counterPile = new List<Counter>();
        public List<Card> discardPile = new List<Card>();
        public List<Card> cardDeck = new List<Card>();
        public List<Card> faceUpCards = new List<Card>();
        public List<Card> goldDeck = new List<Card>(); // deck of gold cards - might be better to represent as int?
        public List<Road> roads = new List<Road>(); // All game roads
        public Dictionary<string, Town> towns = new Dictionary<string, Town>();
        public bool witchEnabled;
        public bool randomGold;
        public bool randomDest;
        public Auction auction;
        public Player winner = null;
        public bool winnerDeclared = false;

        public static GameStateMsg CreateMsg()
        {
            GameStateMsg msg = new GameStateMsg();
            msg.variant = Game.variant;
            msg.numRounds = Game.numRounds;
            msg.curRound = Game.curRound;
            msg.phase = Game.phase;
            msg.currentPlayer = Game.currentPlayer;
            msg.participants = Game.participants;
            msg.ChosenColors = Game.ChosenColors;
            msg.finishedPhase = Game.finishedPhase;
            msg.faceUpCounters = Game.faceUpCounters;
            msg.counterPile = Game.counterPile;
            msg.discardPile = Game.discardPile;
            msg.cardDeck = Game.cardDeck;
            msg.faceUpCards = Game.faceUpCards;
            msg.goldDeck = Game.goldDeck;
            msg.roads = Game.roads;
            msg.towns = Game.towns;
            msg.witchEnabled = Game.witchEnabled;
            msg.randomGold = Game.randomGold;
            msg.randomDest = Game.randomDest;
            msg.auction = Game.auction;
            msg.winner = Game.winner;
            msg.winnerDeclared = Game.winnerDeclared;

            return msg;
        }

        public static void ReadMsg(GameStateMsg msg)
        {
            Game.variant = msg.variant;
            Game.numRounds = msg.numRounds;
            Game.curRound = msg.curRound;
            Game.phase = msg.phase;
            Game.currentPlayer = msg.currentPlayer;
            Game.participants = msg.participants;
            Game.ChosenColors = msg.ChosenColors;
            Game.finishedPhase = msg.finishedPhase;
            Game.faceUpCounters = msg.faceUpCounters;
            Game.counterPile = msg.counterPile;
            Game.discardPile = msg.discardPile;
            Game.cardDeck = msg.cardDeck;
            Game.faceUpCards = msg.faceUpCards;
            Game.goldDeck = msg.goldDeck;
            Game.roads = msg.roads;
            Game.towns = msg.towns;
            Game.witchEnabled = msg.witchEnabled;
            Game.randomGold = msg.randomGold;
            Game.randomDest = msg.randomDest;
            Game.auction = msg.auction;
            Game.winner = msg.winner;
            Game.winnerDeclared = msg.winnerDeclared;
        }
    }
}