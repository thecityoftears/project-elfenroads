use crate::{
    auth::{table::{GlobalSessionTable, PlayerLockResult}, access::AccessToken},
    config::Config,
    imports::*,
    lobby::{client::LobbyServiceClient, records::{LsPutGame, LsGetToken}, service::{BuiltWebApi, WebService}},
    session::{core::CoreSession, id::SessionId, msg::{ToSessionEvent, FromSessionEvent}},
    ws::{public::PublicWsService, internal::{NewInternalWsReceiver, InternalWsService}},
};

use super::query::QueryConnector;

pub(crate) struct Fabric {
    pub lsc: Arc<LobbyServiceClient>,
    pub send_shutdown: broadcast::Sender<()>,
    pub gst: GlobalSessionTable,
    pub to_session_recv: mpsc::Receiver<ToSessionEvent>,
    pub send_from_session: broadcast::Sender<FromSessionEvent>,
    pub recv_from_session: broadcast::Receiver<FromSessionEvent>,
    pub qc_access_token: QueryConnector<(AccessToken, SessionId), PlayerLockResult>,
    pub qc_put: QueryConnector<(SessionId, LsPutGame), bool>,
    pub qc_del: QueryConnector<SessionId, bool>,
    pub internal_ws_conn: NewInternalWsReceiver
}

async fn access_token_ticker(cfg: &Config, lsg: &LsGetToken) -> tokio::time::Interval {
    let refresh_duration = std::cmp::max(cfg.min_token_refresh_s, lsg.expires_in);
    info!("access token refreshes in {} seconds", refresh_duration);
    let mut at_ticker = interval(Duration::from_secs(
        refresh_duration
    ));
    // skip first tick
    at_ticker.tick().await;
    at_ticker
}

