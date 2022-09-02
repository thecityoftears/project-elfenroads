mod auth {
    pub(crate) mod access;
    pub(crate) mod table;
    pub(crate) mod user;
}
mod fabric {
    mod handlers;
    pub(crate) mod msg;
    pub(crate) mod query;
    pub(crate) mod service;
}
mod lobby {
    pub(crate) mod client;
    pub(crate) mod handlers;
    pub(crate) mod records;
    pub(crate) mod service;
}
mod session {
    pub(crate) mod core;
    pub(crate) mod id;
    pub(crate) mod msg;
    pub(crate) mod records;
}
mod ws {
    pub(crate) mod builder;
    pub(crate) mod conn;
    pub(crate) mod internal;
    pub(crate) mod public;
}
mod config;
mod consts;
mod imports;
mod init;
mod store;

use crate::imports::*;

fn main() -> Result<()> {
    env_logger::builder().filter_level(log::LevelFilter::Info).try_init().unwrap();
    let (s_stop, r_stop) = tokio::sync::mpsc::channel(10);
    ctrlc::set_handler(move || {
        s_stop.blocking_send(());
        
    }).expect("Error setting Ctrl-C handler");
    let lc = LaunchConfig::from_args();
    let pfx = store::Prefix::try_from(lc.config_path.as_path())?;
    init::start_all(r_stop, pfx.cfg)
}


/*pub(crate) async fn make_ws_connection(
    cfg: &Config
) -> Result<impl Stream> {
    let ws_listener_sockets = cfg
            .public_ws_url
            .socket_addrs(|| None)?;
        info!("ws listen on {:?}", &ws_listener_sockets);
        let tcp_listener = TcpListener::bind(ws_listener_sockets.as_slice())
            .await?;
        Ok(TcpListenerStream::new(tcp_listener))
}*/

use init::LaunchConfig;