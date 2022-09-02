using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameButton : MonoBehaviour
{
    bool canStart = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    async void Update()
    {
        Dictionary<string, GameSession> sessions = await LobbyService.GetSessions();
        GameSession session;
        if(!sessions.TryGetValue(Client.ConnectedSessionID.ToString(), out session)) {
            canStart = true;
            UpdateButtonColor();
            return;
        }
        
        if(session.Savegameid != null && session.Savegameid != "") {
            SaveGame save = await LobbyService.GetSaveGame(session.GameParameters.DisplayName, session.Savegameid);
            List<string> neededPlayers = save.Players;
            foreach(string connected in session.Players) {
                neededPlayers.Remove(connected);
            }
            if(neededPlayers.Count == 0) {
                canStart = true;
                UpdateButtonColor();
                return;
            }
            else {
                canStart = false;
                UpdateButtonColor();
                return;
            }
        }
        else {
            canStart = true;
            UpdateButtonColor();
        }
    }

    private void UpdateButtonColor() {
        if(canStart)
            gameObject.GetComponent<UnityEngine.UI.Button>().image.color = Color.white;
        else
            gameObject.GetComponent<UnityEngine.UI.Button>().image.color = Color.grey;
    }

    public async void StartGame() {
        if(canStart) {
            await LobbyService.StartGame();
        }
    }
}
