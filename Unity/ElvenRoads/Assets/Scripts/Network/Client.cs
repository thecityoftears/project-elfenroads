using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elfencore.Shared.GameState;

public static class Client
{
    public static string AccessToken = null;
    public static string RefreshToken = null;
    public static string WsUrl = null;
    public static string ServerURLLocation = null;
    public static SessionListGetter.ElfenroadsGameParems gameParems;

    public static string Username = null;

    public static string GameServiceName = null;
    public static ulong ConnectedSessionID;

    public static Player GetLocalPlayer() {
        foreach(Player p in Game.participants) {
            if(p.GetName() == Username)
                return p;
        }
        return null;
    }

}
