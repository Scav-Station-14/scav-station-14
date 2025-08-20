namespace Content.Server._Scav.Persistence;

[RegisterComponent]
public sealed partial class ShuttlePersistenceTrackerComponent : Component
{
    /// <summary>
    /// Database ID of the ship record this ship is attached to.
    /// </summary>
    [DataField]
    public int ShipId { get; set; }
}
