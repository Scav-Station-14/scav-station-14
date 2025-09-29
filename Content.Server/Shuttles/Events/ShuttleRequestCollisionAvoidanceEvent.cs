namespace Content.Server.Shuttles.Events;

/// <summary>
/// Request for collision avoidance system check, called every frame but will only actually run every half-second or so to prevent performance nightmare
/// </summary>
[ByRefEvent]
public record struct ShuttleRequestCollisionAvoidanceEvent
{

}
