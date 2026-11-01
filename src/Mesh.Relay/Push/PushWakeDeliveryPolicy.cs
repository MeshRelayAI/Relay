using Mesh.Shared;

namespace Mesh.Relay.Push;

internal static class PushWakeDeliveryPolicy
{
    public const int TimeToLiveSeconds = 300;

    // Keep notification-worthy Android data wakes separate from routine synchronization.
    public static string CollapseKey(PushWakeMode mode)
        => mode switch
        {
            PushWakeMode.AlertAndSync => "mesh-alert-sync",
            PushWakeMode.SyncOnly => "mesh-sync",
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
}
