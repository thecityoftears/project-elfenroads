using System.Collections.Generic;

namespace Elfencore.Shared.Messages.ServerToCore {
    // StartGame {players: list string}
    public class OpStartGame
    {
        public string op;
        public List<string> players;
    }
    // Connected {user: string}
    public class OpConnected {
        public string op;
        public string user;
    }
    // Payload {from: string, payload: string}
    public class OpPayload {
        public string op;
        public string from;
        public string payload;
    }
    // Disconnected {user: string}
    public class OpDisconnected {
        public string op;
        public string user;
    }
}