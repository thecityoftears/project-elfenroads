use crate::{imports::*, fabric::query::{QueryConnector, Query}, session::id::SessionId, config::Config};

use super::{records::LsPutGame, handlers::LobbyServiceHandler};

pub(crate) type WebApiTermination = ();

pub(crate) struct WebService {
    is_at: SocketAddr,
    fut: Pin<Box<dyn Future<Output = ()> + Send>>,
    broadcast_stop: broadcast::Receiver<()>
}
pub(crate) struct BuiltWebApi {
    pub web_api: WebService,
    pub qc_put: QueryConnector<(SessionId, LsPutGame), bool>,
    pub qc_del: QueryConnector<SessionId, bool>,
}

impl WebService {
    pub fn execute(self, rt: Handle) -> tokio::task::JoinHandle<()> {
        rt.spawn(async move {
            let fut = self.fut;
            let mut broadcast_stop = self.broadcast_stop;
            info!("web is up");
            tokio::select! {
                _ = fut => {
                },
                _ = broadcast_stop.recv() => {
                }
            }
            info!("web terminated");
        })
    }
    pub fn build(
        cfg: &Config,
        graceful_shutdown: broadcast::Receiver<()>,
        broadcast_stop: broadcast::Receiver<()>
    ) -> Result<BuiltWebApi> {
        let web_api_url = cfg.web_url.clone();
        let socket_addrs = web_api_url.socket_addrs(|| None)?;
        let _graceful_shutdown = graceful_shutdown;
        if let Some(web_api_socket) = socket_addrs.first() {
            let web_api_socket = web_api_socket.to_owned();
            let (q_put, qc_put) = Query::new();
            let (q_del, qc_del) = Query::new();
    
            let routes = {
                // `PUT /{game_name}/api/games/{session}`
                warp::path::param()
                    .and(warp::path("api"))
                    .and(warp::path("games"))
                    .and(warp::path::param())
                    .and(warp::filters::method::put())
                    .and(warp::filters::body::json())
                    .and(warp::any().map(move || q_put.clone()))
                    .and_then(LobbyServiceHandler::handle_put_game)
            }
            .or(
                // `DEL /{game_name}/api/games/{session}`
                warp::path::param()
                    .and(warp::path("api"))
                    .and(warp::path("games"))
                    .and(warp::path::param())
                    .and(warp::filters::method::delete())
                    .and(warp::any().map(move || q_del.clone()))
                    .and_then(LobbyServiceHandler::handle_delete_game)
            );
            let fut = warp::serve(
                routes
            )
            .try_bind(web_api_socket);
            Ok(BuiltWebApi {
                web_api: WebService {
                    is_at: web_api_socket,
                    fut: Box::pin(fut),
                    broadcast_stop,
                },
                qc_put,
                qc_del,
            })
        } else {
            Err(eyre!("No socket addresses"))
        }
    }
}
