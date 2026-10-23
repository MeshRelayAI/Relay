using Mesh.Shared;

namespace Mesh.Relay.Push;

internal static class PushWakeDeliveryPolicy
{
    public const int TimeToLiveSeconds = 300;

    public static string ApnsCollapseKey(PushWakeMode mode, string wakeId)
        => mode == PushWakeMode.AlertAndSync
            ? NotificationPreviewProtocol.Hash("alert:" + wakeId)
            : CollapseKey(mode);

    // A later silent sync must not replace an undelivered visible alert.
    public static string CollapseKey(PushWakeMode mode)
        => mode switch
        {
            PushWakeMode.AlertAndSync => "mesh-alert-sync",
            PushWakeMode.SyncOnly => "mesh-sync",
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
}
