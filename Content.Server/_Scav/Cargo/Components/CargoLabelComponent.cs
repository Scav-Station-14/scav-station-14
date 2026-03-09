namespace Content.Server._Scav.Cargo.Components;

/// <summary>
/// This is used for marking containers as
/// containing miscellaneous salvage
/// </summary>
[RegisterComponent]
public sealed partial class CargoLabelComponent : Component
{
    /// <summary>
    /// The Station to receive payment for this sale
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? AssociatedStationId;
}
