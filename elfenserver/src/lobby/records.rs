use crate::{imports::*, auth::{access::AccessToken, user::User}, config::Config, session::id::SessionId};

#[derive(Deserialize, Serialize, Debug, Clone)]
#[serde(rename_all = "camelCase")]
pub(crate) struct LsPlayer {
    pub name: String,
    pub preferred_colour: String,
}

#[derive(Deserialize, Serialize, Debug, Clone)]
#[serde(rename_all = "camelCase")]
pub(crate) struct LsPutGame {
    pub creator: String,
    #[serde(rename = "gameServer")]
    pub game_service: String,
    pub players: Vec<LsPlayer>,
    pub savegame: String,
}

#[derive(Deserialize, Serialize)]
pub(crate) struct LsPutSave {
    #[serde(rename = "gamename")]
    pub game_name: String,
    pub players: Vec<String>,
    #[serde(rename = "savegameid")]
    pub save_game_id: String
}

impl LsPutGame {
    pub fn validate(self, session_id: &SessionId, config: &Config) -> Result<ValidatedLsPutGame> {
        // check that it's a valid gs
        if !config.get_variants().iter().any(|e| &self.game_service == e) {
            Err(eyre!("unknown game {}", &self.game_service))
        } else {
            // if savegame is given, the file must already exist
            if !self.savegame.is_empty() {
                let mut fp = config.savegames_path.clone();
                fp.push(self.savegame);
                if fp.is_file() {
                    Ok(ValidatedLsPutGame {
                        session_id: *session_id,
                        creator: self.creator,
                        service: self.game_service,
                        players: self.players.iter().map(|ls_p| User::from(ls_p.to_owned())).collect(),
                        save_path: fp,
                        is_new: false
                    })
                } else {
                    Err(eyre!("{:?} not a file", &fp))
                }
            } else {
                let mut fp = config.savegames_path.clone();
                // by default the filename is `{session_id}`
                fp.push(
                    session_id.raw()
                );
                // generate a filename
                Ok(ValidatedLsPutGame {
                    session_id: *session_id,
                    creator: self.creator,
                    service: self.game_service,
                    players: self.players.iter().map(|ls_p| User::from(ls_p.to_owned())).collect(),
                    save_path: fp,
                    is_new: true
                })
            }
        }
    }
}

pub(crate) struct ValidatedLsPutGame {
    session_id: SessionId,
    creator: String,
    service: String,
    players: Vec<User>,
    save_path: PathBuf,
    is_new: bool
}

impl ValidatedLsPutGame {
    pub fn session_id(&self) -> SessionId {
        self.session_id.clone()
    }
    pub fn joinable_players(&self) -> Vec<User> {
        self.players.clone()
    }
    pub fn backing_file(&self) -> PathBuf {
        self.save_path.clone()
    }
    pub fn service(&self) -> String {
        self.service.clone()
    }
    pub fn is_new(&self) -> bool {
        self.is_new
    }
}

impl Display for ValidatedLsPutGame {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        self.session_id.fmt(f)
    }
}

#[derive(Deserialize, Serialize, Debug)]
#[serde(rename_all = "camelCase")]
pub(super) struct LsRegisterGame {
    pub location: String,
    pub max_session_players: u8,
    pub min_session_players: u8,
    pub name: String,
    pub display_name: String,
    pub web_support: bool,
}

#[derive(Deserialize, Serialize, Debug)]
pub(crate) struct LsGetToken {
    pub access_token: AccessToken,
    pub expires_in: u64,
    pub refresh_token: AccessToken,
    pub scope: String,
    pub token_type: String,
}