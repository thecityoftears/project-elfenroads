using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using UnityEngine.UI;

public class OpponentUI : MonoBehaviour
{
    public GameObject prototype;

    private List<OpponentUIUpdater> opponents = new List<OpponentUIUpdater>();

    /// <summary> Called to make a UI per other player </summary>
    public void SetupUI()
    {
        prototype.SetActive(true);
        foreach (Player p in Game.participants)
        {
            if (p.GetName() == Client.Username) // dont add yourself
                continue;

            GameObject generated = Instantiate(prototype, prototype.transform.parent);
            generated.transform.name = "Opponent: " + p.GetName();
            OpponentUIUpdater opp =  generated.GetComponent<OpponentUIUpdater>();
            opp.username = p.GetName();
            opponents.Add(opp);
        }

        prototype.SetActive(false);
    }

    public void UpdateUI()
    {
        foreach (OpponentUIUpdater opp in opponents)
        {
            Player p = Game.GetPlayerFromName(opp.username);
            if (p.GetColor() != Elfencore.Shared.GameState.Color.WHITE)
            {
                Elfencore.Shared.GameState.Color c = p.GetColor();
                opp.playerImage.color = new UnityEngine.Color((float)c.r / 255, (float)c.g / 255, (float)c.b / 255);
            }

            opp.UpdateValues();
        }
    }
}
