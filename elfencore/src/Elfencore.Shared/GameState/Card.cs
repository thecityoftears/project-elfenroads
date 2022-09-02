namespace Elfencore.Shared.GameState
{
    public class Card
    {
        public enum CardType
        {
            DRAGON,
            UNICORN,
            TROLLWAGON,
            ELFCYCLE,
            MAGICCLOUD,
            GIANTPIG,
            RAFT,
            GOLD,
            WITCH
        }

        public CardType type;

        public Card()
        { }

        public Card(CardType t)
        {
            type = t;
        }

        public Card(int typeNum)
        {
            type = (CardType)typeNum;
        }

        public bool IsTravelCard()
        {
            return type == CardType.DRAGON || type == CardType.UNICORN || type == CardType.TROLLWAGON || type == CardType.ELFCYCLE || type == CardType.MAGICCLOUD || type == CardType.GIANTPIG || type == CardType.RAFT;
        }

        public bool IsWitchCard()
        {
            return type == CardType.WITCH;
        }

        public bool IsGoldCard()
        {
            return type == CardType.GOLD;
        }

        public Card(TransportType t)
        {
            switch (t)
            {
                case (TransportType.DRAGON):
                    type = CardType.DRAGON;
                    break;

                case (TransportType.UNICORN):
                    type = CardType.UNICORN;
                    break;

                case (TransportType.TROLLWAGON):
                    type = CardType.TROLLWAGON;
                    break;

                case (TransportType.ELFCYCLE):
                    type = CardType.ELFCYCLE;
                    break;

                case (TransportType.MAGICCLOUD):
                    type = CardType.MAGICCLOUD;
                    break;

                case (TransportType.GIANTPIG):
                    type = CardType.GIANTPIG;
                    break;

                case (TransportType.RAFT):
                    type = CardType.RAFT;
                    break;
            }
        }

        /// <summary> Decides whether or not this Card is functionally identical to other.
        /// ONLY HANDLES TRANSPORT COUNTERS AT THE MOMENT </summary>
        /// <param name="other"> </param>
        /// <returns> whether or not they are functionally similar </returns>
        public bool SameType(Card other)
        {
            return type == other.type;
        }

        public TransportType GetTransportType()
        {
            switch (type)
            {
                case (CardType.DRAGON):
                    return TransportType.DRAGON;

                case (CardType.UNICORN):
                    return TransportType.UNICORN;

                case (CardType.TROLLWAGON):
                    return TransportType.TROLLWAGON;

                case (CardType.ELFCYCLE):
                    return TransportType.ELFCYCLE;

                case (CardType.MAGICCLOUD):
                    return TransportType.MAGICCLOUD;

                case (CardType.GIANTPIG):
                    return TransportType.GIANTPIG;

                case (CardType.RAFT):
                    return TransportType.RAFT;
            }
            return TransportType.RAFT; // JUST FOR TEMP
        }

        /*
        public bool canUse(Player player, Road road)
        {
            if (!road.ContainsObstacle())
            {
                switch (type)
                {
                    case (CardType.DRAGON):
                        {
                            if (road.region != Region.RIVER) { return true; }
                            break;
                        }
                    case (CardType.UNICORN):
                        {
                            if (road.region != Region.PLAINS && road.region != Region.RIVER) { return true; }
                            break;
                        }
                    case (Type.TROLLWAGON):
                        CardType
                            if (road.region != Region.RIVER) { return true; }
                            break;
                        }
                    case (CardType.ELFCYCLE):
                    case (CardType.MAGICCLOUD):
                        {
                            if (road.region != Region.DESERT && road.region != Region.RIVER) { return true; }
                            break;
                        }
                    case (CardType.GIANTPIG):
                        {
                            if (road.region != Region.MOUNTAIN && road.region != Region.DESERT && road.region != Region.RIVER) { return true; }
                            break;
                        }
                    case (CardType.RAFT):
                        {
                            if (road.region == Region.RIVER) { return true; }
                            break;
                        }
                };
            }
            return false;
        }

        public bool canUseWitch(Player player, Road road)
        {
            if (GetTransportType() != Type.WITCH)
                return false;
            if (player.amountOfGold() >= 1)
                return road.ContainsObstacle();
            else
                return false;
        }
        */
    }
}

/*
namespace Elfencore.Shared.GameState
{
    public abstract class Card
    {
        private bool visible;

        public Card()
        {
            visible = true;
        }

        public void SetVisible(bool visibility) { visible = visibility; }

        /// <summary> Decides whether or not this Card is functionally identical to other.
        /// ONLY HANDLES TRANSPORT COUNTERS AT THE MOMENT </summary>
        /// <param name="other"> </param>
        /// <returns> whether or not they are functionally similar </returns>
        public bool SameType(Card other)
        {
            // TODO: Add other counter types

            if (GetType() != other.GetType())
                return false;
            else if (GetType() == typeof(TravelCard))
            {
                TravelCard otr = (TravelCard)other;
                TravelCard cur = (TravelCard)this;
                return otr.GetTransportType() == cur.GetTransportType();
            }
            else
            {
                // unhandled type
                return false;
            }
        }

        // Returns wether Card can be used in given context.
        public abstract bool canUse(Player player, Road road);
    }
}*/