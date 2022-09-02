use crate::{imports::*, session::id::SessionId, lobby::{client::LobbyServiceClient, records::ValidatedLsPutGame}};

use super::{access::AccessToken, user::User};

#[derive(PartialEq, Eq, Hash, Clone)]
pub struct SessionSecret(Uuid);

impl TryFrom<&str> for SessionSecret {
    type Error = Report;

    fn try_from(value: &str) -> Result<Self, Self::Error> {
        match Uuid::parse_str(value) {
            Ok(v) => Ok(SessionSecret(v)),
            Err(e) => Err(eyre!(e))
        }
    }
}

impl Display for SessionSecret {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "<secret:`{}`>", self.0)
    }
}

impl SessionSecret {
    pub fn new() -> SessionSecret {
        SessionSecret(Uuid::new_v4())
    }
    pub fn inner(&self) -> String {
        self.0.to_string()
    }
}

#[derive(Default)]
struct SessionLockTable {
    joined_players: HashMap<User, SessionId>,
    joinable: HashMap<SessionId, Vec<User>>,
    session_secrets: HashMap<SessionSecret, SessionId>
}

#[derive(Default)]
pub(crate) struct GlobalSessionTable(Arc<Mutex<SessionLockTable>>);

#[derive(Clone, Debug)]
pub(crate) enum PlayerLockResult {
    Success(User, SessionId),
    AlreadyInSession(SessionId),
    NoSuchSession(SessionId),
    NotJoinable(SessionId),
    InvalidUser
}

pub(crate) enum PlayerUnlockResult {
    Unlocked(SessionId),
    InvalidUser
}

impl Clone for GlobalSessionTable {
    fn clone(&self) -> Self {
        Self(self.0.clone())
    }
}

impl GlobalSessionTable {
    pub async fn try_add_player(&self, lsc: &LobbyServiceClient, token: &AccessToken, requested_session: SessionId) -> PlayerLockResult {
        if let Ok(resolved_user) = User::try_resolve(lsc, token).await {
            let mut slt = self.0.lock();
            if let Some(session) = slt.joined_players.get(&resolved_user) {
                error!("{} already in {}", resolved_user, session);
                return PlayerLockResult::AlreadyInSession(requested_session);
            }
            // does the session even exist?
            if let Some(joinable_players) = slt.joinable.get(&requested_session) {
                if joinable_players.contains(&resolved_user) {
                    slt.joined_players.insert(resolved_user.clone(), requested_session.clone());
                    PlayerLockResult::Success(resolved_user, requested_session)
                } else {
                    PlayerLockResult::NotJoinable(requested_session)
                }
            } else {
                PlayerLockResult::NoSuchSession(requested_session)
            }
        } else {
            error!("gst couldn't resolve {}", token);
            PlayerLockResult::InvalidUser
        }
    }
    pub fn try_add_session(&self, validated_put: &ValidatedLsPutGame) -> Result<SessionId> {
        let mut slt = self.0.lock();
        let session = validated_put.session_id();
        let joinable_players = validated_put.joinable_players();
        if !slt.joinable.contains_key(&session) {
            info!("adding joinable {} with {:?}", session, &joinable_players);
            slt.joinable.insert(session.clone(), joinable_players);
            Ok(session.clone())
        } else {
            Err(eyre!("{} already exists", session))
        }
    }
    pub fn try_remove_session(&self, session: &SessionId) {
        let mut slt = self.0.lock();
        slt.joinable.remove(session);
        slt.joined_players.retain(|u, s| s != session);
        slt.session_secrets.retain(|sec, s| s != session);
    }
    pub fn try_remove_player(&self, player: &User, allow_rejoin: bool) -> PlayerUnlockResult {
        let mut slt = self.0.lock();
        if let Some(old_session_id) = slt.joined_players.remove(player) {
            if !allow_rejoin {
                info!("Not allowing {} to rejoin {}", player, &old_session_id);
                // which session is the player in?
                let mut session_table = slt.joinable.remove(&old_session_id).unwrap();
                session_table = session_table.iter().filter(|p| *p != player).map(|p| p.to_owned()).collect();
                slt.joinable.insert(old_session_id.clone(), session_table);
            }
            info!("Removed {} from {}", player, &old_session_id);
            PlayerUnlockResult::Unlocked(old_session_id)
        } else {
            warn!("{} not active?", player);
            PlayerUnlockResult::InvalidUser
        }
    }
    pub fn session_joinable_players(&self, session: &SessionId) -> Vec<User> {
        let slt = self.0.lock();
        if let Some(players) = slt.joinable.get(session) {
            players.to_owned()
        } else {
            vec![]
        }
    }
    pub fn session_active_players(&self, session: &SessionId) -> Vec<User> {
        let slt = self.0.lock();
        slt.joined_players.iter().filter_map(|(player_name, session_id)| if session_id == session {
            Some(player_name.to_owned())
        } else {
            None
        }).collect()
    }
    pub fn resolve_secret(&self, secret: &SessionSecret) -> Option<SessionId> {
        let slt = self.0.lock();
        slt.session_secrets.get(secret).map(|e| *e)
    }
    pub fn associate_secret(&self, secret: &SessionSecret, validated_put: &ValidatedLsPutGame) -> Result<()> {
        let mut slt = self.0.lock();
        if let Some(existing_session) = slt.session_secrets.get(secret) {
            Err(eyre!("{} <-> {} already exists", existing_session, secret))
        } else {
            info!("{} <-> {} paired!", &validated_put.session_id(), &secret);
            slt.session_secrets.insert(secret.clone(), validated_put.session_id());
            Ok(())
        }
    }
}