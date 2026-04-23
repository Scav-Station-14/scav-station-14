using Content.Server.GameTicking.Presets;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Utility;

namespace Content.Server._Scav.GameRule;

/// <summary>
/// Describes information for a station map. Works similarly to Frontier's POIs but dependent on database records instead of spawn chances.
/// </summary>
[Prototype]
[Serializable]
public sealed partial class PlayerStationPrototype : IPrototype, IInheritingPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<PlayerStationPrototype>))]
    public string[]? Parents { get; private set; }

    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }

    /// <summary>
    /// The name of this point of interest.
    /// </summary>
    [DataField(required: true)]
    public string Name { get; private set; } = "";

    /// <summary>
    /// Should we set the warppoint name based on the grid name.
    /// </summary>
    [DataField]
    public bool NameWarp { get; set; } = true;

    /// <summary>
    /// If true, makes the warp point admin-only (hiding it for players).
    /// </summary>
    [DataField]
    public bool HideWarp { get; set; } = false;

    /// <summary>
    /// Minimum range to spawn this POI at.
    /// </summary>
    [DataField]
    public int MinimumDistance { get; private set; } = 5000;

    /// <summary>
    /// Maximum range to spawn this POI at.
    /// </summary>
    [DataField]
    public int MaximumDistance { get; private set; } = 10000;

    /// <summary>
    /// Maximum clearance between this POI and others.
    /// Measured between the origins of the respective grids.
    /// </summary>
    [DataField]
    public int MinimumClearance { get; private set; } = 400;

    /// <summary>
    /// Components to be added to any spawned grids.
    /// </summary>
    [DataField]
    [AlwaysPushInheritance]
    public ComponentRegistry AddComponents { get; set; } = new();

    /// <summary>
    /// What gamepresets ID this POI is allowed to spawn on.
    /// If left empty, all presets are allowed.
    /// </summary>
    [DataField]
    public ProtoId<GamePresetPrototype>[] SpawnGamePreset { get; private set; } = [];

    /// <summary>
    /// The path to the grid.
    /// </summary>
    [DataField(required: true)]
    public ResPath GridPath { get; private set; } = default!;
}
