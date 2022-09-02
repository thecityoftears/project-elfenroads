use tokio::net::TcpStream;

use crate::{imports::*, session::{id::SessionId, msg::{ToSessionEvent, OpaquePayload}}, auth::{table::GlobalSessionTable, user::User}};

pub(super) struct PlayerConnection {
    pub ws_stream: WebSocketStream<TcpStream>,
    pub player_name: User,
    pub session_id: SessionId,
    pub gst: GlobalSessionTable,
    pub signal_stop: broadcast::Receiver<()>,
    pub fabric_gs_in: mpsc::Sender<ToSessionEvent>,
    pub payload_out: mpsc::Receiver<OpaquePayload>,
    pub signal_intentional_dc: mpsc::Receiver<()>
}

impl PlayerConnection {
    pub async fn handle(mut self) {
        let mut run = true;
        self.fabric_gs_in
            .send(ToSessionEvent::Connected(
                self.player_name.clone(),
                self.session_id.clone(),
            ))
            .await;
        while run {
            tokio::select! {
                maybe_msg = self.ws_stream.next() => {
                    match maybe_msg {
                        Some(Ok(msg)) => {
                            // info!("{} INCOMING {:?}", &self, &msg);
                            match msg {
                                Message::Text(v) => {
                                    self.fabric_gs_in.send(ToSessionEvent::Packet(self.player_name.clone(), self.session_id.clone(), OpaquePayload::from(v))).await;
                                },
                                Message::Close(_) => {
                                    self.ws_stream.close(None).await;
                                    run = false;
                                },
                                Message::Ping(bs) => {
                                    info!("{} ping", &self);
                                    self.ws_stream.send(Message::Pong(bs)).await;
                                },
                                Message::Pong(bs) => {
                                    info!("{} pong", &self);
                                    self.ws_stream.send(Message::Pong(bs)).await;
                                },
                                _ => {
                                    warn!("unknown message");
                                }
                            }
                        },
                        Some(Err(e)) => {
                            error!("{} player connection error {}", &self, e);
                            self.ws_stream.close(None).await;
                            run = false;
                        },
                        None => {
                            warn!("{} no more ws messages", &self);
                            self.ws_stream.close(None).await;
                            run = false;
                        }
                    }
                },
                _ = self.signal_stop.recv() => {
                    // info!("{} graceful shutdown", &self);
                    self.ws_stream.close(None).await;
                    run = false;
                },
                maybe_out = self.payload_out.recv() => {
                    match maybe_out {
                        Some(msg) => {
                            // info!("{} OUTGOING {:?}", &self, &msg);
                            self.ws_stream.send(msg.into()).await;
                        },
                        None => {
                            warn!("{} out bus closed?", &self);
                            self.ws_stream.close(Some(CloseFrame {
                                code: CloseCode::Normal,
                                reason: Cow::Borrowed("out bus closed")
                            })).await;
                            run = false;
                        },
                    }
                },
                _ = self.signal_intentional_dc.recv() => {
                    // info!("{} session ordered disconnection", &self);
                    self.ws_stream.close(Some(CloseFrame {
                        code: CloseCode::Normal,
                        reason: Cow::Borrowed("received disconnect from session")
                    })).await;
                    run = false;
                }
            }
        }
        self.fabric_gs_in.send(ToSessionEvent::Disconnected(self.player_name.clone(), self.session_id.clone())).await;
        self.gst.try_remove_player(&self.player_name, false);
        info!("{} stopped", &self);
    }
}

impl Display for PlayerConnection {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "<pwc:{}:{}>",
            self.player_name, self.session_id
        )
    }
}