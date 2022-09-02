using System;
using CommandLine;

namespace Elfencore.Console {
    public record LaunchArguments {
        [Option(Required = true, HelpText = "Set port to communicate with networking server.")]
        public UInt16 Port { get; set; }
        [Option(Required = true, HelpText = "Session ID")]
        public UInt64 SessionId { get; set; }
        [Option(Required = true, HelpText = "Game variant")]
        public string GameVariant { get; set; }
        [Option(Required = true, HelpText = "Secret to connect to internal WebSocket")]
        public string Secret { get; set; }
        [Option(Required = true, HelpText = "Number Of Rounds to play. Default = 3")]
        public UInt16 NumRounds { get; set; }
        [Option(Required = true, HelpText = "Are we playing with a destination town? Default = false")]
        public string Destination { get; set; }
        [Option(Required = false, HelpText = "Are we playing with a randomly distributed gold? Default = false")]
        public string RandomGold { get; set; }
        [Option(Required = false, HelpText = "Are we playing with the witch? Default = false")]
        public string Witch { get; set; }
        [Option(Required = false, HelpText = "Optional backing file for launched session.")]
        public string? FilePath { get; set; }
        [Option(Required = false, HelpText = "Is this a new gamesession or not")]
        public bool IsNewSession { get; set; }
    }
}
