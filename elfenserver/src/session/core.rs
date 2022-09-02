use std::{sync::atomic::Ordering, fs::OpenOptions};

use crate::{imports::*, config::Config, lobby::records::{LsPutGame, ValidatedLsPutGame}, auth::{table::{GlobalSessionTable, SessionSecret}, access::AccessToken}, session::msg::FromSessionOp, consts::WS_BUFFER_LEN};

use super::{id::SessionId, msg::{ToSessionEvent, FromSessionEvent, ToSessionOp}};

pub(crate) struct CoreSession {
    session_id: SessionId,
    pub backing_file: PathBuf,
    config: Arc<Config>,
    jh_proc: JoinHandle<Result<()>>,
    conn: Option<CoreSessionConnection>,
    put: ValidatedLsPutGame,
    send_from_session: broadcast::Sender<FromSessionEvent>,
    secret: SessionSecret,
    gst: GlobalSessionTable
}

impl CoreSession {
    pub fn gs_name(&self) -> String {
        self.put.service()
    }
    pub fn save_name(&self) -> &str {
        self.backing_file.file_name().unwrap().to_str().unwrap()
    }
}

struct CoreSessionConnection {
    handle: JoinHandle<Result<()>>,
    send_op: mpsc::Sender<ToSessionOp>
}

impl Drop for CoreSession {
    fn drop(&mut self) {
        self.jh_proc.abort();
        match &self.conn {
            Some(c) => {
                c.handle.abort();
            },
            None => {},
        }
    }
}

