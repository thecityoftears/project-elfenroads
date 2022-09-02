using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.Messages.ClientToServer;

public class SaveGameButton : MonoBehaviour
{
    public void RequestSaveGame() {
        MessageHandler.Message(new RequestSave());
    }
}
