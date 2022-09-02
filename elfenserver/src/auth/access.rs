use crate::{imports::*, consts::AUTH_ENCODE_SET};

/// Deserialized access token. Usually provided by a URL with a trailing `?access_token={value}`.
/// 
/// Content shouldn't be assumed to be urlencoded.
#[derive(Clone, Debug, Deserialize, Serialize)]
pub(crate) struct AccessToken(String);

impl Display for AccessToken {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "<token:`{}`>", &self.0)
    }
}

impl AccessToken {
    pub fn encode(&self) -> String {
        percent_encoding::percent_encode(self.0.as_bytes(), AUTH_ENCODE_SET).to_string()
    }
}
impl TryFrom<&Url> for AccessToken {
    type Error = eyre::Report;

    /// Try parse `AccessToken` from a `Url`.
    /// 
    /// Success if there exists a query pair `access_token`, `{value}`.
    fn try_from(value: &Url) -> Result<Self, Self::Error> {
        if let Some((_, access_token)) = value.query_pairs().find(|(k, _v)| {
            k == "access_token"
        }) {
            Ok(AccessToken(access_token.to_string()))
        } else {
            Err(eyre!("access_token not in url {}", value))
        }
    }
}