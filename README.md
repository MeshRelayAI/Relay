# Mesh Relay

Run your own **Mesh** relay: the small server that routes end-to-end-encrypted
messages between people. A relay never sees message contents; it only passes
along encrypted data and knows who is connected. Running one keeps even the
metadata (who talks to whom) with you.

> Just want to **use** Mesh? You don't need this. Get the app from
> [MeshRelayAI/Mesh](https://github.com/MeshRelayAI/Mesh). This repo is only for
> people who want to host the server themselves.

This repository contains the **full relay source**, published under AGPL-3.0, so
you can read exactly what the server does before you trust it with your traffic.
The end-to-end encryption itself lives in the separately published, permissively
licensed [Mesh.Shared](https://github.com/MeshRelayAI/Shared) library (vendored
here under `src/Mesh.Shared`).

## Run it

### Fastest: one command (prebuilt image)

```bash
docker run -p 8080:8080 ghcr.io/meshrelayai/relay
```

That's a full relay on `http://localhost:8080`. No download, no build. Point a
Mesh client at `http://your-host:8080` (or put it behind an HTTPS reverse proxy).

### Build the image from source

```bash
git clone https://github.com/MeshRelayAI/Relay.git
cd Relay
docker build -t mesh-relay .
docker run -p 8080:8080 mesh-relay
```

Or with Docker Compose:

```bash
docker compose up mesh-relay
```

Durable + multi-replica (adds Redis for presence, quota and cross-node routing):

```bash
docker compose --profile redis up
```

### Run it directly with the .NET SDK (no Docker)

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/Mesh.Relay
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
| `CONNECTOR_{KEY}_CLIENT_ID` | Override a knowledge-connector OAuth client id (e.g. `CONNECTOR_DROPBOX_CLIENT_ID`) | built-in |
| `CONNECTOR_{KEY}_SECRET` | The matching OAuth client secret (confidential providers only) | none |

## Health and metrics

- `GET /health` returns `{"status":"ok"}`
- `GET /metrics` returns aggregate counters (no handles or PII)

## Notes

- Always put a public relay behind TLS.
- A handle is registered per relay; users on different relays can't message each
  other (there's no federation between relays).
- See `SELF-HOSTING.md` for security details and scaling guidance.

## License

The relay is licensed under the
[GNU Affero General Public License v3.0](LICENSE) (AGPL-3.0). If you run a modified
version of this relay as a network service, the AGPL requires you to offer that
modified source to its users.

The vendored `src/Mesh.Shared` protocol and encryption library is licensed
separately under the [Apache License 2.0](src/Mesh.Shared/LICENSE).
