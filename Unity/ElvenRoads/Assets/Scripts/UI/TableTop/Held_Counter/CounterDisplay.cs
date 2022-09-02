using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Elfencore.Shared.GameState;

public class CounterDisplay : MonoBehaviour
{
    public InGameUIController UIManager;
    public GameObject counterTemplate;
    private List<GameObject> generatedUI = new List<GameObject>();

    /// <summary> Call this method when there is a change in local player counters in posession. Destroys all old counters and regenerates the UI </summary>
    public void UpdateUI()
    {
        DestroyUI(); // get rid of old Counters

        CountersCanBeClicked(null, false); // set ui back to its default state

        List<Counter> counters = Client.GetLocalPlayer().GetCounters();

        counterTemplate.SetActive(true);
        foreach (Counter c in counters)
        {
            GameObject generatedCounter = Instantiate(counterTemplate, counterTemplate.transform.parent);
            generatedCounter.transform.name = c.type.ToString();
            generatedCounter.GetComponent<Image>().sprite = UIResources.GetSpriteFor(c);
            CounterPressedButton but = generatedCounter.GetComponent<CounterPressedButton>();
            but.counter = c;
            but.UIManager = UIManager;

            generatedUI.Add(generatedCounter);
        }
        counterTemplate.SetActive(false);
    }

    private void DestroyUI()
    {
        foreach (GameObject oldUI in new List<GameObject>(generatedUI))
        {
            Destroy(oldUI);
            generatedUI.Remove(oldUI);
        }
    }

    /// <summary> Used when a road has been clicked and the Counters need to become clickable </summary>
    public void CountersCanBeClicked(Road road, bool containsDoubleSpell)
    {
        Road r;
        if(road != null)
            r = Game.GetRoad(road.source.getName(), road.dest.getName(), road.region);
        else 
            r = null;

        bool clickable = r != null;
        foreach (GameObject obj in generatedUI)
        {
            obj.GetComponent<Button>().enabled = clickable;

            Counter c = new Counter(convert(obj.name));
            if( (r != null) && (
                (c.IsTrasportCounter() && c.CanTravel(r.region) && !r.ContainsTransportCounter()) ||
                (c.IsTrasportCounter() && c.CanTravel(r.region) && containsDoubleSpell) ||
                (c.type == Counter.CounterType.TREEOBS && !r.ContainsObstacle() && !r.ContainsGold() && c.ValidHere(r.region) && r.ContainsTransportCounter()) ||
                (c.type == Counter.CounterType.SEAOBS && !r.ContainsObstacle() && !r.ContainsGold() && c.ValidHere(r.region)) ||
                (c.IsDoubleSpell() && r.ContainsTransportCounter()) ||
                (c.IsExchangeSpell()) ||
                (c.type == Counter.CounterType.GOLD && !r.ContainsObstacle() && !r.ContainsGold() && r.ContainsTransportCounter())
            )) {
                obj.transform.GetChild(0).gameObject.SetActive(clickable);
            }
            else
                obj.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    private Counter.CounterType convert(string type) {
        switch(type) {
            case("DRAGON"):
                return Counter.CounterType.DRAGON;
            case("UNICORN"):
                return Counter.CounterType.UNICORN;
            case("TROLLWAGON"):
                return Counter.CounterType.TROLLWAGON;
            case("ELFCYCLE"):
                return Counter.CounterType.ELFCYCLE;
            case("MAGICCLOUD"):
                return Counter.CounterType.MAGICCLOUD;
            case("GIANTPIG"):
                return Counter.CounterType.GIANTPIG;
            case("RAFT"):
                return Counter.CounterType.RAFT;
            case("TREEOBS"):
                return Counter.CounterType.TREEOBS;
            case("SEAOBS"):
                return Counter.CounterType.SEAOBS;
            case("GOLD"):
                return Counter.CounterType.GOLD;
            case("DOUBLESPELL"):
                return Counter.CounterType.DOUBLESPELL;
            case("EXCHANGESPELL"):
                return Counter.CounterType.EXCHANGESPELL;
            default:
                return Counter.CounterType.RAFT;
        }
    }
}
