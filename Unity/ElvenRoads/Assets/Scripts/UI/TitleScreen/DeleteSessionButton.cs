using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteSessionButton : MonoBehaviour
{
    [HideInInspector]
    public ulong sessionID;

    public SessionListGetter sessionListGetter;

    public async void DeleteSession() {
        if(await LobbyService.DeleteSession(sessionID)) {
            sessionListGetter.RefreshSessionList();
        }
    }
}
