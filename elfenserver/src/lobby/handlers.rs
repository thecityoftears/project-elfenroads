use crate::{imports::*, session::id::SessionId, fabric::query::Query};

use super::records::LsPutGame;

pub(crate) struct LobbyServiceHandler {}

impl LobbyServiceHandler {
    pub(crate) async fn handle_put_game(
        game_name: String,
        session_id: SessionId,
        put: LsPutGame,
        fb_put_game: Query<(SessionId, LsPutGame), bool>,
    ) -> Result<impl Reply, Rejection> {
        if game_name != put.game_service {
            error!("hpg game_name {} != put.game_server {}", &game_name, &put.game_service);
            Err(warp::reject())
        } else {
            match fb_put_game.submit_and_wait((session_id, put)).await {
                Ok(true) => {
                    // info!("hpg ok");
                    Ok(warp::reply())
                },
                _ => {
                    // error!("hpg reject");
                    Err(warp::reject())
                },
            }
        }
    }
    // todo
    pub(crate) async fn handle_delete_game(
        game_name: String,
        session_id: SessionId,
        fb_del_game: Query<SessionId, bool>,
    ) -> Result<impl Reply, Rejection> {
        match fb_del_game.submit_and_wait(session_id).await {
            Ok(true) => Ok(warp::reply()),
            _ => Err(warp::reject()),
        }
    }
}