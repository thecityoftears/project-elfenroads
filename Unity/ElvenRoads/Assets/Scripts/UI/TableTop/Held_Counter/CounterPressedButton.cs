using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;

public class CounterPressedButton : MonoBehaviour
{
    public Counter counter;

    public InGameUIController UIManager;

    public void InformManagerOfPressed() {
        UIManager.HeldCounterSelected(counter);
    }
}
