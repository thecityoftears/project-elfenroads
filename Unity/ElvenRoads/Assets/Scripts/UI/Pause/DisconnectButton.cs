using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DisconnectButton : MonoBehaviour
{
    public async void DisconnectFromSession() {
        await LobbyService.LeaveGame();
        GameSession curSession;
        Dictionary<string, GameSession> sessions = await LobbyService.GetSessions();
        sessions.TryGetValue(Client.ConnectedSessionID.ToString(), out curSession);

        if(curSession == null) {
            Debug.Log("ERROR: Session is gone");
        }
        else if(curSession.Creator == Client.Username) {
            await LobbyService.DeleteSession(Client.ConnectedSessionID);
        }

        SceneManager.LoadScene("Title Screen");
    }
}
