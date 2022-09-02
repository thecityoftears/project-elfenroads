using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Elfencore.Shared.GameState;

public class PlayerPanelScript : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI citiesVisitedText;    
    public TextMeshProUGUI cardsText;
    public TextMeshProUGUI countersText;
    public TextMeshProUGUI playerNameText;
    public RawImage bootImage;
    public GameObject border;
    public Image destination;
    
    public void UpdateUI() {
        Player localPlayer = Client.GetLocalPlayer();
        if(localPlayer == null)
            return;

        if(localPlayer.GetColor() == null) 
            bootImage.enabled = false;
        else {
            bootImage.enabled = true;
            Elfencore.Shared.GameState.Color c = localPlayer.GetColor();
            bootImage.color = new UnityEngine.Color((float) c.r / 255, (float) c.g / 255, (float) c.b / 255);
        }

        playerNameText.text = "Name: " + Client.Username;
        if(Game.variant == Variant.ELFENLAND)
            goldText.gameObject.transform.parent.gameObject.SetActive(false);
        else {
            goldText.gameObject.transform.parent.gameObject.SetActive(true);
            goldText.text = "x " + localPlayer.amountOfGold();
        }

        citiesVisitedText.text = "x " + localPlayer.NumOfTownVisited();

        if(Game.IsCurrentPlayer(Client.GetLocalPlayer()))
            border.SetActive(true);
        else
            border.SetActive(false);

        if(Game.randomDest && localPlayer.destination != null) {
            destination.sprite = UIResources.GetSpriteForDest(localPlayer.destination.getName());
            destination.gameObject.SetActive(true);
        }
        else
            destination.gameObject.SetActive(false);
    }
}
