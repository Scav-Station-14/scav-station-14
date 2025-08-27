namespace Content.Shared._Scav.Persistence;

/// <summary>
/// Used to track the ShipId of a ship spawned from a garage console. Due to EntitySerializer not liking Guids, the Guid is stored as a string here.
/// </summary>
[RegisterComponent]
public sealed partial class ShuttlePersistenceTrackerComponent : Component
{
    /// <summary>
    /// Database ID of the ship record this ship is attached to.
    /// </summary>
    [DataField] [NonSerialized]
    public string ShipGuid;
}
