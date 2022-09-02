using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IconEnlarger : MonoBehaviour
{
    public RectTransform imgRect;
    private Vector2 m_originSizeDelta;

    private void Start()
    {
        m_originSizeDelta = imgRect.sizeDelta;
    }

    public void OnEnlargeEnter()
    {
        imgRect.sizeDelta *= 1.1f;
    }

    public void OnEnlargeExit()
    {
        imgRect.sizeDelta = m_originSizeDelta;
    }
}