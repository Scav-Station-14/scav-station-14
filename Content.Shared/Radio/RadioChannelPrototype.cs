using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Radio;

[Prototype]
public sealed partial class RadioChannelPrototype : IPrototype
{
    /// <summary>
    /// Human-readable name for the channel.
    /// </summary>
    [DataField("name")]
    public LocId Name { get; private set; } = string.Empty;

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedName => Loc.GetString(Name);

    /// <summary>
    /// Single-character prefix to determine what channel a message should be sent to.
    /// </summary>
    [DataField("keycode")]
    public char KeyCode { get; private set; } = '\0';

    [DataField("frequency")]
    public int Frequency { get; private set; } = 0;

    [DataField("color")]
    public Color Color { get; private set; } = Color.Lime;

    [IdDataField, ViewVariables]
    public string ID { get; private set; } = default!;

    // Scav: Replaced LongRange bool with enum
    /// <summary>
    /// Determines whether or not this channel requires a telecommunication server
    /// </summary>
    [DataField("range"), ViewVariables]
    public ChannelRange Range = ChannelRange.ShortRange;

    // Frontier: radio channel frequencies
    /// <summary>
    /// If true, the frequency of the message being sent will be appended to the chat message
    /// </summary>
    [DataField, ViewVariables]
    public bool ShowFrequency = false;
    // End Frontier
}

// Scav: radio range enum, rather than bool
/// <summary>
/// Defines the range and behavior of a radio channel
/// </summary>
[Serializable, NetSerializable]
public enum ChannelRange : byte
{
    /// <summary>
    /// Only accessible to users within a physical distance from the radio server
    /// </summary>
    ShortRange,
    /// <summary>
    /// Available across the entire map, but requires at least one server providing the channel to exist somewhere on that map
    /// </summary>
    LongRange,
    /// <summary>
    /// Available everywhere with no restrictions
    /// </summary>
    Global
}
// End scav
