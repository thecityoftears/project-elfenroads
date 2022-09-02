use crate::{imports::*, auth::user::User};
#[derive(Serialize, Deserialize)]
pub(crate) struct SaveInfo {
    #[serde(rename = "gameService")]
    pub raw_gs_name: String,
    pub players: Vec<String>
}