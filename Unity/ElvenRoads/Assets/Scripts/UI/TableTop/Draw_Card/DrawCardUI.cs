using System.Collections;
using System.Collections.Generic;
using Elfencore.Shared.GameState;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DrawCardUI : MonoBehaviour
{
    private List<GameObject> generatedCards = new List<GameObject>();
    public GameObject cardPrototype;
    public TextMeshProUGUI goldNumDisplay;

    public TextMeshProUGUI drawCardDisplay;

    public void UpdateUI()
    {
        if (Game.IsDrawCardPhase() && Game.curRound != 1) {
            gameObject.SetActive(true);
            DestroyOldUI();

            cardPrototype.SetActive(true);
            //Update Face Up Cards
            foreach (Card c in Game.faceUpCards)
            {
                GameObject cardGenerated = Instantiate(cardPrototype, cardPrototype.transform.parent);
                cardGenerated.GetComponent<FaceUpCardButton>().faceUpCard = c;
                cardGenerated.GetComponent<Image>().sprite = UIResources.GetSpriteFor(c);

                generatedCards.Add(cardGenerated);
            }

            //Update gold nums
            goldNumDisplay.text = "x " + Game.goldDeck.Count.ToString();

            //Update player name
            drawCardDisplay.text = Game.currentPlayer.GetName() + " is drawing a Travel Card!";

            cardPrototype.SetActive(false);

            UpdateColor();
        }
        else {
            gameObject.SetActive(false);
        }
    }

    private void UpdateColor() {
        UnityEngine.Color c;
        if(Game.IsCurrentPlayer(Client.GetLocalPlayer()))
            c = UnityEngine.Color.white;
        else
            c = UnityEngine.Color.gray;

        foreach(GameObject card in generatedCards) {
            card.GetComponent<Image>().color = c;
        }
        gameObject.transform.GetChild(2).GetComponent<Image>().color = c;
        gameObject.transform.GetChild(3).GetComponent<Image>().color = c;
    }

    private void DestroyOldUI()
    {
        //Destroy Face Up Cards
        foreach (GameObject obj in new List<GameObject>(generatedCards))
        {
            Destroy(obj);
            generatedCards.Remove(obj);
        }
    }
}