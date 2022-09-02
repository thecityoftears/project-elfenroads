using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;

[RequireComponent(typeof(UnityEngine.UI.Button), typeof(UnityEngine.UI.Image))]
public class CardPressedButton : MonoBehaviour
{
    public Card card;
    public GameObject exit;

    public InGameUIController UIManager;

    public void InformManagerOfPressed()
    {
        UIManager.CardSelected(this);
    }
}
