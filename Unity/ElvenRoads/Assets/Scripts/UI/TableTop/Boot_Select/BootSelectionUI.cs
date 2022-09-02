using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using gs = Elfencore.Shared.GameState;
using TMPro;

public class BootSelectionUI : MonoBehaviour
{
    public GameObject prototype;
    private List<KeyValuePair<string, gs.Color>> colors = new List<KeyValuePair<string, gs.Color>>() {
        new KeyValuePair<string, gs.Color>("Blue", gs.Color.BLUE), new KeyValuePair<string, gs.Color>("Cyan", gs.Color.CYAN), 
        new KeyValuePair<string, gs.Color>("Green", gs.Color.GREEN),
        new KeyValuePair<string, gs.Color>("Magenta", gs.Color.MAGENTA), new KeyValuePair<string, gs.Color>("Mint", gs.Color.MINT), new KeyValuePair<string, gs.Color>("Orange", gs.Color.ORANGE), 
        new KeyValuePair<string, gs.Color>("Red", gs.Color.RED), new KeyValuePair<string, gs.Color>("Violet", gs.Color.VIOLET), new KeyValuePair<string, gs.Color>("White", gs.Color.WHITE), 
        new KeyValuePair<string, gs.Color>("Yellow", gs.Color.YELLOW)
    };
    // Start is called before the first frame update
    public void SetupUI()
    {
        prototype.SetActive(true);
        foreach(KeyValuePair<string, gs.Color> pair in colors) {
            GameObject generated = Instantiate(prototype, prototype.transform.parent);
            gs.Color c = pair.Value;
            generated.GetComponent<Image>().color = new Color((float) c.r / 255, (float) c.g / 255, (float) c.b / 255);
            generated.GetComponent<ChooseBootButton>().color = c;
            generated.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = pair.Key;
        }
        prototype.SetActive(false);  
    }

    public void UpdateUI() {
        if(!Client.GetLocalPlayer().selectedBoot) 
            gameObject.SetActive(true);
        else 
            gameObject.SetActive(false);
    }
}
