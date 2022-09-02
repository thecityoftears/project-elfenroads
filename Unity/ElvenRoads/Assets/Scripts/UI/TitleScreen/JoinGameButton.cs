using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinGameButton : MonoBehaviour
{
    [HideInInspector]
    public ulong joinedGameId;

    public GameObject SessionListScreen;

    public GameObject WaitRoomScreen;

    public async void JoinGame()
    {
        Dictionary<string, GameSession> sessions =  await LobbyService.GetSessions();
        if (sessions.ContainsKey(joinedGameId.ToString()) && sessions[joinedGameId.ToString()].Creator == Client.Username)
        {
            Client.ConnectedSessionID = joinedGameId;

            WaitRoomScreen.SetActive(true);
            SessionListScreen.SetActive(false);
        }
        else if (await LobbyService.JoinGame(joinedGameId))
        {
            Client.ConnectedSessionID = joinedGameId;

            SessionListScreen.SetActive(false);
            WaitRoomScreen.SetActive(true);
        }
    }
}
