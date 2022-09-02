using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using Elfencore.Shared.Messages.ClientToServer;
using TMPro;
using UnityEngine.UI;

public class EndRoundCounterDisplay : MonoBehaviour
{
    public GameObject prototype;
    public GameObject egPrototype;

    private List<GameObject> generatedCounters = new List<GameObject>();

    public TextMeshProUGUI titleDisplay;

    public List<Counter> toKeep = new List<Counter>();

    public void UpdateUI()
    {

        if (Game.phase == GamePhase.EndOfRound && Game.finishedPhase.FindIndex(p => p.GetName() == Client.GetLocalPlayer().GetName()) == -1)
        {
            DestroyCounters();
            if (Game.variant == Variant.ELFENLAND)
            {
                gameObject.SetActive(true);

                prototype.SetActive(true);
                foreach (Counter c in new List<Counter>(Client.GetLocalPlayer().GetCounters()))
                {
                    GameObject generated = Instantiate(prototype, prototype.transform.parent);
                    generated.GetComponent<ChooseCounterButton>().counterToKeep = c;
                    generated.GetComponent<Image>().sprite = UIResources.GetSpriteFor(c);
                    generated.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Hidden: " + !c.IsVisible();

                    generatedCounters.Add(generated);
                }
                prototype.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);

                egPrototype.SetActive(true);
                foreach (Counter c in new List<Counter>(Client.GetLocalPlayer().GetCounters()))
                {
                    GameObject generated = Instantiate(egPrototype, egPrototype.transform.parent);
                    generated.GetComponent<ChooseCounterButton>().counterToKeep = c;
                    generated.GetComponent<Image>().sprite = UIResources.GetSpriteFor(c);
                    generated.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Hidden: " + !c.IsVisible();
                    generated.GetComponent<ChooseCounterButton>().ui = this;

                    generatedCounters.Add(generated);
                }
                egPrototype.SetActive(false);
            }

            //Update text
            titleDisplay.text = Client.GetLocalPlayer().GetName() + " : Choose which Counters to keep";
        }
        else
        {
            gameObject.SetActive(false);
        }

    }

    public void addCounter(Counter c)
    {
        if (!toKeep.Contains(c))
        {
            toKeep.Add(c);
        }

        if (toKeep.Count == 2)
        {
            CountersToKeep msg = new CountersToKeep();
            msg.Counters = toKeep;
            MessageHandler.Message(msg);
            toKeep.Clear();
        }
    }

    private void DestroyCounters()
    {
        foreach (GameObject obj in new List<GameObject>(generatedCounters))
        {
            Destroy(obj);
            generatedCounters.Remove(obj);
        }
    }
}
