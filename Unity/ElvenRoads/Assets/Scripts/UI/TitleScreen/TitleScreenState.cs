using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenState : MonoBehaviour
{
    public GameObject LoginScreen;
    public GameObject SessionListScreen;

    // Start is called before the first frame update
    void Start()
    {
        if(Client.AccessToken != null) {
            LoginScreen.SetActive(false);
            SessionListScreen.SetActive(true);
        }

        AudioManager.PlaySound("TitleScreenMusic");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
