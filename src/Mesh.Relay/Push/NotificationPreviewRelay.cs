using Mesh.Relay.Backplane;
using Mesh.Relay.Hub;
using Mesh.Relay.RateLimiting;
using Mesh.Relay.Storage;
using Mesh.Shared;

namespace Mesh.Relay.Push;

/// <summary>Short-lived routing metadata only; never contains a notification payload.</summary>
public sealed record NotificationWakeRoute(
    string SenderHandle, string SenderDevice, string RecipientHandle, string RecipientDevice,
    string WakeId, DateTimeOffset ExpiresAt)
{
    public string Key => KeyFor(RecipientHandle, RecipientDevice, WakeId);
    public static string KeyFor(string handle, string device, string wake)
        => NotificationPreviewProtocol.Hash(string.Join('\0', handle, device, wake));
}

public static class NotificationPreviewRelay
{
    public static void MapNotificationPreviews(this WebApplication app)
    {
        app.MapPost(NotificationPreviewProtocol.Route, async (
            NotificationPreviewRequest request, HttpContext context, IRelayStore store,
            IBackplane backplane, MeshRouter router, IMessageRateLimiter limiter, TimeProvider clock) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            if (!NotificationPreviewProtocol.IsValidRequest(request, clock.GetUtcNow()))
                return Results.BadRequest(new { error = "invalid_preview_proof" });
            var recipient = await store.GetHandleAsync(request.Handle, context.RequestAborted);
            if (recipient is null || !recipient.DevicePublicKeys.Contains(request.PublicKey, StringComparer.Ordinal))
                return Results.Unauthorized();
            var (decision, policy) = await limiter.TryAcquireAsync(
                request.Handle, MessageRateBucket.Direct, context.RequestAborted);
            if (!policy.Enabled || !decision.Allowed) return Results.StatusCode(StatusCodes.Status429TooManyRequests);

            var device = DeviceProtocol.DeviceId(request.PublicKey);
            var route = await backplane.FindNotificationWakeAsync(request.Handle, device, request.WakeId,
                context.RequestAborted);
            if (route is null || route.ExpiresAt <= clock.GetUtcNow()) return Results.NoContent();
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
            deadline.CancelAfter(TimeSpan.FromSeconds(8));
            try
            {
                var response = await router.ReadNotificationPreviewAsync(route, request, deadline.Token);
                if (response is null) return Results.NoContent();
                var sender = await store.GetHandleAsync(route.SenderHandle, deadline.Token);
                recipient = await store.GetHandleAsync(request.Handle, deadline.Token);
                if (recipient?.DevicePublicKeys.Contains(request.PublicKey, StringComparer.Ordinal) != true)
                    return Results.Unauthorized();
                if (response.SenderHandle != route.SenderHandle
                    || response.SenderPublicKey is not { Length: > 0 and <= 256 }
                    || DeviceProtocol.DeviceId(response.SenderPublicKey) != route.SenderDevice
                    || sender?.DevicePublicKeys.Contains(response.SenderPublicKey, StringComparer.Ordinal) != true
                    || response.Ciphertext is not { Length: > 0 and <= NotificationPreviewProtocol.MaxResponseBytes }
                    || !MeshCrypto.Verify(response.SenderPublicKey,
                        NotificationPreviewProtocol.ResponseCanonical(request, response), response.Signature))
                {
                    app.Logger.LogWarning("Notification preview rejected: invalid sender proof.");
                    return Results.StatusCode(StatusCodes.Status502BadGateway);
                }
                return Results.Ok(response);
            }
            catch (OperationCanceledException) when (!context.RequestAborted.IsCancellationRequested)
            {
                app.Logger.LogDebug("Notification preview retrieval deadline reached.");
                return Results.StatusCode(StatusCodes.Status504GatewayTimeout);
            }
            catch (Microsoft.AspNetCore.SignalR.HubException ex)
            {
                app.Logger.LogDebug(ex, "Notification preview peer unavailable.");
                return Results.NoContent();
            }
        });
    }
}
