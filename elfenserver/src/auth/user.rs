use crate::{imports::*, lobby::{client::LobbyServiceClient, records::LsPlayer}};

use super::access::AccessToken;

#[derive(Deserialize, Serialize, PartialEq, Eq, Hash, Clone, Debug)]
pub(crate) struct User(String);

impl Display for User {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "<user:{}>", self.0)
    }
}

impl User {
    pub async fn try_resolve(lsc: &LobbyServiceClient, token: &AccessToken) -> Result<User> {
        lsc.get_user_name(token).await.map(|s| User(s))
    }
    pub fn raw(&self) -> &str {
        &self.0
    }
}

impl From<LsPlayer> for User {
    fn from(lsp: LsPlayer) -> Self {
        User(lsp.name)
    }
}