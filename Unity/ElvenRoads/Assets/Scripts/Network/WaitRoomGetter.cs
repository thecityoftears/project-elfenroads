using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Elfencore.Shared.GameState;

public class WaitRoomGetter : MonoBehaviour
{
    public GameObject playerTemplate;

    public GameObject aloneOverlay;

    public GameObject startGameButton;

    public List<GameObject> createdPlayers = new List<GameObject>();

    public float timeBetweenRefresh = 1.0f;

    private float timeSinceLastRefresh;

    void Start() {
        
        RefreshWaitRoom();
        timeSinceLastRefresh = Time.time;
    }

    // Update is called once per frame
    void Update() {
        if(Time.time >= timeBetweenRefresh + timeSinceLastRefresh) {
            RefreshWaitRoom();
            timeSinceLastRefresh = Time.time;
        }
    }


    public async void RefreshWaitRoom() {
        // clear old playercards
        foreach(GameObject oldPlayer in createdPlayers) {
            Destroy(oldPlayer);
        }

        // get session info
        Dictionary<string, GameSession> sessionList = await LobbyService.GetSessions();

        
        if(sessionList.ContainsKey(Client.ConnectedSessionID.ToString())) {
            GameSession currentSession = sessionList[Client.ConnectedSessionID.ToString()];

            // need to start game
            if(currentSession.Launched) {
                Client.ServerURLLocation = currentSession.GameParameters.Location;
                Debug.Log("Starting game... Connected to: " + Client.ServerURLLocation);
                Client.gameParems = SessionListGetter.GetVariantDetails(currentSession.GameParameters.Name);

                foreach(string p in currentSession.Players) {
                    Game.participants.Add(new Player(p));
                }

                SceneManager.LoadScene("TableTop");
            }
            else {
                List<string> connectedPlayers = currentSession.Players;

                // we only add player cards if theres more than just the host
                if(connectedPlayers.Count > 1) {
                    // for host start button
                    if(currentSession.Creator == Client.Username 
                    && connectedPlayers.Count >= currentSession.GameParameters.MinSessionPlayers 
                    && connectedPlayers.Count <= currentSession.GameParameters.MaxSessionPlayers) {
                        startGameButton.SetActive(true);
                    }
                    else 
                        startGameButton.SetActive(false);


                    playerTemplate.SetActive(true);

                    aloneOverlay.SetActive(false);

                    foreach(string player in connectedPlayers) {
                        if(player != Client.Username) {
                            GameObject newPlayer = GameObject.Instantiate(playerTemplate, playerTemplate.transform.parent);

                            newPlayer.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Name: " + player; 

                            createdPlayers.Add(newPlayer);
                        }
                    }

                    playerTemplate.SetActive(false);

                }
                else { // inform that host is alone
                    aloneOverlay.SetActive(true);
                }
            }
        }
        else {
            Debug.Log("ERROR: LS no longer contains connected session");
        }
    }
}
