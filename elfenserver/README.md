# Elfenserver

Program providing an abstraction for speaking to lobbyservice, authentication, and collection of save files

## Requirements

- Rust 2021
- `libssl-dev`
- `build-essential`
- `pkg-config`
- .NET 6 for core

## Reference config

Located at `/demo_config.toml`

## Running

```
cargo run -- --config {config_file}
```