use tokio::net::TcpStream;

use crate::config::Config;
use crate::consts::InternalConnectionListener;
use crate::imports::*;
use crate::auth::table::{GlobalSessionTable, SessionSecret};
use crate::session::id::SessionId;

pub(crate) type NewInternalWsSender = mpsc::Sender<(SessionId, WebSocketStream<TcpStream>)>;
pub(crate) type NewInternalWsReceiver = mpsc::Receiver<(SessionId, WebSocketStream<TcpStream>)>;

// there's no InternalWsConnection as Service will send WebSocketStream<TcpStream> to fabric
// then fabric will have CoreSession generate its own connection
pub(crate) struct InternalWsService {
    pub(super) gst: GlobalSessionTable,
    pub(super) fabric_send_conn: NewInternalWsSender,
    pub(super) shutdown: broadcast::Receiver<()>,
    pub(super) listener: InternalConnectionListener
}

impl InternalWsService {
    pub fn execute(
        self,
        rt: Handle,
        cfg: Arc<Config>
    ) -> JoinHandle<()> {
        rt.spawn(async move {
            info!("internal ws up");
            match InternalWsService::main_loop(&self.gst, self.listener, self.fabric_send_conn, self.shutdown).await {
                Ok(_) => {
                    info!("internal ws terminated");
                },
                Err(e) => {
                    error!("internal ws terminated with error {}", e);
                },
            }
        })
    }
    async fn main_loop(
        gst: &GlobalSessionTable,
        mut sock: TcpListenerStream,
        fabric_send_conn: NewInternalWsSender,
        mut shutdown: broadcast::Receiver<()>
    ) -> Result<()> {
        loop {
            tokio::select! {
                Some(Ok(new_stream)) = sock.next() => {
                    info!("internal ws new stream");
                    match InternalWsService::handle_new_stream(new_stream, gst, &fabric_send_conn).await {
                        Ok(_) => {
                            info!("internal ws handshake ok");
                        },
                        Err(e) => {
                            error!("internal ws handshake err {}", e);
                        }
                    }
                },
                _ = shutdown.recv() => {
                    break;
                }
            }
        }
        Ok(())
    }
    async fn handle_new_stream(
        stream: TcpStream,
        gst: &GlobalSessionTable,
        fabric_send_conn: &NewInternalWsSender
    ) -> Result<()> {
        let (notify_session_id, recv_notify_session_id) = oneshot::channel();
        match tokio_tungstenite::accept_hdr_async(
            stream,
            |req: &tungstenite::handshake::server::Request,
             resp: tungstenite::handshake::server::Response|
             -> Result<
                tungstenite::handshake::server::Response,
                tungstenite::handshake::server::ErrorResponse,
            > {
                tokio::task::block_in_place(|| {
                    InternalWsService::handle_handshake(req, resp, gst, notify_session_id)
                })
            },
        )
        .await
        {
            Ok(wss) => {
                let session_id = recv_notify_session_id.await.unwrap();
                fabric_send_conn.send((session_id, wss)).await.wrap_err(eyre!("internal ws: failed to send new conn to fabric"))
            }
            Err(e) => Err(e.into()),
        }
    }
    fn handle_handshake(
        req: &tungstenite::handshake::server::Request,
        resp: tungstenite::handshake::server::Response,
        gst: &GlobalSessionTable,
        notify_session_id: oneshot::Sender<SessionId>
    ) -> Result<
        tungstenite::handshake::server::Response,
        tungstenite::handshake::server::ErrorResponse,
    > {
        let mut raw_sec = req.uri().path().to_string();
        raw_sec.retain(|c| c != '/');
        let secret = SessionSecret::try_from(raw_sec.as_str()).map_err(|_| {
            error!("internal ws handshake: failed to parse secret");
            Response::builder()
                .status(StatusCode::BAD_REQUEST)
                .body(Some("Cannot parse secret".to_owned()))
                .unwrap()
        })?;
        match gst.resolve_secret(&secret) {
            Some(session_id) => {
                info!("internal ws handshake: resolved {} -> {}", &secret, &session_id);
                notify_session_id.send(session_id);
                Ok(resp)
            },
            None => {
                error!("internal ws handshake: failed to resolve {}", &secret);
                Err(Response::builder()
                                .status(StatusCode::UNAUTHORIZED)
                                .body(None)
                                .unwrap())
            },
        }
    }
}