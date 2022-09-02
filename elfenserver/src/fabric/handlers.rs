use std::ops::Deref;

use crate::{
    auth::{
        table::{GlobalSessionTable, SessionSecret},
        user::User, access::AccessToken,
    },
    config::Config,
    imports::*,
    lobby::records::LsPutGame,
    session::{
        core::CoreSession,
        id::SessionId,
        msg::{FromSessionEvent, ToSessionEvent},
    },
};

use super::service::Fabric;

impl Fabric {
    pub(super) async fn handle_web_put(
        rt: &Handle,
        cfg: Arc<Config>,
        sessions: &mut HashMap<SessionId, CoreSession>,
        session_id: &SessionId,
        gst: &GlobalSessionTable,
        put: LsPutGame,
        allowed_games: &Vec<String>,
        reply: oneshot::Sender<bool>,
        send_from_session: broadcast::Sender<FromSessionEvent>
    ) -> Result<SessionId> {
        match put.validate(session_id, &cfg) {
            Ok(validated_put) => match gst.try_add_session(&validated_put) {
                Ok(session_id) => {
                    info!("fabric: web req put ok, session {}", session_id);
                    sessions.insert(
                        session_id,
                        CoreSession::start(
                            rt,
                            cfg.clone(),
                            validated_put,
                            send_from_session,
                            SessionSecret::new(),
                            gst.clone(),
                        )
                        .await,
                    );
                    reply.send(true);
                    Ok(session_id)
                }
                Err(e) => {
                    warn!("fabric: web req put fail: {}", &e);
                    reply.send(false);
                    Err(eyre!(e))
                }
            },
            Err(e) => {
                error!("hpg fail: {}", e);
                Err(e)
            }
        }
    }
    pub(super) async fn handle_web_del(
        session_id: &SessionId,
        gst: &GlobalSessionTable,
        reply: oneshot::Sender<bool>,
    ) -> Option<SessionId> {
        info!("fabric: web req del");
        gst.try_remove_session(session_id);
        reply.send(true);
        Some(*session_id)
        /*match gst.try_remove_session(session_id) {
            Some(session_id) => {
                reply.send(true);
                Some(session_id)
            }
            None => {
                reply.send(false);
                None
            }
        }*/
    }
    pub(super) async fn handle_shutdown(cfg: &Config, send_shutdown: broadcast::Sender<()>) {
        info!("fabric stopping");
        send_shutdown.send(());
    }
    pub(super) async fn handle_ws_incoming(
        sessions: &mut HashMap<SessionId, CoreSession>,
        ws_incoming: ToSessionEvent,
    ) {
        // info!("{:?}", &ws_incoming);
        match ws_incoming {
            ToSessionEvent::Connected(player_name, session_id) => {
                if let Some(session) = sessions.get(&session_id) {
                    session
                        .signal(ToSessionEvent::Connected(player_name, session_id))
                        .await;
                } else {
                    warn!("{} no longer exists", &session_id);
                }
            }
            ToSessionEvent::Packet(player_name, session_id, raw_payload) => {
                if let Some(session) = sessions.get(&session_id) {
                    session
                        .signal(ToSessionEvent::Packet(player_name, session_id, raw_payload))
                        .await;
                } else {
                    warn!("{} no longer exists", &session_id);
                }
            }
            ToSessionEvent::Disconnected(player_name, session_id) => {
                if let Some(session) = sessions.get(&session_id) {
                    session
                        .signal(ToSessionEvent::Disconnected(player_name, session_id))
                        .await;
                } else {
                    warn!("{} no longer exists", &session_id);
                }
            }
        }
    }
}
