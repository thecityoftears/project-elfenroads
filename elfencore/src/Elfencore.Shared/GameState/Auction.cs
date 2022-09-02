using System.Collections.Generic;


namespace Elfencore.Shared.GameState
{
    public class Auction
    {
        public int currentBid;
        public Player leadingBidPlayer;
        public Queue<Counter> upForAuction = new Queue<Counter>();
        public Queue<Player> playersInAuction = new Queue<Player>();

        public void SetupNewAuction()
        {
            upForAuction.Clear();
            //Gets twice as many counters as there are players from CounterPile to set them up
            for (int i = 0; i < Game.getNoPlayers() * 2; i++)
            {
                Counter toAuction = Game.counterPile[0];
                Game.counterPile.Remove(toAuction);
                upForAuction.Enqueue(toAuction);
            }
            refreshAuction();
        }

        public void refreshAuction()
        {
            currentBid = 0;
            leadingBidPlayer = null;
            playersInAuction.Clear();
            foreach (Player p in Game.participants)
            {
                playersInAuction.Enqueue(p);
            }
        }

        public int GetCurrentBid()
        {
            return currentBid;
        }

        public Player GetLeadingPlayer()
        {
            return leadingBidPlayer;
        }

        public void SetNewBid(int bid, Player player)
        {
            leadingBidPlayer = player;
            currentBid = bid;
        }

        public void passPlayer(Player p)
        {
            playersInAuction.Dequeue();
        }
    }
}