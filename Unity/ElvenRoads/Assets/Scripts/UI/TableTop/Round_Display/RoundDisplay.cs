using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Elfencore.Shared.GameState;

public class RoundDisplay : MonoBehaviour
{
    public TextMeshProUGUI num;
    public TextMeshProUGUI phase;

    public void UpdateUI() {
        num.text = Game.curRound.ToString();
        phase.text = "Phase: " + Game.phase.ToString();
    }
}
