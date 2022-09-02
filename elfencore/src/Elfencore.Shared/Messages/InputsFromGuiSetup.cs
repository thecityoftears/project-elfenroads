using System;
using Elfencore.Shared.GameState;
namespace Elfencore.Shared.Messages
{

    public class CreateGame
    {
        public string GameVersion;
    }
    public class LaunchSession
    {
        public string GameSessionID;
    }
    public class LoadGame
    {
        public string SaveGame;
    }
}

