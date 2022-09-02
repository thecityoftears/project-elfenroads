using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugButton : MonoBehaviour
{
    public string username;
    public string password = "abc123_ABC123";

    public GameObject SessionListScreen;
    public GameObject LoginScreen;
    
    public async void Login() {
        LoginResponse response = await LobbyService.Login(username, password);
        Client.AccessToken = response.AccessToken.Replace("+", "%2b"); // need to replace since + encodes blankspace in http messages
        Client.RefreshToken = response.RefreshToken;
        Client.Username = username;

        LoginScreen.SetActive(false);
        SessionListScreen.SetActive(true);
        SessionListScreen.GetComponent<SessionListGetter>().RefreshSessionList();
    }
}
