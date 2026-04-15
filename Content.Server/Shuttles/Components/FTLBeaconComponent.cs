using System.Numerics;

namespace Content.Server.Shuttles.Components;

/// <summary>
/// Shows up on a shuttle's map as an FTL target.
/// </summary>
[RegisterComponent]
public sealed partial class FTLBeaconComponent : Component
{
    /// <summary>
    /// Coords to use when FTLing to a destination. Used to ensure that landing is outside of a dungeon, for example.
    /// </summary>
    [DataField]
    public Vector2 Coords = Vector2.Zero;

    /// <summary>
    /// Angle rotation used to apply the shuttle offset relative to.
    /// </summary>
    [DataField]
    public Angle Rotation = Angle.Zero;
}
