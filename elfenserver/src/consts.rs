use percent_encoding::CONTROLS;

use crate::imports::*;

pub(crate) const AUTH_ENCODE_SET: &AsciiSet = &CONTROLS.add(b'+');
pub(crate) const GENERAL_ENCODE_SET: base64::Config = base64::URL_SAFE_NO_PAD;
pub(crate) const WS_BUFFER_LEN: usize = 10000;
pub(crate) type ExternalConnectionStream = TcpStream;
pub(crate) type ExternalConnectionListener = TcpListenerStream;
pub(crate) type InternalConnectionStream = TcpStream;
pub(crate) type InternalConnectionListener = TcpListenerStream;