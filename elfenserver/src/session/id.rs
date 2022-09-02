use crate::imports::*;

#[derive(Deserialize, Serialize, Clone, Debug, PartialEq, Eq, Hash, Copy)]
pub(crate) struct SessionId(u64);

impl SessionId {
    pub fn inner(&self) -> u64 {
        self.0
    }
    pub fn raw(&self) -> String {
        self.0.to_string()
    }
}

impl Display for SessionId {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "<session:{}>", &self.0)
    }
}

impl FromStr for SessionId {
    type Err = eyre::Report;

    fn from_str(s: &str) -> Result<Self, Self::Err> {
        match s.parse::<u64>() {
            Ok(v) => Ok(SessionId(v)),
            Err(e) => Err(e.into()),
        }
    }
}

impl TryFrom<&Url> for SessionId {
    type Error = eyre::Report;

    /// `ws://{address}/{game_name}/{session_id}`
    fn try_from(value: &Url) -> Result<Self, Self::Error> {
        let mut sgmts = value.path_segments().ok_or(eyre!("failed to get segments of url {}", value))?;
        sgmts.next().ok_or(eyre!("no first path segment"))?;
        let str_id = sgmts.next().ok_or(eyre!("no second path segment"))?;
        match str_id.parse() {
            Ok(v) => Ok(SessionId(v)),
            Err(e) => Err(eyre!(e))
        }
    }
}