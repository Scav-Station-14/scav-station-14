using Robust.Shared.Serialization;

namespace Content.Shared._Scav.Shipyard.Events;

/// <summary>
///     Get a ship from the database, load the relevant file, and spawn it in the world
/// </summary>
[Serializable, NetSerializable]
public sealed class GarageConsoleRetrieveMessage : BoundUserInterfaceMessage
{
    public Guid ShipId;
    public GarageConsoleRetrieveMessage(Guid shipId)
    {
        ShipId = shipId;
    }
}
