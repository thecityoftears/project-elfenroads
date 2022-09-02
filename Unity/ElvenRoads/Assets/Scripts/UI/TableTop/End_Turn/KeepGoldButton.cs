using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using Elfencore.Shared.Messages.ClientToServer;
using TMPro;

public class KeepGoldButton : MonoBehaviour
{
    public bool keep;
    public TextMeshProUGUI textField;

    public void UpdateUI()
    {
        textField.text = "Keep Gold: " + Client.GetLocalPlayer().goldThisTurn;
    }

    public void SelectChooseGold()
    {
        ChooseToGetGoldForTravel msg = new ChooseToGetGoldForTravel();
        msg.gold = keep;

        MessageHandler.Message(msg);
    }
}
