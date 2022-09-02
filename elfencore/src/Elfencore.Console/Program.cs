using CommandLine;
using Elfencore.Console;
using Elfencore.Network;
using Elfencore.Session;
using Elfencore.Shared.GameState;

var terminate = new CancellationTokenSource();
Console.CancelKeyPress += delegate {
    terminate.Cancel();
};
await Parser.Default.ParseArguments<LaunchArguments>(args).WithParsedAsync(async opts => {
    Variant variant;
    Console.WriteLine("PORT IS: " + opts.Port);

    if (opts.GameVariant == "land") {
        variant = Variant.ELFENLAND;
    } else if (opts.GameVariant == "gold") {
        variant = Variant.ELFENGOLD;
    } else {
        return;
    }
    Console.WriteLine(opts);

    bool hasDest = false;
    if (opts.Destination == "true") {
        hasDest = true;
    }

    bool hasRandomGold = false;
    if (opts.RandomGold == "true") {
        hasRandomGold = true;
    }

    bool hasWitch = false;
    if (opts.Witch == "true") {
        hasWitch = true;
    }

    Session session;
    if (!opts.IsNewSession) {
        string text = "";
        try {
            text = System.IO.File.ReadAllText(opts.FilePath);
        } catch (Exception e) {
            Console.Error.WriteLine(e.Message);
        }
        session = new Session(text, opts.FilePath);
    } else
        session = new Session(variant, opts.NumRounds, hasDest, hasWitch, hasRandomGold, opts.FilePath);

    var connector = new Connector(opts.Port, opts.SessionId, opts.Secret, session, terminate.Token);

    await connector.Start();

    return;
});