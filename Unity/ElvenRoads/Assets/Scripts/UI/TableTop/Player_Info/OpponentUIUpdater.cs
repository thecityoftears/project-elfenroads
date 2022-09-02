using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using TMPro;
using UnityEngine.UI;

public class OpponentUIUpdater : MonoBehaviour
{
    public string username;
    public GameObject counterPrototype;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI citiesVisitedText;
    public TextMeshProUGUI cardsText;
    public TextMeshProUGUI playerNameText;
    public Image playerImage;
    public GameObject border;

    private List<GameObject> generatedCounters = new List<GameObject>();

    public void UpdateValues()
    {
        Player player = Game.GetPlayerFromName(username);

        if (Game.variant == Variant.ELFENLAND)
            goldText.gameObject.transform.parent.gameObject.SetActive(false);
        else
        {
            goldText.gameObject.transform.parent.gameObject.SetActive(true);
            goldText.text = "x " + player.amountOfGold();
        }

        playerNameText.text = "Name: " + player.GetName();
        citiesVisitedText.text = "x " + player.NumOfTownVisited();
        cardsText.text = "x " + player.NumOfCardsHeld();

        DestroyCreatedCounters();
        counterPrototype.SetActive(true);
        foreach (Counter c in player.GetCounters())
        {
            GameObject generated = Instantiate(counterPrototype, counterPrototype.transform.parent);
            if (c.IsVisible())
                generated.GetComponent<Image>().sprite = UIResources.GetSpriteFor(c);
            else
                generated.GetComponent<Image>().sprite = UIResources.GetSpriteForFaceDownCounter();

            generatedCounters.Add(generated);
        }
        counterPrototype.SetActive(false);

        if(Game.IsCurrentPlayer(player))
            border.SetActive(true);
        else
            border.SetActive(false);
    }

    public void DestroyCreatedCounters()
    {
        foreach (GameObject obj in new List<GameObject>(generatedCounters))
        {
            Destroy(obj);
            generatedCounters.Remove(obj);
        }
    }
}
