using System.Collections.Generic;

namespace Elfencore.Shared.Messages.CoreToServer {
    // Single {target: string, payload: string}
    public class OpSingle {
        public OpSingle(string target, string payload) {
            this.op = "Single";
            this.target = target;
            this.payload = payload;
        }
        public string op;
        public string target;
        public string payload;
    }
    // Broadcast {payload: string}
    public class OpBroadcast {
        public OpBroadcast(string payload) {
            this.op = "Broadcast";
            this.payload = payload;
        }
        public string op;
        public string payload;
    }
    // Terminate
    public class OpTerminate {
        public OpTerminate() {
            this.op = "Terminate";
        }
        public string op;
    }
    // Save
    public class OpSave
    {
        public string op;
        public IEnumerable<string> players;
        public OpSave(IEnumerable<string> players)
        {
            this.op = "Save";
            this.players = players; 
        }
    }
}