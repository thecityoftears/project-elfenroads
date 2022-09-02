using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using UnityEngine.UI;
using TMPro;

public class CardToTravelPopup : MonoBehaviour
{

    public GameObject optionPrototype;
    public GameObject witchPrototype;
    private List<GameObject> generatedOptions = new List<GameObject>();

    /// <summary> Tells the popup to create the options to travel on a selected road </summary>
    public void DisplayOptionsFor(Town source, Town dest)
    {
        // remove stale options
        foreach (GameObject obj in generatedOptions)
        {
            Destroy(obj);
        }
        generatedOptions.Clear();

        CaravanSelector addedCaravan = null;

        foreach (Road r in Game.GetRoadsBetween(source, dest))
        {
            bool canUseCaravan = false; // assuming we can only use caravans on roads with a counter
            int numObstacles = 0;
            List<TransportType> allowedTypes = new List<TransportType>();

            foreach (Counter c in r.GetCounters())
            {
                if (c.IsTrasportCounter())
                {
                    canUseCaravan = true;
                    allowedTypes.Add(c.GetTransportType());
                }
                else if (c.IsObstacle())
                    numObstacles++;
            }

            if (r.witchUsed)
            {
                numObstacles = 0;
            }

            if (r.region == Region.LAKE || r.region == Region.RIVER)
                allowedTypes.Add(TransportType.RAFT);

            if (numObstacles > 0 && !r.witchUsed && Client.GetLocalPlayer().GetCards().FindIndex(c => c.IsWitchCard()) != -1)
            {
                witchPrototype.SetActive(true);
                //Init Card Selector Object
                GameObject generated = Instantiate(witchPrototype, transform);
                generated.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Use Witch For Obstacle";
                generated.GetComponent<Image>().sprite = UIResources.GetSpriteFor(new Card(Card.CardType.WITCH));
                WitchSelector sel = generated.GetComponent<WitchSelector>();
                sel.targetRoad = r;
                generatedOptions.Add(generated);
            }
            witchPrototype.SetActive(false);

            optionPrototype.SetActive(true);
            // create option for each transport counter (spell can add 1 more)
            foreach (TransportType type in allowedTypes)
            {
                // get number of cards needed for this travel
                int cardsNeeded = 0;
                if (!Game.travelValues.TryGetValue(new KeyValuePair<TransportType, Region>(type, r.region), out cardsNeeded))
                    continue;

                cardsNeeded += numObstacles;
                if (r.region == Region.RIVER && type == TransportType.RAFT && Client.GetLocalPlayer().GetLocation().getName() != r.source.getName())
                    cardsNeeded++;

                GameObject generated = Instantiate(optionPrototype, transform);
                CardSelector cs = generated.AddComponent<CardSelector>();
                cs.road = r;
                generated.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = type.ToString();
                generated.GetComponent<Image>().sprite = UIResources.GetSpriteFor(new Card(type));

                generatedOptions.Add(generated);

                for (int i = 0; i < cardsNeeded; i++)
                {
                    cs.cardsUsedIfSelected.Add(new Card(type));
                }

                List<Card> playerCards = new List<Card>(Client.GetLocalPlayer().GetCards());
                bool canTravel = true;
                foreach (Card c in cs.cardsUsedIfSelected)
                {
                    if (!RemoveCard(playerCards, c))
                    {
                        canTravel = false;
                        break;
                    }
                }
                if (!canTravel)
                    generated.GetComponent<Image>().color = UnityEngine.Color.gray;
            }
            // create Caravan option if applicable
            if (canUseCaravan)
            {
                int cardsNeeded = 3 + numObstacles;
                bool dontAdd = false;
                if (addedCaravan != null)
                {
                    if (cardsNeeded < addedCaravan.cardsNeeded)
                    {
                        Destroy(addedCaravan.gameObject);
                        generatedOptions.Remove(addedCaravan.gameObject);
                    }
                    else
                        dontAdd = true;
                }
                if (!dontAdd)
                {
                    GameObject generated = Instantiate(optionPrototype, transform);
                    generated.GetComponent<Image>().sprite = UIResources.GetCaravanSprite();
                    CaravanSelector cs = generated.AddComponent<CaravanSelector>();
                    cs.cardsNeeded = cardsNeeded;
                    cs.targetRoad = r;
                    generated.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "CARAVAN";

                    addedCaravan = cs;

                    generatedOptions.Add(generated);
                }
            }

            optionPrototype.SetActive(false);

        }
    }

    private bool RemoveCard(List<Card> cards, Card card)
    {
        Card toRemove = null;
        foreach (Card c in cards)
        {
            if (c.type == card.type)
            {
                toRemove = c;
                break;
            }
        }
        if (toRemove == null)
            return false;
        else
        {
            cards.Remove(toRemove);
            return true;
        }
    }
}