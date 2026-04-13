using Content.Shared.Cargo;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Stacks;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Scav.Cargo.Components;

/// <summary>
/// A console that manipulates the distribution of revenue on the station.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(SharedCargoSystem))]
public sealed partial class FundManagementConsoleComponent : Component
{
    /// <summary>
    /// Sound played when the budget distribution is set.
    /// </summary>
    [DataField]
    public SoundSpecifier SetDistributionSound = new SoundCollectionSpecifier("CargoPing");

    [DataField]
    public SoundSpecifier ErrorSound = new SoundCollectionSpecifier("CargoError");

    /// <summary>
    /// The time at which account actions can be performed again.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextAccountActionTime;

    /// <summary>
    /// The minimum time between account actions when <see cref="TransferUnbounded"/> is false
    /// </summary>
    [DataField]
    public TimeSpan AccountActionDelay = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The stack representing cash dispensed on withdrawals.
    /// </summary>
    [DataField]
    public ProtoId<StackPrototype> CashType = "Credit";

    public static string CashSlotId = "station-bank-ATM-cashSlot";

    [DataField]
    public ItemSlot CashSlot = new();

    [DataField]
    public ProtoId<CargoAccountPrototype> SelectedAccount = "Cargo";

    /// <summary>
    /// The time at which the console will be able to play the deny sound.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextDenySoundTime = TimeSpan.Zero;

    /// <summary>
    /// The time between playing the deny sound.
    /// </summary>
    [DataField]
    public TimeSpan DenySoundDelay = TimeSpan.FromSeconds(2);
}
