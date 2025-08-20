using Robust.Shared.Serialization;

namespace Content.Shared._Scav._Shipyard;

/// <summary>
///     Contains data about a ship, to be stored when not in the database
///     Based loosely on the implementation of PlayerPreferences
/// </summary>
[Serializable]
[NetSerializable]
public sealed class ShipData
{
    public int Id { get; set; }
    public string ShipName { get; set; } = null!;
    public string ShipNameSuffix { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public List<ProfileIdentifier> ProfileData { get; set; } = new List<ProfileIdentifier>();
}

/// <summary>
///     Profiles seem to never be actually addressed by ID, but rather by a tuple of user id and slot. This object makes addressing that in a database query easier
/// </summary>
[Serializable]
[NetSerializable]
public sealed class ProfileIdentifier
{
    public Guid UserId { get; set; }
    public int Slot { get; set; }
}
