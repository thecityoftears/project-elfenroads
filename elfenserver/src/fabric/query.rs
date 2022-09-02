use crate::{imports::*, auth::{access::AccessToken, table::PlayerLockResult}, session::id::SessionId};

/// Incoming messages which expect a reply. Used by services.
pub(crate) struct Query<I, O>(mpsc::UnboundedSender<(I, oneshot::Sender<O>)>);

impl<I, O> Query<I, O> {
    pub fn new() -> (Query<I, O>, QueryConnector<I, O>) {
        let (s, r) = mpsc::unbounded_channel();
        (Query(s), QueryConnector(r))
    }
    pub async fn submit_and_wait(&self, msg: I) -> Result<O> {
        let (o_s, o_r) = oneshot::channel();
        self.0.send((msg, o_s)).map_err(|_| Box::new("query self send err"));
        o_r.await.map_err(|e| e.into())
    }
    pub fn blocking_send_and_recv(&self, msg: I) -> Option<O> {
        let (s, mut r) = oneshot::channel();
        self.0.send((msg, s));
        // info!("query sent");
        std::thread::sleep(Duration::from_millis(500));
        match r.try_recv() {
            Ok(v) => {
                // info!("query here");
                Some(v)
            }
            Err(_) => {
                // warn!("no query result");
                None
            }
        }
    }
}

impl<I, O> Clone for Query<I, O> {
    fn clone(&self) -> Self {
        Self(self.0.clone())
    }
}
/// Incoming messages which expect a reply. Returned by service creation, used by fabric to link the system together.
pub(crate) struct QueryConnector<I, O>(mpsc::UnboundedReceiver<(I, oneshot::Sender<O>)>);

impl<I, O> Stream for QueryConnector<I, O> {
    type Item = (I, oneshot::Sender<O>);

    fn poll_next(
        mut self: std::pin::Pin<&mut Self>,
        cx: &mut std::task::Context<'_>,
    ) -> std::task::Poll<Option<Self::Item>> {
        self.0.poll_recv(cx)
    }
}

pub(crate) type QueryConnectorLockUser = QueryConnector<(AccessToken, SessionId), PlayerLockResult>;
pub(crate) type QueryLockUser = Query<(AccessToken, SessionId), PlayerLockResult>;