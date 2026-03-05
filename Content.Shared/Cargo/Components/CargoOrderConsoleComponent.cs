using Content.Shared.Access;
using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Content.Shared.Radio;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Cargo.Components;

/// <summary>
/// Handles sending order requests to cargo. Doesn't handle orders themselves via shuttle or telepads.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(SharedCargoSystem))]
public sealed partial class CargoOrderConsoleComponent : Component
{
    /// <summary>
    /// The account that this console pulls from for ordering.
    /// </summary>
    [DataField]
    public ProtoId<CargoAccountPrototype> Account = "Cargo";

    [DataField]
    public SoundSpecifier ErrorSound = new SoundCollectionSpecifier("CargoError");

    /// <summary>
    /// All of the <see cref="CargoProductPrototype.Group"/>s that are supported.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ProtoId<CargoMarketPrototype>> AllowedGroups = new()
    {
        "market",
        "SalvageJobReward2",
        "SalvageJobReward3",
        "SalvageJobRewardMAX",
    };

    /// <summary>
    /// Radio channel on which order approval announcements are transmitted
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<RadioChannelPrototype> AnnouncementChannel = "Supply";

    /// <summary>
    /// Secondary radio channel which always receives order announcements.
    /// </summary>
    public static readonly ProtoId<RadioChannelPrototype> BaseAnnouncementChannel = "Supply";

    /// <summary>
    /// The behaviour of the cargo console regarding orders
    /// </summary>
    [DataField]
    public CargoOrderConsoleMode Mode = CargoOrderConsoleMode.DirectOrder;

    /// <summary>
    /// The time at which the console will be able to print a slip again.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextPrintTime = TimeSpan.Zero;

    /// <summary>
    /// The time between prints.
    /// </summary>
    [DataField]
    public TimeSpan PrintDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The sound made when printing occurs
    /// </summary>
    [DataField]
    public SoundSpecifier PrintSound = new SoundCollectionSpecifier("PrinterPrint");

    /// <summary>
    /// The sound made when an order slip is scanned
    /// </summary>
    [DataField]
    public SoundSpecifier ScanSound = new SoundCollectionSpecifier("CargoBeep");

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

/// <summary>
/// The behaviour of the cargo order console
/// </summary>
[Serializable, NetSerializable]
public enum CargoOrderConsoleMode : byte
{
    /// <summary>
    /// Place orders directly
    /// </summary>
    DirectOrder,
    /// <summary>
    /// Print a slip to be inserted into a DirectOrder console
    /// </summary>
    PrintSlip,
    /// <summary>
    /// Transfers the order to the primary account
    /// </summary>
    SendToPrimary,
}
