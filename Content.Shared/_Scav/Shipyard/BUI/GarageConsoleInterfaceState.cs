using Robust.Shared.Serialization;

namespace Content.Shared._Scav.Shipyard.BUI;

[NetSerializable, Serializable]
public sealed class GarageConsoleInterfaceState : BoundUserInterfaceState
{
    public readonly string? ShipDeedTitle;
    public readonly bool IsTargetIdPresent;
    public readonly byte UiKey;


    public GarageConsoleInterfaceState(
        string? shipDeedTitle,
        bool isTargetIdPresent,
        byte uiKey)
    {
        ShipDeedTitle = shipDeedTitle;
        IsTargetIdPresent = isTargetIdPresent;
        UiKey = uiKey;
    }
}
