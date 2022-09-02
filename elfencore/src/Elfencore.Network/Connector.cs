using Elfencore.Session;
using ServerToCore = Elfencore.Shared.Messages.ServerToCore;
using Newtonsoft.Json;
using Websocket.Client;
using System.Net.WebSockets;

namespace Elfencore.Network {
    public class Connector {
        public Connector(UInt16 port, UInt64 sessionId, string secret, Session.Session session, CancellationToken signalStop) {
            this.port = port;
            this.sessionId = sessionId;
            this.secret = secret;
            this.signalStop = signalStop;
            this.session = session;
            var conn_url = new UriBuilder("ws", "127.0.0.1", this.port, this.secret);
            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket {
                Options = {
                    KeepAliveInterval = TimeSpan.FromSeconds(5),
                }
            });
            this.ws = new WebsocketClient(conn_url.Uri, factory);
            this.ws.ReconnectTimeout = null;
            this.session.OutBroadcast.CollectionChanged += (_, ev) => {
                try {
                    foreach (var msg in ev.NewItems) {
                        this.ws.Send(JsonConvert.SerializeObject(msg));
                    }
                } catch (Exception e) {
                }
            };
            this.session.OutSingle.CollectionChanged += (_, ev) => {
                try {
                    foreach (var msg in ev.NewItems) {
                        this.ws.Send(JsonConvert.SerializeObject(msg));
                    }
                } catch (Exception e) {
                }
            };
            this.session.OutSave.CollectionChanged += (_, ev) => {
                try {
                    foreach (var msg in ev.NewItems) {
                        this.ws.Send(JsonConvert.SerializeObject(msg));
                    }
                } catch (Exception e) {
                }
            };
            this.session.OutTerminate.CollectionChanged += (_, ev) => {
                try
                {
                    foreach (var msg in ev.NewItems)
                    {
                        this.ws.Send(JsonConvert.SerializeObject(msg));
                    }
                }
                catch (Exception e)
                {
                }
            };
        }
        private WebsocketClient ws;
        private UInt16 port;
        private UInt64 sessionId;
        private CancellationToken signalStop;
        private string secret;
        private Session.Session session;
        private ManualResetEvent exitEvent = new ManualResetEvent(false);
        public async Task Start() {
            this.ws.MessageReceived.Subscribe(msg => {
                try {
                    object op = null;
                    var kv = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg.Text);
                    // there must be an "op" key for sure
                    if (kv.TryGetValue("op", out op)) {
                        if ((string)op == "StartGame") {
                            var opStart = JsonConvert.DeserializeObject<ServerToCore.OpStartGame>(msg.Text);
                            session.HandleStartGame(opStart.players);
                        } else if ((string)op == "Connected") {
                            var opConnected = JsonConvert.DeserializeObject<ServerToCore.OpConnected>(msg.Text);
                            this.session.HandleConnected(opConnected.user);
                        } else if ((string)op == "Payload") {
                            var opPayload = JsonConvert.DeserializeObject<ServerToCore.OpPayload>(msg.Text);
                            this.session.HandlePayload(opPayload.from, opPayload.payload);
                        } else if ((string)op == "Disconnected") {
                            var opDisconnected = JsonConvert.DeserializeObject<ServerToCore.OpDisconnected>(msg.Text);
                            this.session.HandleDisconnected(opDisconnected.user);
                        } else {
                            Console.WriteLine("unknown op " + op);
                        }
                    } else {
                        Console.WriteLine("message is not an op!");
                    }
                } catch (Exception e) {
                    Console.WriteLine("error parsing server message: " + e);
                }
            });
            this.ws.DisconnectionHappened.Subscribe(msg => {
                Console.WriteLine("Close status: " + msg.CloseStatus);
                Console.WriteLine("Close status desc: " + msg.CloseStatusDescription);
                Console.WriteLine("Exception: " + msg.Exception);
                Console.WriteLine("Subprotocol: " + msg.SubProtocol);
                Console.WriteLine("Type: " + msg.Type);
            });
            await this.ws.Start();
            exitEvent.WaitOne();
        }
    }
}