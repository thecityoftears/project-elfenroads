using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using UnityEngine.UI;
using TMPro;

public class HideCounterUI : MonoBehaviour
{
    private List<GameObject> generatedCounters = new List<GameObject>();

    public GameObject prototype;

    public TextMeshProUGUI topTextDisplay;

    public void UpdateUI()
    {
        if (Game.phase == GamePhase.ChooseCounterPhase)
        {
            gameObject.SetActive(true);
            DestroyOldUI();

            prototype.SetActive(true);
            foreach (Counter c in Client.GetLocalPlayer().schrodingerCounters)
            {
                GameObject generated = Instantiate(prototype, prototype.transform.parent);
                generated.GetComponent<HideCounterButton>().faceUpCounter = c;
                generated.GetComponent<Image>().sprite = UIResources.GetSpriteFor(c);

                generatedCounters.Add(generated);
            }
            prototype.SetActive(false);

            //Update text
            topTextDisplay.text = "Choose which Counter to make Hidden!";
        }
        else
        {
            gameObject.SetActive(false);
        }

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