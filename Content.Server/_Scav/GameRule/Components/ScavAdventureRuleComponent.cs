namespace Content.Server._Scav.GameRule.Components;

[RegisterComponent, Access(typeof(ScavAdventureRuleSystem))]
public sealed partial class ScavAdventureRuleComponent : Component
{
    public Dictionary<int, EntityUid> Stations = new();
    public List<EntityUid> NFPlayerMinds = new();
    public List<EntityUid> CargoDepots = new();
    public List<EntityUid> MarketStations = new();
    public List<EntityUid> RequiredPois = new();
    public List<EntityUid> OptionalPois = new();
    public List<EntityUid> UniquePois = new();
}
