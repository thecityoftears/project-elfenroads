use tokio::net::TcpStream;

use crate::{imports::*, auth::{table::{GlobalSessionTable, PlayerLockResult}, user::User, access::AccessToken}, session::{msg::{ToSessionEvent, FromSessionEvent, OpaquePayload}, id::SessionId}, fabric::{query::QueryLockUser}, config::Config, lobby::client::LobbyServiceClient, consts::ExternalConnectionListener};

use super::conn::PlayerConnection;

pub(crate) struct PublicWsService {
    pub(super) passthrough_gst: GlobalSessionTable,
    pub(super) stopper: broadcast::Sender<()>,
    pub(super) send_to_session: mpsc::Sender<ToSessionEvent>,
    pub(super) passthrough_request_player_name: QueryLockUser,
    pub(super) listener: ExternalConnectionListener,
    pub(super) config: Arc<Config>,
    pub(super) recv_from_session: broadcast::Receiver<FromSessionEvent>
}

struct PublicWsConnection {
    conn: JoinHandle<()>,
    out: mpsc::Sender<OpaquePayload>,
    send_dc: mpsc::Sender<()>,
    user: User
}

impl PublicWsConnection {
    pub async fn out(&self, msg: OpaquePayload) {
        self.out.send(msg).await;
    }
    pub async fn disconnect(&self) {
        self.send_dc.send(()).await;
    }
}

