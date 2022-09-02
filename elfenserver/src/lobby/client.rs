use crate::{
    auth::{access::AccessToken, user::User},
    consts::GENERAL_ENCODE_SET,
    imports::*,
};

use super::records::{LsGetToken, LsPutSave, LsRegisterGame};

#[derive(Clone)]
pub(crate) struct LobbyServiceClient {
    ls_url: Url,
    c: reqwest::Client,
}

impl LobbyServiceClient {
    pub fn new(lobby_service_url: Url) -> LobbyServiceClient {
        Self {
            ls_url: lobby_service_url,
            c: reqwest::ClientBuilder::new().build().unwrap(),
        }
    }
    /// `PUT /api/gameservices/{gameservice}`
    /// Request-Parameters: `access_token=...`
    /// Content-Type: `application/json`
    pub async fn register_game(
        &self,
        admin_key: &AccessToken,
        web_url: &Url,
        gs_name: &str,
        gs_alias: &str,
        min_session_players: u8,
        max_session_players: u8,
    ) -> Result<String> {
        let mut call_url = self.ls_url.clone();
        let mut call_url_segments = call_url.path_segments_mut().unwrap();
        call_url_segments
            .push("api")
            .push("gameservices")
            .push(gs_name);
        drop(call_url_segments);
        call_url.set_query(Some(&format!("access_token={}", admin_key.encode())));
        let mut game_url = web_url.clone();
        game_url.path_segments_mut().unwrap().push(gs_name);
        let mut game_url = game_url.to_string();
        let try_reg = LsRegisterGame {
            location: game_url,
            max_session_players,
            min_session_players,
            name: gs_name.to_owned(),
            display_name: gs_alias.to_owned(),
            web_support: false,
        };
        match self
            .c
            .put(call_url.to_string())
            .header("Content-Type", "application/json")
            .json(&try_reg)
            .send()
            .await
        {
            Ok(resp) => {
                if resp.status().is_success() {
                    Ok(gs_name.to_string())
                } else {
                    Err(eyre!("ls query register game: bad response {:?}", resp))
                }
            }
            Err(e) => Err(eyre!("ls query register game: request failed: {}", e)),
        }
    }
    /// `DEL /api/gameservices/{gameservice}`
    /// Request-Parameters: `access_token=...`
    /// Content-Type: `application/json`
    pub async fn deregister_game(
        &self,
        admin_key: &AccessToken,
        ws_url: &Url,
        gs_name: &str,
    ) -> Result<()> {
        let mut game_url = ws_url.clone();
        game_url.set_path(gs_name);
        let mut call_url = self.ls_url.clone();
        let mut call_url_segments = call_url.path_segments_mut().unwrap();
        call_url_segments
            .push("api")
            .push("gameservices")
            .push(gs_name);
        drop(call_url_segments);
        call_url.set_query(Some(&format!("access_token={}", admin_key.encode())));
        match self.c.delete(call_url.to_string()).send().await {
            Ok(resp) => {
                if resp.status().is_success() {
                    Ok(())
                } else {
                    Err(eyre!("ls query deregister game: bad response {:?}", resp))
                }
            }
            Err(e) => Err(eyre!("ls query deregister game: request failed: {}", e)),
        }
    }
    pub async fn fetch_access(&self, username: &str, password: &str) -> Result<LsGetToken> {
        info!("fetching access token for user {}", username);
        /*let q = format!(
            "{}oauth/token?grant_type=password&username={}&password={}",
            &self.ls_url, username, password
        );*/
        let mut call_url = self.ls_url.clone();
        let mut call_url_segments = call_url.path_segments_mut().unwrap();
        call_url_segments.push("oauth").push("token");
        drop(call_url_segments);
        let mut queries = call_url.query_pairs_mut();
        queries
            .append_pair("grant_type", "password")
            .append_pair("username", username)
            .append_pair("password", password)
            .finish();
        drop(queries);
        match self
            .c
            .post(call_url)
            .basic_auth("bgp-client-name", Some("bgp-client-pw"))
            .send()
            .await
        {
            Ok(resp) => {
                if resp.status().is_success() {
                    let resp_body = resp.text().await?;
                    serde_json::from_str(&resp_body).map_err(|e| e.into())
                } else {
                    Err(eyre!("ls query fetch access token: bad response {:?}", resp))
                }
            }
            Err(e) => Err(eyre!("ls query fetch access token: request failed: {}", e)),
        }
    }
    pub async fn refresh_token(&self, tk: &AccessToken) -> Result<LsGetToken> {
        info!("refreshing token with {}", tk);
        let mut call_url = self.ls_url.clone();
        let mut call_url_segments = call_url.path_segments_mut().unwrap();
        call_url_segments.push("oauth").push("token");
        drop(call_url_segments);
        let mut queries = call_url.query_pairs_mut();
        queries
            .append_pair("grant_type", "refresh_token")
            .append_pair("refresh_token", &tk.encode())
            .finish();
        drop(queries);
        match self
            .c
            .post(call_url)
            .basic_auth("bgp-client-name", Some("bgp-client-pw"))
            .send()
            .await
        {
            Ok(resp) => {
                if resp.status().is_success() {
                    let resp_body = resp.text().await?;
                    serde_json::from_str(&resp_body).map_err(|e| e.into())
                } else {
                    Err(eyre!("ls query refresh token: bad response {:?}", resp))
                }
            }
            Err(e) => Err(eyre!("ls query refresh token: request failed: {}", e)),
        }
    }
    /// `GET /oauth/username`
    /// Request-Parameters: `access_token=...`
    pub async fn get_user_name(&self, user_token: &AccessToken) -> Result<String> {
        info!("resolving username from token {}", user_token);
        match self
            .c
            .get(&format!(
                "{}oauth/username?access_token={}",
                &self.ls_url,
                user_token.encode()
            ))
            .send()
            .await
        {
            Ok(resp) => {
                if resp.status().is_success() {
                    resp.text().await.map_err(|e| e.into())
                } else {
                    Err(eyre!("ls query get username: bad response {:?}", resp))
                }
            }
            Err(e) => Err(eyre!("ls query get username: request failed: {}", e)),
        }
    }
    /// `PUT /api/gameservices/{gameservice}/savegames/{savegame}
    pub async fn put_save_game(
        &self,
        admin_key: &AccessToken,
        gs_name: &str,
        players: Vec<String>,
        save_id: &str,
    ) -> Result<String> {
        let mut call_url = self.ls_url.clone();
        let mut call_url_segments = call_url.path_segments_mut().unwrap();
        call_url_segments
            .push("api")
            .push("gameservices")
            .push(&gs_name)
            .push("savegames")
            .push(save_id);
        drop(call_url_segments);
        call_url.set_query(Some(&format!("access_token={}", admin_key.encode())));
        match self
            .c
            .put(call_url.to_string())
            .header("Content-Type", "application/json")
            .json(&LsPutSave {
                game_name: gs_name.to_owned(),
                players,
                save_game_id: save_id.to_owned(),
            })
            .send()
            .await
        {
            Ok(resp) => if resp.status().is_success() {
                resp.text().await.map_err(|e| e.into())
            } else {
                Err(eyre!("ls query get username: bad response {:?}", resp))
            },
            Err(e) => Err(eyre!("ls put save game: request failed: {}", e)),
        }
    }
}
