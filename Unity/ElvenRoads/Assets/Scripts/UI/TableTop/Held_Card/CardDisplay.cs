using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{

    public InGameUIController UIManager;
    public GameObject cardTemplate;
    private List<GameObject> generatedUI = new List<GameObject>();

    private float width;
    private float height;

    private float bwidth;
    private float bheight;

    public void Start() {
        RectTransform rt = cardTemplate.GetComponent<RectTransform>();
        width = rt.rect.width;
        height = rt.rect.height;

        RectTransform brt = cardTemplate.transform.GetChild(0).GetComponent<RectTransform>();
        bwidth = brt.rect.width;
        bheight = brt.rect.height;
    }

    /// <summary> Call this method when there is a change in local player cards in hand. Destroys all old cards and regenerates the hand </summary>
    public void UpdateUI()
    {
        List<Card> cards = Client.GetLocalPlayer().GetCards();
        DestroyUI(); // get rid of old Cards

        if(Client.GetLocalPlayer().NumOfCardsHeld() > 9) {
            cardTemplate.GetComponent<RectTransform>().sizeDelta = new Vector2(width * 0.75f, height * 0.75f);
            cardTemplate.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(bwidth * 0.75f, bheight * 0.75f);
        }
        else {
            cardTemplate.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
            cardTemplate.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(bwidth, bheight);
        }

        cardTemplate.SetActive(true);
        foreach (Card c in cards)
        {
            GameObject generatedCard = Instantiate(cardTemplate, cardTemplate.transform.parent);
            generatedCard.transform.name = c.type.ToString();
            generatedCard.GetComponent<Image>().sprite = UIResources.GetSpriteFor(c);
            CardPressedButton but = generatedCard.GetComponent<CardPressedButton>();
            but.card = c;
            but.UIManager = UIManager;

            generatedUI.Add(generatedCard);
        }
        cardTemplate.SetActive(false);
    }

    private void DestroyUI()
    {
        foreach (GameObject oldUI in new List<GameObject>(generatedUI))
        {
            Destroy(oldUI);
            generatedUI.Remove(oldUI);
        }
    }

    /// <summary> Used when a caravan has been selected and the Cards need to become clickable </summary>
    public void CardsCanBeClicked(bool clickable)
    {
        foreach (GameObject obj in generatedUI)
        {
            obj.GetComponent<Button>().enabled = clickable;
            obj.transform.GetChild(0).gameObject.SetActive(clickable);
        }
    }
}