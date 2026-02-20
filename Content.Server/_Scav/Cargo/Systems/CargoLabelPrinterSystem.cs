using Content.Server._Scav.Cargo.Components;
using Content.Server.Station.Systems;
using Content.Shared._Scav.Cargo.Components;
using Content.Shared.Interaction;
using Content.Shared.Paper;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._Scav.Cargo.Systems;

public sealed class CargoLabelPrinterSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PaperSystem _paperSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CargoLabelPrinterComponent, InteractHandEvent>(OnPrintLabel);
    }


    private void OnPrintLabel(EntityUid uid, CargoLabelPrinterComponent component, InteractHandEvent args)
    {
        if (!_interactionSystem.InRangeUnobstructed(args.User, args.Target))
            return;

        if (_timing.CurTime < component.NextPrintTime)
            return;

        if (_station.GetOwningStation(uid) is not { } station)
            return;

        var label = Spawn(component.LabelId, Transform(uid).Coordinates);
        component.NextPrintTime = _timing.CurTime + component.PrintDelay;
        SetupLabel(label, station);
        _audio.PlayPvs(component.PrintSound, uid);
    }

    private void SetupLabel(EntityUid uid,
        EntityUid stationId,
        PaperComponent? paper = null,
        CargoLabelComponent? label = null)
    {
        if (!Resolve(uid, ref paper, ref label))
            return;

        label.AssociatedStationId = stationId;
        var msg = new FormattedMessage();
        msg.AddText(Loc.GetString("salvage-tag-header"));
        msg.PushNewline();
        //msg.AddText(Loc.GetString("bounty-manifest-list-start"));
        _paperSystem.SetContent((uid, paper), msg.ToMarkup());
    }
}
