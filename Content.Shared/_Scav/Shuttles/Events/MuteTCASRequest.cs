using Robust.Shared.Serialization;

namespace Content.Shared._Scav.Shuttles.Events
{
    /// <summary>
    /// Raised on the client when it wishes to mute the collision avoidance alert for a console.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class MuteTCASRequest : BoundUserInterfaceMessage
    {
        public NetEntity? ConsoleEntityUid { get; set; }
        public bool Muted { get; set; }
    }
}
