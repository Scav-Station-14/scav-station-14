using Content.Server._NF.BindToStation;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.VariationPass;
using Content.Server.GameTicking.Rules.VariationPass.Components;
using Content.Shared._NF.BindToStation;
using Content.Shared.CCVar;// Scav
using Robust.Shared.Configuration;// Scav

namespace Content.Server._NF.GameTicking.Rules.VariationPass;

public sealed class BindToStationVariationPass : VariationPassSystem<BindToStationVariationPassComponent>
{
    [Dependency] BindToStationSystem _bindToStation = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!; // Scav
    protected override void ApplyVariation(Entity<BindToStationVariationPassComponent> ent, ref StationVariationPassEvent args)
    {
        // Exempt station?  Don't apply this variation.
        if (HasComp<BindToStationVariationPassExemptionComponent>(args.Station))
            return;

        // Tie vendors to a particular station.
        var vendorQuery = AllEntityQuery<BindToStationComponent, TransformComponent>();
        while (vendorQuery.MoveNext(out var uid, out var bind, out var xform))
        { // Scav
            if(_cfg.GetCVar(CCVars.DisableStationBinding))
                _bindToStation.BindToStation(uid, null, false);

            if (!bind.Enabled || !IsMemberOfStation((uid, xform), ref args) || _cfg.GetCVar(CCVars.DisableStationBinding))
                continue; // Scav end

            _bindToStation.BindToStation(uid, args.Station);
        }
    }
}
