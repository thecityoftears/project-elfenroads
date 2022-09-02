use crate::{imports::*, auth::user::User};

use super::id::SessionId;

// Stable, only constructed by public ws worker
#[derive(Debug)]
pub(crate) enum ToSessionEvent {
    Connected(User, SessionId),
    Packet(User, SessionId, OpaquePayload),
    Disconnected(User, SessionId),
}

// client -> server -> core
// core ( {op: .., payload: ..} ) -> server ( send payload | broadcast payload ) -> client
#[derive(Deserialize, Serialize, Debug, Clone)]
pub(crate) struct OpaquePayload(String);

impl From<String> for OpaquePayload {
    fn from(s: String) -> Self {
        OpaquePayload(s)
    }
}

impl Into<Message> for OpaquePayload {
    fn into(self) -> Message {
        Message::Text(self.0)
    }
}

#[derive(Deserialize, Serialize, Debug, Clone)]
pub(crate) enum FromSessionEvent {
    Send(User, OpaquePayload),
    Broadcast(SessionId, OpaquePayload),
    ConnectionEnded(SessionId),
    ProcessEnded(SessionId),
    Save(SessionId, Vec<User>)
}

#[derive(Deserialize, Serialize, Debug)]
#[serde(tag = "op")]
pub(crate) enum FromSessionOp {
    Single {
        target: User,
        payload: OpaquePayload
    },
    Broadcast {
        payload: OpaquePayload
    },
    Save {
        players: Vec<User>
    }
}

impl TryFrom<Message> for FromSessionOp {
    type Error = Report;

    fn try_from(value: Message) -> Result<Self, Self::Error> {
        match value {
            Message::Text(v) => {
                serde_json::from_str(&v).wrap_err(eyre!("serde error"))
            },
            _ => Err(eyre!("unknown message"))
        }
    }
}

#[derive(Deserialize, Serialize, Debug)]
#[serde(tag = "op")]
pub(crate) enum ToSessionOp {
    Connected {
        user: String
    },
    Payload {
        from: String,
        payload: OpaquePayload
    },
    Disconnected {
        user: String
    },
    StartGame {
        players: Vec<String>
    }
}

impl Into<Message> for ToSessionOp {
    fn into(self) -> Message {
        Message::Text(serde_json::to_string(&self).unwrap())
    }
}