impl PublicWsService {
    pub fn execute(
        self,
        rt: Handle,
        lsc: Arc<LobbyServiceClient>,
    ) -> tokio::task::JoinHandle<Result<()>> {
        let outer_rt = rt.clone();
        outer_rt.spawn(async move {
            info!("public ws up");
            match PublicWsService::main_loop(
                self.config,
                &lsc,
                &rt,
                self.passthrough_gst.clone(),
                &self.stopper,
                self.send_to_session,
                self.passthrough_request_player_name,
                self.recv_from_session,
                self.listener
            )
            .await {
                Ok(_) => {
                    info!("public ws terminated");
                    Ok(())
                },
                Err(e) => {
                    error!("public ws terminated with error {}", e);
                    Err(e)
                }
            }
        })
    }
    async fn main_loop(
        cfg: Arc<Config>,
        lsc: &LobbyServiceClient,
        rt: &Handle, // to spawn individual client handlers
        gst: GlobalSessionTable,
        passthrough_emit_stop: &broadcast::Sender<()>,
        send_to_session: mpsc::Sender<ToSessionEvent>,
        passthrough_request_player_name: QueryLockUser,
        recv_from_session: broadcast::Receiver<FromSessionEvent>,
        mut listener: TcpListenerStream
    ) -> Result<()> {
        let mut signal_stop = passthrough_emit_stop.subscribe();
        let mut shared_bus_out = recv_from_session;
        let mut individuals = HashMap::new();
        loop {
            tokio::select! {
                // new public conn
                Some(Ok(new_stream)) = listener.next() => {
                    match PublicWsService::handle_new_stream(
                        new_stream,
                        lsc,
                        cfg.clone(),
                        gst.clone(),
                        rt.clone(),
                        passthrough_emit_stop.subscribe(),
                        send_to_session.clone(),
                        passthrough_request_player_name.clone()
                    ).await {
                        Ok(p_conn) => {
                            // info!("{}", &p_conn.user);
                            individuals.insert(p_conn.user.clone(), p_conn);
                        },
                        Err(e) => {
                            // non-critical error, just warn
                            warn!("Failed to accept new ws connection: {}", e);
                        },
                    }
                },
                // stop
                _ = signal_stop.recv() => {
                    break;
                },
                maybe_out = shared_bus_out.recv() => {
                    match maybe_out {
                        Ok(out_msg) => {
                            PublicWsService::handle_outgoing_event(
                                out_msg, &mut individuals, &gst
                            ).await;
                        }
                        Err(_) => {
                            error!("public ws: shared bus closed");
                            break;
                        }
                    }
                }
            }
        }
        Ok(())
    }
    async fn handle_outgoing_event(
        ev: FromSessionEvent,
        individuals: &mut HashMap<User, PublicWsConnection>,
        gst: &GlobalSessionTable
    ) {
        match ev {
            FromSessionEvent::Send(username, message) => {
                if let Some(p) = individuals.get(&username) {
                    // info!("ws -> worker: {} {:?}", &username, &message);
                    p.out(message).await;
                } else {
                    warn!("ws: {} not here for outgoing individual message", username);
                }
            },
            FromSessionEvent::Broadcast(session_id, message) => {
                let gst_matching_players = gst.session_active_players(&session_id);
                for (active_name, p) in individuals.iter() {
                    if gst_matching_players.contains(active_name) {
                        // info!("ws -> worker broadcast: {} {:?}", &active_name, &message);
                        p.out(message.clone()).await;
                    }
                }
            },
            FromSessionEvent::ProcessEnded(session_id) => {
                let gst_matching_players = gst.session_active_players(&session_id);
                for (active_name, p) in individuals.iter() {
                    if gst_matching_players.contains(active_name) {
                        // info!("ws -> worker disconnect: {}", &active_name);
                        p.disconnect().await;
                    }
                }
            },
            // ignore
            _ => {}
        }
    }
    async fn handle_new_stream(
        stream: TcpStream,
        _lsc: &LobbyServiceClient,
        cfg: Arc<Config>,
        gpl: GlobalSessionTable,
        rt: Handle,
        signal_stop: broadcast::Receiver<()>,
        passthrough_fabric_gs_send: mpsc::Sender<ToSessionEvent>,
        passthrough_request_player_name: QueryLockUser
    ) -> Result<PublicWsConnection> {
        let (notify_player_added, recv_notify_player_added) = oneshot::channel();
        match tokio_tungstenite::accept_hdr_async(
            stream,
            |req: &tungstenite::handshake::server::Request,
             resp: tungstenite::handshake::server::Response|
             -> Result<
                tungstenite::handshake::server::Response,
                tungstenite::handshake::server::ErrorResponse,
            > {
                tokio::task::block_in_place(|| {
                    PublicWsService::handle_handshake(req, resp, cfg, gpl.clone(), passthrough_request_player_name, notify_player_added)
                })
            },
        )
        .await
        {
            Ok(wss) => {
                let (player_name, session_id) = recv_notify_player_added.await.unwrap();
                let (private_send_out, private_gs_out) = mpsc::channel(1000);
                let (send_dc, recv_dc) = mpsc::channel(10);
                Ok(PublicWsConnection {
                    conn: rt.spawn(PlayerConnection {
                        ws_stream: wss,
                        player_name: player_name.clone(),
                        session_id,
                        gst: gpl,
                        signal_stop,
                        fabric_gs_in: passthrough_fabric_gs_send,
                        payload_out: private_gs_out,
                        signal_intentional_dc: recv_dc
                    }.handle()),
                    out: private_send_out,
                    send_dc,
                    user: player_name
                })
            }
            Err(e) => Err(e.into()),
        }
    }
    // can't be async because of tungstenite limitations
    fn handle_handshake(
        req: &tungstenite::handshake::server::Request,
        resp: tungstenite::handshake::server::Response,
        cfg: Arc<Config>,
        gst: GlobalSessionTable,
        request_player_name: QueryLockUser,
        notify_player_added: oneshot::Sender<(User, SessionId)>,
    ) -> Result<
        tungstenite::handshake::server::Response,
        tungstenite::handshake::server::ErrorResponse,
    > {
        info!("ws header: parsing {:?}", &req);
        if let Some(host) = req.headers().get("host") {
            let url = Url::parse(
                &format!("ws://{}{}", host.to_str().unwrap(), &req.uri().to_string())
            ).map_err(|_| {
                error!("ws header: cannot parse url");
                Response::builder()
                    .status(StatusCode::BAD_REQUEST)
                    .body(Some("Not a URL".to_owned()))
                    .unwrap()
            })?;
            match SessionId::try_from(&url) {
                Ok(requested_session) => {
                    let token = AccessToken::try_from(&url).map_err(|e| {
                        warn!("ws header: get access token from url failed");
                        Response::builder()
                            .status(StatusCode::BAD_REQUEST)
                            .body(Some(e.to_string()))
                            .unwrap()
                    })?;
                    let token_report = token.clone();
                    if let Some(q) = request_player_name.blocking_send_and_recv((
                        token.clone(),
                        requested_session
                    )) {
                        match q {
                            PlayerLockResult::Success(player_name, session) => {
                                notify_player_added.send((player_name, session));
                                Ok(resp)
                            }
                            PlayerLockResult::AlreadyInSession(session_id) => {
                                let msg = format!(
                                    "ws header: {} already in session {}, rejecting",
                                    &token_report, &session_id
                                );
                                error!("{}", msg);
                                Err(Response::builder()
                                    .status(StatusCode::UNAUTHORIZED)
                                    .body(Some(msg))
                                    .unwrap())
                            }
                            PlayerLockResult::InvalidUser => {
                                let msg = format!("ws header: invalid token {}, rejecting", &token_report);
                                error!("{}", msg);
                                Err(Response::builder()
                                    .status(StatusCode::UNAUTHORIZED)
                                    .body(Some(msg))
                                    .unwrap())
                            }
                            PlayerLockResult::NotJoinable(session_id) => {
                                let msg = format!("ws header: session {} not joinable", session_id);
                                error!("{}", msg);
                                Err(Response::builder()
                                    .status(StatusCode::UNAUTHORIZED)
                                    .body(Some(msg))
                                    .unwrap())
                            }
                            PlayerLockResult::NoSuchSession(session_id) => {
                                let msg = format!("ws header: no such session {}", session_id);
                                error!("{}", msg);
                                Err(Response::builder()
                                    .status(StatusCode::UNAUTHORIZED)
                                    .body(Some(msg))
                                    .unwrap())
                            }
                        }
                    } else {
                        let msg = format!("ws header: timeout getting token {}", &token);
                        error!("{}", msg);
                        Err(Response::builder()
                            .status(StatusCode::INTERNAL_SERVER_ERROR)
                            .body(Some(msg))
                            .unwrap())
                    }
                }
                Err(e) => {
                    // invalid session id
                    Err(Response::builder()
                        .status(StatusCode::BAD_REQUEST)
                        .body(Some(e.to_string()))
                        .unwrap())
                }
            }
        } else {
            warn!("ws header: missing host");
            Err(Response::builder()
                .status(StatusCode::BAD_REQUEST)
                .body(Some("Header missing host".to_owned()))
                .unwrap())
        }
    }
}