impl CoreSession {
    pub async fn start(
        hd: &Handle,
        config: Arc<Config>,
        validated_put: ValidatedLsPutGame,
        send_from_session: broadcast::Sender<FromSessionEvent>,
        secret: SessionSecret,
        gst: GlobalSessionTable
    ) -> CoreSession {
        info!("starting {} {}", &validated_put, &secret);
        let inner_secret = secret.clone();
        let inner_send_from_session = send_from_session.clone();
        let inner_gst = gst.clone();
        let secret_content = secret.inner();
        let session = validated_put.session_id();
        gst.associate_secret(&secret, &validated_put);
        let service = validated_put.service();
        let backing_file = validated_put.backing_file();
        let is_new = validated_put.is_new();
        CoreSession {
            session_id: session.clone(),
            backing_file: validated_put.backing_file(),
            config: config.clone(),
            jh_proc: hd.spawn_blocking(move || {
                let session = session;
                if !config.demo_mode {
                    info!("{} starting elfencore", &session);
                    match CoreSession::cmd_dotnet_run(&config, &session, &service, &inner_secret, &backing_file, is_new) {
                        Ok(mut res) => {
                            res.wait().wrap_err("dotnet process wasn't running")?;
                            info!("{} elfencore exited", &session);
                            inner_send_from_session.send(FromSessionEvent::ProcessEnded(session.clone()));
                        },
                        Err(e) => {
                            error!("failed to start dotnet binary: {}", e);
                            panic!();
                        }
                    }
                } else {
                    warn!("{} in demo mode, no automatic disconnection since no child process will run", &session);
                    info!("{} manually connect to ws://127.0.0.1:{}/{}", &session, config.internal_ws_port, secret_content);
                }
                Ok(())
            }),
            put: validated_put,
            conn: None,
            send_from_session,
            secret,
            gst,
        }
    }
    fn serve(
        session: SessionId,
        rt: &Handle,
        mut t: WebSocketStream<TcpStream>,
        mut shutdown: broadcast::Receiver<()>,
        send_from_session: broadcast::Sender<FromSessionEvent>,
        config: Arc<Config>,
        gst: GlobalSessionTable
    ) -> CoreSessionConnection {
        let (send_op, mut recv_op) = mpsc::channel::<ToSessionOp>(WS_BUFFER_LEN);
        CoreSessionConnection {
            handle: rt.spawn(async move {
                info!("{} core connection established", &session);
                let active_players = gst.session_joinable_players(&session);
                let ser: ToSessionOp = ToSessionOp::StartGame {
                    players: active_players.iter().map(|p| p.raw().to_owned()).collect()
                }.into();
                tokio::time::sleep(Duration::from_secs(2)).await;
                let p_list = ser.into();
                info!("sending player list {:?}", p_list);
                t.send(p_list).await;
                loop {
                    tokio::select! {
                        maybe_msg = t.next() => {
                            match maybe_msg {
                                Some(Ok(msg)) => {
                                    match msg {
                                        Message::Ping(bs) => {
                                            t.send(Message::Pong(bs)).await;
                                        },
                                        Message::Pong(bs) => {
                                            t.send(Message::Pong(bs)).await;
                                        },
                                        msg => match FromSessionOp::try_from(msg.clone()) {
                                            Ok(op) => {
                                                // info!("FROM CORE {} {:?}", &session, &msg);
                                                match op {
                                                    FromSessionOp::Single { target, payload } => {
                                                        send_from_session.send(FromSessionEvent::Send(target, payload));
                                                    },
                                                    FromSessionOp::Broadcast { payload } => {
                                                        send_from_session.send(FromSessionEvent::Broadcast(session.clone(), payload));
                                                    },
                                                    FromSessionOp::Save {players} => {
                                                        send_from_session.send(FromSessionEvent::Save(session.clone(), players));
                                                    }
                                                }
                                            },
                                            Err(e) => {
                                                warn!("unknown message from core {:?} {}", &msg, e);
                                            },
                                        }
                                    }
                                },
                                Some(Err(e)) => {
                                    error!("iwc {} unexpected dc {}", &session, e);
                                    break;
                                },
                                None => {
                                    info!("iwc {} dc", &session);
                                    break;
                                },
                            }
                        },
                        op = recv_op.recv() => {
                            match op {
                                Some(op) => {
                                    // info!("TO CORE {} {:?}", &session, &op);
                                    t.send(op.into()).await;
                                },
                                None => {
                                    info!("iwc {} no more ops", &session);
                                    break;
                                }
                            }
                        },
                        _ = shutdown.recv() => {
                            info!("iwc {} shutdown signal", &session);
                            break;
                        }
                    }
                }
                info!("{} core connection ended", &session);
                t.close(None).await;
                send_from_session.send(FromSessionEvent::ConnectionEnded(session.clone()));
                Ok(())
            }),
            send_op
        }
    }
    pub fn bind_stream(&mut self,
        rt: &Handle,
        t: WebSocketStream<TcpStream>,
        s: broadcast::Sender<FromSessionEvent>,
        shutdown: broadcast::Receiver<()>
    ) {
        self.conn = Some(CoreSession::serve(self.session_id, rt, t, shutdown, self.send_from_session.clone(), self.config.clone(), self.gst.clone()));
    }
    pub fn cmd_dotnet_publish(cfg: &Config) -> Result<Child> {
        Command::new("dotnet").current_dir(&cfg.core_session_project_path)
            .args(["publish", "-o", cfg.built_core_path.canonicalize()?.to_str().ok_or(eyre!("invalid file path"))?]).spawn().wrap_err("failed to publish dotnet binary")
    }
    pub fn unbind_stream(&mut self) {
        self.conn = None;
    }
    fn cmd_dotnet_run(cfg: &Config, session_id: &SessionId, game_name: &str, secret: &SessionSecret, file_path: &Path, is_new: bool) -> Result<Child> {
        let unbased = base64::decode_config(game_name, base64::URL_SAFE_NO_PAD)?;
        let decoded_game_name = String::from_utf8_lossy(&unbased);
        let mut split_game_vars = decoded_game_name.split(':');
        let variant = split_game_vars.next().ok_or(eyre!("no variant"))?;
        let rounds = split_game_vars.next().ok_or(eyre!("no rounds"))?;
        let dest_town = split_game_vars.next().ok_or(eyre!("no dest town"))?;
        let mut cmd = Command::new("dotnet");
        info!("cmd ok");
        info!("afp {:?}", file_path);
        if is_new {
            // create the file
            OpenOptions::new().write(true).create(true).truncate(false).open(file_path)?;
        }
        let absolute_fp = file_path.canonicalize()?;
        info!("afp ok");
        let validated_file_path = absolute_fp.to_str().ok_or(eyre!("invalid file path"))?;
        info!("vfp ok");

        cmd.current_dir(&cfg.built_core_path.canonicalize()?)
            .args([
                &cfg.dotnet_binary_name,
                "--",
                "--filepath",
                validated_file_path,
                "--sessionid",
                &session_id.inner().to_string(),
                "--port",
                &cfg.internal_ws_port.to_string(),
                "--secret",
                &secret.inner(),
                "--gamevariant",
                variant,
                "--numrounds",
                rounds,
                "--destination",
                dest_town
            ]);
        if variant == "gold" {
            let witch = split_game_vars.next().ok_or(eyre!("no witch"))?;
            let random_gold = split_game_vars.next().ok_or(eyre!("no random gold"))?;
            cmd.args([ "--witch", witch, "--randomgold", random_gold]);
        }
        if is_new {
            cmd.args(["--isnewsession"]);
        }
        info!("{:?}", &cmd);
        cmd.spawn().wrap_err("failed to spawn dotnet process")
    }
    pub async fn signal(&self, ev: ToSessionEvent) {
        match &self.conn {
            Some(conn) => {
                match ev {
                    ToSessionEvent::Connected(u, s) => {
                        assert_eq!(s, self.session_id);
                        conn.send_op.send(ToSessionOp::Connected{user: u.raw().to_owned()}).await;
                    },
                    ToSessionEvent::Packet(u, s, p) => {
                        assert_eq!(s, self.session_id);
                        conn.send_op.send(ToSessionOp::Payload {from: u.raw().to_owned(), payload: p}).await;
                    },
                    ToSessionEvent::Disconnected(u, s) => {
                        assert_eq!(s, self.session_id);
                        conn.send_op.send(ToSessionOp::Disconnected{user: u.raw().to_owned()}).await;
                    },
                }
            },
            None => {
                warn!("{} no connection to core, ignoring ev", &self.session_id);
            },
        }
    }
}