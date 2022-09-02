using System.Collections.Generic;

namespace Elfencore.Shared.GameState
{
    public class Counter
    {
        public enum CounterType
        {
            DRAGON,
            UNICORN,
            TROLLWAGON,
            ELFCYCLE,
            MAGICCLOUD,
            GIANTPIG,
            RAFT,
            TREEOBS,
            SEAOBS,
            GOLD,
            DOUBLESPELL,
            EXCHANGESPELL
        }

        public CounterType type;
        public bool visible;

        public Counter()
        {
        }

        public Counter(CounterType t)
        {
            type = t;
            visible = true;
        }

        public Counter(int typeNum)
        {
            type = (CounterType)typeNum;
        }

        public Counter(TransportType t)
        {
            visible = true;
            switch (t)
            {
                case (TransportType.DRAGON):
                    type = CounterType.DRAGON;
                    break;

                case (TransportType.UNICORN):
                    type = CounterType.UNICORN;
                    break;

                case (TransportType.TROLLWAGON):
                    type = CounterType.TROLLWAGON;
                    break;

                case (TransportType.ELFCYCLE):
                    type = CounterType.ELFCYCLE;
                    break;

                case (TransportType.MAGICCLOUD):
                    type = CounterType.MAGICCLOUD;
                    break;

                case (TransportType.GIANTPIG):
                    type = CounterType.GIANTPIG;
                    break;

                case (TransportType.RAFT):
                    type = CounterType.RAFT;
                    break;
            }
        }

        public void SetVisible(bool visibility)
        { visible = visibility; }

        public bool IsVisible()
        { return visible; }

        public bool SameType(Counter other)
        {
            return type == other.type;
        }

        public bool IsTrasportCounter()
        {
            return type == CounterType.DRAGON || type == CounterType.UNICORN || type == CounterType.TROLLWAGON || type == CounterType.ELFCYCLE || type == CounterType.MAGICCLOUD || type == CounterType.GIANTPIG || type == CounterType.RAFT;
        }

        public bool IsObstacle()
        {
            return type == CounterType.TREEOBS || type == CounterType.SEAOBS;
        }

        public bool IsExchangeSpell()
        {
            return type == CounterType.EXCHANGESPELL;
        }

        public bool IsDoubleSpell()
        {
            return type == CounterType.DOUBLESPELL;
        }

        public bool IsGold()
        {
            return type == CounterType.GOLD;
        }

        public TransportType GetTransportType()
        {
            switch (type)
            {
                case (CounterType.DRAGON):
                    return TransportType.DRAGON;

                case (CounterType.UNICORN):
                    return TransportType.UNICORN;

                case (CounterType.TROLLWAGON):
                    return TransportType.TROLLWAGON;

                case (CounterType.ELFCYCLE):
                    return TransportType.ELFCYCLE;

                case (CounterType.MAGICCLOUD):
                    return TransportType.MAGICCLOUD;

                case (CounterType.GIANTPIG):
                    return TransportType.GIANTPIG;

                case (CounterType.RAFT):
                    return TransportType.RAFT;
            }
            return TransportType.RAFT; // JUST FOR TEMP
        }

        public bool CanTravel(Region r)
        {
            return Game.travelValues.ContainsKey(new KeyValuePair<TransportType, Region>(GetTransportType(), r));
        }

        public bool ValidHere(Region r)
        {
            return (type == CounterType.SEAOBS && (r == Region.LAKE || r == Region.RIVER)) || (type == CounterType.TREEOBS && (r != Region.LAKE && r != Region.RIVER));
        }


        public static bool CanUseExchangeSpell(Counter c1, Road r1, Counter c2, Road r2)
        {
            return c1.CanTravel(r2.region) && c2.CanTravel(r1.region);
        }
    }
};