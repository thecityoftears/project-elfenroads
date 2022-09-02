using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

using UnityEngine.EventSystems;

public class Refresh : MonoBehaviour
{

    private static float nextActionTime = 0.0f;
    public static float period = 60f;

    async void Update()
    {
        if (Time.time > nextActionTime)
        {
            nextActionTime += period;
            LoginResponse response = await LobbyService.Refresh();

            if (response == null)
            {
                Debug.Log("Error refreshing token!");
            }
            else
            {
                Debug.Log("Refreshed token!");
                Client.AccessToken = response.AccessToken.Replace("+", "%2b"); // need to replace since + encodes blankspace in http messages
                Client.RefreshToken = response.RefreshToken;
            }
        }
    }
}