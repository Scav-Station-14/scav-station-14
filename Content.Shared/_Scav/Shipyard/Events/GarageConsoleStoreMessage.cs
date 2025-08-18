using Robust.Shared.Serialization;

namespace Content.Shared._Scav.Shipyard.Events;

/// <summary>
///     Store a ship in the database and remove it from the world. The button holds no info and is doing a validation check for a deed client side, but we will still check on the server.
/// </summary>
[Serializable, NetSerializable]
public sealed class GarageConsoleStoreMessage : BoundUserInterfaceMessage
{
    public GarageConsoleStoreMessage()
    {
    }
}
