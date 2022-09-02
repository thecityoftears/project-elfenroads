use std::{path::Path, io::BufReader, fs::File};
use std::io::prelude::*;

use crate::{config::Config};

pub(crate) struct Prefix {
    pub cfg: Config
}

impl TryFrom<&Path> for Prefix {
    type Error = eyre::Report;

    fn try_from(value: &Path) -> Result<Self, Self::Error> {
        let f = File::open(value)?;
        let mut buf = BufReader::new(f);
        let mut v = Vec::new();
        buf.read_to_end(&mut v);
        match toml::from_slice(&v) {
            Ok(cfg) => {
                Ok(Prefix {
                    cfg
                })
            },
            Err(e) => {
                Err(e.into())
            },
        }
    }
}