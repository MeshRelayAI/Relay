# Runtime-only image for the Mesh relay.
#
# This uses the prebuilt, self-contained Linux binary shipped in bin/linux-x64,
# so no .NET SDK and no source are required to build or run it.
#
#   docker build -t mesh-relay .
#   docker run -p 8080:8080 mesh-relay
#
# The relay runs fully standalone with no external services: an in-memory handle
# registry, presence and quota. Set REDIS_CONNECTION to make presence, quota and
# cross-node routing durable/multi-replica, and MODEL_* to offer a hosted free
# model. See SELF-HOSTING.md.

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0
WORKDIR /app
COPY bin/linux-x64/ ./
RUN chmod +x ./Mesh.Relay
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["./Mesh.Relay"]
