using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using UnityEngine.UI;
using TMPro;

public class DrawCounterUI : MonoBehaviour
{
    private List<GameObject> generatedCounters = new List<GameObject>();

    public GameObject prototype;

    public  TextMeshProUGUI drawCardDisplay;

    public void UpdateUI()
    {
        if (Game.IsDrawCounterPhase()) {
            gameObject.SetActive(true);
            DestroyOldUI();

            prototype.SetActive(true);
            foreach (Counter c in Game.faceUpCounters)
            {
                GameObject generated = Instantiate(prototype, prototype.transform.parent);
                generated.GetComponent<FaceUpCounterButton>().faceUpCounter = c;
                generated.GetComponent<Image>().sprite = UIResources.GetSpriteFor(c);

                generatedCounters.Add(generated);
            }
            prototype.SetActive(false);

            //Update player name
            drawCardDisplay.text = Game.currentPlayer.GetName() + " is drawing a Counter!";
        }
        else {
            gameObject.SetActive(false);
        }
        
        UpdateColor();
    }

    private void UpdateColor() {
        UnityEngine.Color c;
        if(Game.IsCurrentPlayer(Client.GetLocalPlayer()))
            c = UnityEngine.Color.white;
        else
            c = UnityEngine.Color.gray;

        foreach(GameObject counter in generatedCounters) {
            counter.GetComponent<Image>().color = c;
        }
        gameObject.transform.GetChild(2).GetComponent<Image>().color = c;
    }

    private void DestroyOldUI()
    {
        foreach (GameObject obj in new List<GameObject>(generatedCounters))
        {
            Destroy(obj);
            generatedCounters.Remove(obj);
        }
    }
}