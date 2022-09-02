using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;
using Newtonsoft.Json;
using Elfencore.Shared.GameState;
using Elfencore.Shared.Messages.ServerToClient;
using Elfencore.Shared.Messages;

public class MessageHandler : MonoBehaviour
{
    public InGameUIController UIManager;

    [HideInInspector]
    public static WebSocket websocket;

    // Start is called before the first frame update
    void Start()
    {

        CreateWebsocket();
    }

    private bool IsLocalPlayer()
    {
        return Game.currentPlayer.GetName() == Client.Username;
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket == null)
        {
            CreateWebsocket();
        }
        else
        {
            websocket.DispatchMessageQueue();
        }
#endif
    }

    private async void CreateWebsocket()
    {

        // Testing. Trying to connect to server to make requests.
        // Current output from OnError: "Error! Unable to connect to the remote server", the connection closes.
        Client.WsUrl = "ws://127.0.0.1:8081";
        //Client.AccessToken = System.Net.WebUtility.UrlEncode("46g72CTKHZeyJexxmbo/DF3xuFs="); //received from login. tried with and without escaping
        // Client.ConnectedSessionID = 3;
        //Client.GameServiceName = "bGFuZDozOmZhbHN";

        websocket = new WebSocket(String.Format("{0}/{1}/{2}?access_token={3}", Client.WsUrl, Client.GameServiceName, Client.ConnectedSessionID.ToString(), Client.AccessToken));

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            UIManager.DisplayErrorMessage("DISCONNECTED");
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            Debug.Log("OnMessage!");

            Elfencore.Shared.Messages.Message obj = JsonConvert.DeserializeObject<Elfencore.Shared.Messages.Message>(System.Text.Encoding.UTF8.GetString(bytes));
            //long pID = obj.pID; // can lookup by pID
            string messageName = obj.Tag;

            // TODO: Add functionality when receiving messages. Should be same here as on server.
            switch (messageName)
            {
                case "StartGame":
                    { // this will give the gamestate but also say to start the game
                        JsonSerializerSettings   jsSettings =  new JsonSerializerSettings
                        {
                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                        };

                        var value = JsonConvert.DeserializeObject<GameStateMsg>(obj.Content, jsSettings);
                        GameStateMsg.ReadMsg(value);

                        Debug.Log("Start Game message recieved: " + obj.Content);

                        UIManager.SetupGameUI();
                        break;
                    }
                case "BroadcastGameState":
                    {
                        JsonSerializerSettings   jsSettings =  new JsonSerializerSettings
                        {
                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                        };

                        var value = JsonConvert.DeserializeObject<GameStateMsg>(obj.Content, jsSettings);
                        GameStateMsg.ReadMsg(value);

                        Debug.Log("Broadcast Game message recieved: " + obj.Content);

                        UIManager.UpdateUI();
                        break;
                    }
                case "SendErrorMessage":
                    {
                        var value = obj.Content;
                        UIManager.DisplayErrorMessage(value);

                        Debug.Log("Error message recieved: " + obj.Content);

                        break;
                    }
                // DEFAULT !
                default:
                    {
                        Debug.Log("Invalid Message Type!");
                        break;
                    }
            }
        };

        // waiting for messages
        await websocket.Connect();
    }

    async void OnDestroy()
    {
        await websocket.Close();
    }

    #region SendWebSocketMessage
    public static async void Message(object o)
    {
        if (websocket.State == WebSocketState.Open)
        {
            // Sending json
            await websocket.SendText(JsonConvert.SerializeObject(new Message(o.GetType().ToString(), JsonConvert.SerializeObject(o))));
        }
    }
    #endregion

    #region CloseWebsocket
    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
    #endregion
}
