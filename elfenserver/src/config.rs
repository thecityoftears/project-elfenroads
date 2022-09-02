use tokio::fs::DirEntry;
use tokio_stream::wrappers::ReadDirStream;
use walkdir::WalkDir;

use crate::auth::access::AccessToken;
use crate::consts::GENERAL_ENCODE_SET;
use crate::imports::*;

use crate::lobby::client::LobbyServiceClient;
use crate::lobby::records::LsGetToken;
use crate::session::records::SaveInfo;

#[derive(Deserialize, Serialize, Debug)]
pub(crate) struct Config {
    pub ls_url: Url,
    pub ls_admin_user: String,
    pub ls_admin_pass: String,
    pub web_url: Url,
    pub public_ws_url: Url,
    pub internal_ws_port: u16,
    pub shutdown_timeout_ms: u64,
    pub core_session_project_path: PathBuf,
    pub demo_mode: bool,
    pub savegames_path: PathBuf,
    pub min_token_refresh_s: u64,
    pub allowed_games: Vec<String>,
    pub built_core_path: PathBuf,
    pub dotnet_binary_name: String
}

impl Display for Config {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        let res = format!(
            "\n\
            ==== SUMMARY ====\n\
            Lobby Service: {}\n\
            Admin Account: {} {}\n\
            Web API URL: {}\n\
            Public WebSocket URL: {}\n\
            Internal WebSocket URL: ws://127.0.0.1:{}/\n\
            ",
            &self.ls_url,
            &self.ls_admin_user,
            &self.ls_admin_pass,
            &self.web_url,
            &self.public_ws_url,
            &self.internal_ws_port
        );
        write!(f, "{}", res)
    }
}

impl Config {
    // {variant}:{rounds int}:{dest_town bool} [gold :{random_gold}:{witch}]
    pub fn get_variants(&self) -> Vec<String> {
        self.allowed_games
        .iter()
        .map(|e| base64::encode_config(e, base64::URL_SAFE_NO_PAD))
        .collect()
    }
    pub fn fetch_admin_key(&self, rt: &Handle, lsc: &LobbyServiceClient) -> Result<LsGetToken> {
        rt.block_on(async {
            lsc.fetch_access(&self.ls_admin_user, &self.ls_admin_pass)
                .await
        })
    }
    pub fn register_games(
        &self,
        rt: &Handle,
        lsc: &LobbyServiceClient,
        admin_key: &AccessToken,
    ) {
        rt.block_on(async move {
            // ls can't handle concurrent requests
            /*self.get_variants()
                .iter()
                .map(|variant| {
                    lsc.register_game(admin_key, &self.web_url, &variant, &variant, 2, 3)
                })
                .collect::<FuturesUnordered<_>>()
                .all(|reg| async move {
                    match reg {
                        Ok(_) => true,
                        Err(e) => {
                            warn!("failed to register gameservice: {}", e);
                            false
                        }
                    }
                })
                .await*/
            for variant in self.get_variants() {
                match lsc.register_game(admin_key, &self.web_url, &variant, &variant, 2, 3).await {
                    Ok(v) => {
                        info!("registered gs {}", v);
                    },
                    Err(_) => {
                        warn!("failed to register gameservice {}", &variant);
                    }
                }
            }
        })
    }
    pub fn register_saves(&self, rt: &Handle, lsc: &LobbyServiceClient, admin_key: &AccessToken) {
        let saves = self.savegames_path.as_path();
        rt.block_on(async move {
            info!("putting saves");
            let pairs = WalkDir::new(saves).into_iter()
                .filter_map(|e| e.ok())
                .filter(|e| 
                    // {save_name}
                    e.path().is_file()
                )
                .map(|e| {
                    let p = e.path();
                    (p.file_name().unwrap().to_str().unwrap().to_owned(), p.to_owned())
                }).collect::<Vec<_>>();
            for (save_name, full_path) in pairs {
                info!("found save {:?}", full_path);
                // read the save file
                match tokio::fs::read(&full_path).await {
                    Ok(raw_save_file) => match serde_json::from_slice::<SaveInfo>(&raw_save_file) {
                        Ok(save_file) => {
                            match lsc.put_save_game(admin_key, &base64::encode_config(&save_file.raw_gs_name, GENERAL_ENCODE_SET), save_file.players, &save_name).await {
                                Ok(_) => {
                                    info!("notified ls of save {}/{}", &save_file.raw_gs_name, save_name);
                                },
                                Err(e) => {
                                    error!("failed to put save {}/{}", &save_file.raw_gs_name, save_name);
                                }
                            }
                        },
                        Err(e) => {
                            error!("failed to deserialize save {:?}: {}", &full_path, e);
                        }
                    },
                    Err(e) => {
                        error!("failed to read save {:?}: {}", &full_path, e);
                    }
                }
            }
        })
    }
}
