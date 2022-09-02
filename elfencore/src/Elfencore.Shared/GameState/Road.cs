using System.Collections.Generic;
using System.Linq;

namespace Elfencore.Shared.GameState
{
    /// <summary> Defines a route between two Towns. Source and Destination towns must be set correctly in the case that the Region = RIVER. Otherwise, order should not matter </summary>
    public class Road
    {
        public Town source;
        public Town dest;
        public Region region;
        public List<Counter> counters = new List<Counter>();
        public bool witchUsed = false;

        public Road(Town src, Town destination, Region roadRegion)
        {
            source = src;
            dest = destination;
            region = roadRegion;
        }

        public void AddCounter(Counter c)
        { counters.Add(c); }

        public List<Counter> GetCounters()
        { return counters; }

        public void DoubleCounter()
        { counters.Add(counters[0]); }

        public void RemoveCounter(Counter c)
        {
            counters.Remove(counters.Where(item => item.type == c.type).First());
        }

        public void RemoveCounters()
        { counters.Clear(); }

        public bool ContainsTransportCounter()
        {
            foreach (Counter c in counters)
            {
                if (c.IsTrasportCounter())
                    return true;
            }
            return false;
        }

        public bool ValidTransportType(TransportType type)
        {
            List<TransportType> transportTypes = new List<TransportType>();
            foreach (Counter c in counters)
            {
                if (c.IsTrasportCounter())
                {
                    transportTypes.Add(c.GetTransportType());
                }
            }

            return transportTypes.Contains(type);
        }

        public bool ContainsObstacle()
        {
            foreach (Counter c in counters)
            {
                if (c.IsObstacle())
                    return true;
            }
            return false;
        }

        public bool ContainsGold()
        {
            foreach (Counter c in counters)
            {
                if (c.type == Counter.CounterType.GOLD)
                    return true;
            }
            return false;
        }

        public bool ContainsDoubleSpell()
        {
            return counters.Any(c => c.IsDoubleSpell());
        }

        public bool ContainsExchangeSpell()
        {
            return counters.Any(c => c.IsExchangeSpell());
        }
    }
};