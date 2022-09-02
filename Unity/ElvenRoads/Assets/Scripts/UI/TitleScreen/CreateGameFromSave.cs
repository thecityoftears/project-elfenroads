using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGameFromSave : MonoBehaviour
{
    [HideInInspector]
    public string saveGameID;
    [HideInInspector]
    public string gameServiceName;

    public GameObject SessionListScreen;

    public GameObject WaitRoomScreen;

    public async void CreateGame()
    {
        ulong? sessionID = await LobbyService.CreateGame(gameServiceName, saveGameID);
        if (sessionID != null)
        {
            // load into game lobby 

            Debug.Log("Create Game Success");

            Client.GameServiceName = gameServiceName;
            Client.ConnectedSessionID = (ulong)sessionID;

            WaitRoomScreen.SetActive(true);
            SessionListScreen.SetActive(false);
        }
        else
            Debug.Log("Create Game Failed");
    }
}
