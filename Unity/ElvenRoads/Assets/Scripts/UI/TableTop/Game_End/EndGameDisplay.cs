using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using TMPro;

public class EndGameDisplay : MonoBehaviour
{
    public TextMeshProUGUI winnerText;
    public GameObject winningPlayerText;

    /// <summary> Call this after setting active to display the game winenr </summary>
    public void DisplayWinner(Player p)
    {
        if (p.GetName() == Client.GetLocalPlayer().GetName())
        {
            winningPlayerText.SetActive(true);
        }
        else
        {
            winningPlayerText.SetActive(false);
        }
        winnerText.text = "Congrats to game winner: " + p.GetName();
    }
}
