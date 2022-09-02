using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;

public abstract class Selector : MonoBehaviour
{
    public int cardsNeeded;
    public Road targetRoad;
    protected InGameUIController m_UIController;

    protected virtual void Start()
    {
        m_UIController = GameObject.FindGameObjectWithTag("UIManager").GetComponent<InGameUIController>();
    }
}