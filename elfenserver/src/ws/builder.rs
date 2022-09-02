use crate::{imports::*, auth::table::GlobalSessionTable, session::msg::{ToSessionEvent, FromSessionEvent}, fabric::query::QueryLockUser, config::Config, consts::{ExternalConnectionListener, InternalConnectionListener}};

use super::{public::PublicWsService, internal::{InternalWsService, NewInternalWsSender}};

pub(crate) struct WsBuilder {
    stop_cloner: broadcast::Sender<()>,
    gst: GlobalSessionTable,
    config: Arc<Config>
}

impl WsBuilder {
    pub fn new(stop_cloner: broadcast::Sender<()>, gst: GlobalSessionTable, config: Arc<Config>) -> WsBuilder {
        WsBuilder {stop_cloner, gst, config}
    }
    fn public_ws_stream(rt: &Handle, cfg: &Config) -> Result<ExternalConnectionListener> {
        let addr = cfg
            .public_ws_url
            .socket_addrs(|| None)?
            .into_iter()
            .next()
            .ok_or(eyre!("no socket addresses"))?;
        rt.block_on(async { 
            let tcp_listener = TcpListener::bind(addr).await?;
            let t_stream = TcpListenerStream::new(tcp_listener);
            Ok(t_stream)
        })
    }
    fn internal_ws_stream(rt: &Handle, cfg: &Config) -> Result<InternalConnectionListener> {
        rt.block_on(async { 
            let tcp_listener = TcpListener::bind(&SocketAddr::new(IpAddr::V4(Ipv4Addr::new(127, 0, 0, 1)), cfg.internal_ws_port)).await?;
            let t_stream = TcpListenerStream::new(tcp_listener);
            Ok(t_stream)
        })
    }
    pub fn build_public_service(
        &self,
        rt: &Handle,
        send_to_session: mpsc::Sender<ToSessionEvent>,
        recv_from_session: broadcast::Receiver<FromSessionEvent>,
        request_player_name: QueryLockUser
    ) -> Result<PublicWsService> {
        let public_listener = WsBuilder::public_ws_stream(rt, &self.config)?;
        Ok(PublicWsService {
            passthrough_gst: self.gst.clone(),
            stopper: self.stop_cloner.clone(),
            send_to_session,
            passthrough_request_player_name: request_player_name,
            listener: public_listener,
            config: self.config.clone(),
            recv_from_session
        })
    }
    pub fn build_internal_service(
        &self, 
        rt: &Handle,
        fabric_send_conn: NewInternalWsSender
    ) -> Result<InternalWsService> {
        let internal_listener = WsBuilder::internal_ws_stream(rt, &self.config)?;
        Ok(InternalWsService {
            gst: self.gst.clone(),
            fabric_send_conn,
            shutdown: self.stop_cloner.subscribe(),
            listener: internal_listener
        })
    }
}