impl Fabric {
    async fn main_loop(
        mut self,
        rt: &Handle,
        cfg: Arc<Config>,
        mut ctrlc_signal: mpsc::Receiver<()>,
        initial_get_token: LsGetToken
    ) -> Result<()> {
        let mut sessions: HashMap<SessionId, CoreSession> = HashMap::new();
        let allowed_games = cfg.get_variants();
        info!("{}", &cfg);
        let mut at_ticker = access_token_ticker(&cfg, &initial_get_token).await;
        let mut refresh_token = initial_get_token.refresh_token;
        let mut access_token = initial_get_token.access_token;
        loop {
            tokio::select! {
                // console signalled stop
                _ = ctrlc_signal.recv() => {
                    Fabric::handle_shutdown(
                        &cfg,
                        self.send_shutdown
                    ).await;
                    break;
                },
                // new internal ws conn
                Some((session_id, ws_stream)) = self.internal_ws_conn.recv() => {
                    match sessions.get_mut(&session_id) {
                        Some(session) => {
                            session.bind_stream(rt, ws_stream, self.send_from_session.clone(), self.send_shutdown.subscribe());
                        },
                        None => {
                            error!("new internal ws conn but {} doesn't exist", &session_id);
                        }
                    }
                },
                // incoming game put request
                Some(req_put) = self.qc_put.next() => {
                    Fabric::handle_web_put(
                        rt,
                        cfg.clone(),
                        &mut sessions,
                        &req_put.0.0,
                        &self.gst,
                        req_put.0.1,
                        &allowed_games,
                        req_put.1,
                        self.send_from_session.clone()
                    ).await;
                },
                // incoming game del request
                Some(req_del) = self.qc_del.next() => {
                    Fabric::handle_web_del(
                        &req_del.0,
                        &self.gst,
                        req_del.1
                    ).await;
                },
                // access token-username resolution request
                Some(((access_token, session_id), send_back)) = self.qc_access_token.next() => {
                    send_back.send(self.gst.try_add_player(&self.lsc, &access_token, session_id).await);
                },
                // public ws incoming
                Some(ws_incoming) = self.to_session_recv.recv() => {
                    Fabric::handle_ws_incoming(
                        &mut sessions,
                        ws_incoming
                    ).await;
                },
                maybe = self.recv_from_session.recv() => match maybe {
                    Ok(fs_ev) => match fs_ev {
                        FromSessionEvent::ProcessEnded(session) => {
                            if let Some(_) = sessions.remove(&session) {
                                info!("{} process ended, nuking from existence and unpairing secret", &session);
                                tokio::time::sleep(
                                    Duration::from_millis(500)
                                ).await;
                                self.gst.try_remove_session(&session);
                                // put save
                            } else {
                                warn!("{} process ended, tried to nuke but it no longer exists", &session);
                            }
                        },
                        FromSessionEvent::ConnectionEnded(session) => {
                            if let Some(s) = sessions.get_mut(&session) {
                                info!("{} connection ended, allowing reconnect until process ends", &session);
                                s.unbind_stream();
                            } else {
                                warn!("{} connection ended but session not found!", &session);
                            }
                        }
                        FromSessionEvent::Save(session, players) => {
                            if let Some(s) = sessions.get(&session) {
                                info!("{} saving", &session);
                                match self.lsc.put_save_game(&access_token, &s.gs_name(), players.iter().map(|u| u.raw().to_owned()).collect(), s.save_name()).await {
                                    Ok(msg) => {
                                        info!("{} put save ok {}", &session, msg);
                                    },
                                    Err(e) => {
                                        error!("{} failed to put save {}", &session, e);
                                    }
                                }
                            } else {
                                warn!("{} save signal but session not found!", &session);
                            }
                        },
                        _ => {

                        }
                    },
                    Err(e) => {
                        error!("from session event bus disconnected {}", e);
                        break;
                    }
                },
                _ = at_ticker.tick() => match self.lsc.refresh_token(&refresh_token).await {
                    Ok(lsg) => {
                        info!("token refresh ok");
                        at_ticker = access_token_ticker(&cfg, &lsg).await;
                        refresh_token = lsg.refresh_token;
                        access_token = lsg.access_token;
                    },
                    Err(e) => {
                        error!("failed to refresh access token: {}", e);
                    }
                }
            }
        }
        Ok(())
    }
    pub(crate) fn coordinate(
        self,
        cfg: Arc<Config>,
        rt: &Handle,
        services: FabricServices,
        get_token: LsGetToken,
        ctrlc_signal: mpsc::Receiver<()>,
    ) -> Result<()> {
        let rt_main = rt.clone();
        rt_main.block_on(async move {
            services.run_until_completion(self, cfg, get_token, ctrlc_signal).await
        })
    }
}

pub(crate) struct FabricServices {
    pub web: WebService,
    pub public_ws: PublicWsService,
    pub internal_ws: InternalWsService,
    pub rt: Handle
}

impl FabricServices {
    async fn run_until_completion(
        self,
        fabric: Fabric,
        cfg: Arc<Config>,
        get_token: LsGetToken,
        ctrlc_signal: mpsc::Receiver<()>,
    ) -> Result<()> {
        let jh_web = self.web.execute(self.rt.clone());
        let jh_public_ws = self.public_ws.execute(
            self.rt.clone(),
            fabric.lsc.clone(),
        );
        let jh_internal_ws = self.internal_ws.execute(self.rt.clone(), cfg.clone());
        // wait for other handles to exit
        let (end_fabric, end_web, end_internal_ws, end_public_ws) = futures::join!(
            fabric.main_loop(
                &self.rt,
                cfg,
                ctrlc_signal,
                get_token
            ),
            jh_web,
            jh_internal_ws,
            jh_public_ws
        );
        info!("fabric exited with {:?}", end_fabric);
        info!("web exited with {:?}", end_web);
        info!("internal ws exited with {:?}", end_internal_ws);
        info!("public ws exited with {:?}", end_public_ws);
        Ok(())
    }
}