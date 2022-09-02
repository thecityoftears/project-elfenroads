using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaveGameButton : MonoBehaviour
{

    public GameObject SessionListScreen;

    public GameObject WaitRoomScreen;

    public async void LeaveGame() {

        Dictionary<string, GameSession> sessions =await LobbyService.GetSessions();
        if(sessions.ContainsKey(Client.ConnectedSessionID.ToString()) && sessions[Client.ConnectedSessionID.ToString()].Creator == Client.Username) {
            WaitRoomScreen.SetActive(false);
            SessionListScreen.SetActive(true);
        }
        else if(await LobbyService.LeaveGame()) {
            WaitRoomScreen.SetActive(false);
            SessionListScreen.SetActive(true);
        }
    }
}
