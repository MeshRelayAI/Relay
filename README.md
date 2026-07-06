# Mesh Relay

Run your own **Mesh** relay: the small server that routes end-to-end-encrypted
messages between people. A relay never sees message contents; it only passes
along encrypted data and knows who is connected. Running one keeps even the
metadata (who talks to whom) with you.

> Just want to **use** Mesh? You don't need this. Get the app from
> [MeshRelayAI/Mesh](https://github.com/MeshRelayAI/Mesh). This repo is only for
> people who want to host the server themselves.

## Run it

Grab the [latest release](../../releases/latest) (`Mesh-Relay-selfhost-*.zip`),
unzip, then pick one:

### Docker (recommended)

```bash
docker compose up mesh-relay
```

Starts a relay on `http://localhost:8080` with in-memory storage. Point a Mesh
client at `http://your-host:8080` (or put it behind an HTTPS reverse proxy).

Durable + multi-replica (adds Redis):

```bash
docker compose --profile redis up
```

### Self-contained binary (no Docker, no .NET)

```bash
bin/linux-x64/run.sh          # Linux   (or: PORT=9000 bin/linux-x64/run.sh)
bin\win-x64\run.cmd           # Windows (or: set PORT=9000 & run.cmd)
```

## Configuration (all optional)

With nothing set, the relay runs in-memory, single node, with no hosted model,
which is the right setup for a private relay. Full reference in `SELF-HOSTING.md`.

| Env var | Purpose | Default |
|---|---|---|
| `ASPNETCORE_URLS` | Listen address | `http://+:8080` |
| `REDIS_CONNECTION` | Presence, quota, cross-node routing (multi-replica) | in-memory |
| `MODEL_ENDPOINT` / `MODEL_API_KEY` / `MODEL_NAME` | Optional hosted free model (OpenAI-compatible) | none |
| `MODEL_DAILY_TOKEN_LIMIT` | Per-handle daily token budget | `100000` |
| `MESH_MSG_RATE_PER_MIN` / `MESH_MSG_BURST` | Per-handle rate limit | `120` / `30` |

## Health and metrics

- `GET /health` returns `{"status":"ok"}`
- `GET /metrics` returns aggregate counters (no handles or PII)

## Notes

- Always put a public relay behind TLS.
- A handle is registered per relay; users on different relays can't message each
  other (there's no federation between relays).
- See `SELF-HOSTING.md` for security details and scaling guidance.

## License

Free for personal and non-commercial use under the
[PolyForm Noncommercial License 1.0.0](LICENSE). Commercial use requires a
license: `ifainberg@outlook.com`.
