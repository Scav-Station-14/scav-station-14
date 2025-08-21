namespace Content.Server._Scav.Persistence;

/// <summary>
/// Used to track the ShipId of a ship spawned from a garage console. Please note that due to EntitySerializer not liking Guids, grids containing this component cannot be saved.
/// </summary>
[RegisterComponent]
public sealed partial class ShuttlePersistenceTrackerComponent : Component
{
    /// <summary>
    /// Database ID of the ship record this ship is attached to.
    /// </summary>
    [DataField] [NonSerialized]
    public Guid ShipId;
}
