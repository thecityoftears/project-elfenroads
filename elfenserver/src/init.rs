use crate::{
    auth::table::GlobalSessionTable,
    config::Config,
    consts::WS_BUFFER_LEN,
    fabric::{query::Query, service::{Fabric, FabricServices}},
    imports::*,
    lobby::{client::LobbyServiceClient, records::LsGetToken, service::WebService},
    ws::builder::WsBuilder, session::core::CoreSession,
};

#[derive(StructOpt, Debug)]
pub(crate) struct LaunchConfig {
    #[structopt(short = "c", long = "config", parse(from_os_str))]
    pub config_path: PathBuf,
}

pub(crate) fn start_all(ctrl_c_signal: tokio::sync::mpsc::Receiver<()>, cfg: Config) -> Result<()> {
    // config file
    let cfg = Arc::new(cfg);
    // shutdown
    let (send_shutdown, recv_shutdown) = broadcast::channel(100);
    // async runtime
    let rt = tokio::runtime::Builder::new_multi_thread()
        .enable_all()
        .build()?;
    let rt = rt.handle();

    // build dotnet binary
    rt.block_on(async {
        info!("building dotnet binary...");
        CoreSession::cmd_dotnet_publish(&cfg)?.wait().wrap_err(eyre!("failed to wait for dotnet publish"))
    })?;

    // setup channels

    // lobby <- server
    let lobby_client = LobbyServiceClient::new(cfg.ls_url.to_owned());
    // lobby -> server
    let built_web_api = WebService::build(&cfg, send_shutdown.subscribe(), send_shutdown.subscribe())?;
    // -> session
    let (send_to_session, to_session_recv) = mpsc::channel(WS_BUFFER_LEN);
    // <- session
    let (send_from_session, from_session_recv) = broadcast::channel(WS_BUFFER_LEN);
    // fabric <- new internal ws
    let (fabric_send_iws_conn, fabric_recv_iws_conn) = mpsc::channel(WS_BUFFER_LEN);
    // query access token
    let (q_access_token, qc_access_token) = Query::new();

    // global table
    let gst = GlobalSessionTable::default();

    // websocket

    // builder
    let ws_builder = WsBuilder::new(send_shutdown.clone(), gst.clone(), cfg.clone());
    // public ws service
    let built_public_ws =
        ws_builder.build_public_service(&rt, send_to_session, from_session_recv, q_access_token)?;
    // internal ws service
    let built_internal_ws = ws_builder.build_internal_service(&rt, fabric_send_iws_conn)?;

    // fetch initial access token
    let admin_key = cfg.fetch_admin_key(rt, &lobby_client)?;

    // register games
    cfg.register_games(rt, &lobby_client, &admin_key.access_token);

    // put saves
    cfg.register_saves(rt, &lobby_client, &admin_key.access_token);

    // fabric
    let fabric = Fabric {
        lsc: Arc::new(lobby_client),
        send_shutdown,
        gst,
        to_session_recv,
        recv_from_session: send_from_session.subscribe(),
        send_from_session,
        qc_access_token,
        qc_put: built_web_api.qc_put,
        qc_del: built_web_api.qc_del,
        internal_ws_conn: fabric_recv_iws_conn,
    };

    // start
    match fabric.coordinate(cfg, rt,  FabricServices {
        web: built_web_api.web_api,
        public_ws: built_public_ws,
        internal_ws: built_internal_ws,
        rt: rt.clone()
    }, admin_key, ctrl_c_signal) {
        Ok(()) => {
            info!("everything shut down successfully");
            Ok(())
        }
        Err(e) => {
            error!("critical: {}", e);
            Err(e)
        }
    }
}
