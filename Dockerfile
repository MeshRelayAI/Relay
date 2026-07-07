# Build the Mesh relay from source, then run it on a small runtime image.
#
#   docker build -t mesh-relay .
#   docker run -p 8080:8080 mesh-relay
#
# The relay runs fully standalone with no external services: an in-memory handle
# registry, presence and quota. Set REDIS_CONNECTION to make presence, quota and
# cross-node routing durable/multi-replica, and MODEL_* to offer a hosted free
# model. See SELF-HOSTING.md.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/ ./src/
RUN dotnet publish src/Mesh.Relay/Mesh.Relay.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Mesh.Relay.dll"]
