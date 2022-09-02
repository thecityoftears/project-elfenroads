using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;
using Elfencore.Shared.Messages.ClientToServer;

public class FaceUpCounterButton : MonoBehaviour
{
    public Counter faceUpCounter;
    
    public void SelectFaceUp() {
        DrawCounter msg = new DrawCounter();
        msg.Counter = faceUpCounter;
        
        MessageHandler.Message(msg);
    }
}
