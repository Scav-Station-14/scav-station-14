using System.Linq;
using Content.Shared._Scav._Shipyard;
using Content.Server._NF.Shipyard.Systems;
using Content.Shared._NF.Shipyard.BUI;
using Content.Shared._NF.Shipyard;
using Content.Shared._Scav.Shipyard;
using Content.Server._NF.Shipyard.Components;
using Content.Shared._NF.Bank.Components;
using Content.Shared._NF.Shipyard.Components;
using Content.Shared.UserInterface;
using Content.Server._NF.Station.Components;
using Content.Shared._NF.CCVar;
using Content.Shared._NF.Shipyard.Events;
using Content.Shared.GameTicking;
using Robust.Shared.Containers;
using Robust.Shared.Log;
using Content.Shared._Scav.Shipyard.Components;
using Content.Shared._Scav.Shipyard.BUI;
using Robust.Server.GameObjects;
using Content.Shared._Scav.Shipyard.Events;
using Content.Server.Database;
using static System.Net.Mime.MediaTypeNames;
using Content.Server.Popups;
using Robust.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Content.Shared.Access.Components;
using Content.Server.Station.Systems;
using static Content.Server._NF.Shipyard.Systems.ShipyardSystem;
using Content.Server.StationEvents.Components;
using Content.Shared.Shuttles.Components;
using Content.Shared.Pinpointer;
using Content.Shared.Station.Components;
using Content.Server.Shuttles.Systems;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Utility;
using Content.Server._NF.ShuttleRecords;
using Content.Server._Scav.Persistence;
using Content.Server.Access.Systems;
using Content.Server.Administration.Logs;
using Content.Server.Preferences.Managers;
using Content.Server.Shuttles.Components;
using Content.Shared.Database;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Preferences;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._Scav.Shipyard.Systems;
public sealed partial class GarageSystem : SharedGarageSystem
{
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly ShipyardSystem _shipyardSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly DockingSystem _docking = default!;
    [Dependency] private readonly ShuttleRecordsSystem _shuttleRecordsSystem = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IServerPreferencesManager _prefsManager = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly IdCardSystem _idSystem = default!;

    public List<ShipData> Ships = new List<ShipData>(); //local copy of the ships stored in the database. this is honestly probably the best way to handle this

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GarageConsoleComponent, BoundUIOpenedEvent>(OnConsoleUIOpened);
        SubscribeLocalEvent<GarageConsoleComponent, GarageConsoleStoreMessage>(OnStoreMessage);
        SubscribeLocalEvent<GarageConsoleComponent, GarageConsoleRetrieveMessage>(OnRetrieveMessage);
        SubscribeLocalEvent<GarageConsoleComponent, EntInsertedIntoContainerMessage>(OnItemSlotChanged);
        SubscribeLocalEvent<GarageConsoleComponent, EntRemovedFromContainerMessage>(OnItemSlotChanged);

        RefreshShips();
    }

    private async void RefreshShips()
    {
        Ships = await _db.GetShipData();
    }

    private void RefreshState(EntityUid uid, string? shipDeed, EntityUid? targetId, GarageConsoleUiKey uiKey, EntityUid player)
    {
        if (!TryGetCharacterData(player, out var userId, out var slot))
            return;

        var userShips = Ships.Where(s => s.ProfileData.Where(p => p.UserId == userId).Any(p => p.Slot == slot)).ToList();

        var newState = new GarageConsoleInterfaceState(
            shipDeed,
            targetId.HasValue,
            ((byte)uiKey),
            userShips);

        _ui.SetUiState(uid, uiKey, newState);
    }

    private void OnConsoleUIOpened(EntityUid uid, GarageConsoleComponent component, BoundUIOpenedEvent args)
    {
        if (!component.Initialized)
            return;

        // kind of cursed. We need to update the UI when an Id is entered, but the UI needs to know the player characters bank account.
        if (!TryComp<ActivatableUIComponent>(uid, out var uiComp) || uiComp.Key == null)
            return;

        if (args.Actor is not { Valid: true } player)
            return;

        //      mayhaps re-enable this later for HoS/SA
        //        var station = _station.GetOwningStation(uid);

        //if (!TryComp<BankAccountComponent>(player, out var bank))
        //    return;

        var targetId = component.TargetIdSlot.ContainerSlot?.ContainedEntity;

        if (TryComp<ShuttleDeedComponent>(targetId, out var deed))
        {
            if (Deleted(deed!.ShuttleUid))
            {
                RemComp<ShuttleDeedComponent>(targetId!.Value);
                return;
            }
        }

        var voucherUsed = HasComp<ShipyardVoucherComponent>(targetId);

        var fullName = deed != null ? ShipyardSystem.GetFullName(deed) : null;

        RefreshState(uid, fullName, targetId, (GarageConsoleUiKey)args.UiKey, player);
    }

    private void OnItemSlotChanged(EntityUid uid, GarageConsoleComponent component, ContainerModifiedMessage args)
    {
        if (!component.Initialized)
            return;

        if (args.Container.ID != component.TargetIdSlot.ID)
            return;

        // kind of cursed. We need to update the UI when an Id is entered, but the UI needs to know the player characters bank account.
        if (!TryComp<ActivatableUIComponent>(uid, out var uiComp) || uiComp.Key == null)
            return;

        var uiUsers = _ui.GetActors(uid, uiComp.Key);

        foreach (var user in uiUsers)
        {
            if (user is not { Valid: true } player)
                continue;

            //if (!TryComp<BankAccountComponent>(player, out var bank))
            //    continue;

            var targetId = component.TargetIdSlot.ContainerSlot?.ContainedEntity;

            if (TryComp<ShuttleDeedComponent>(targetId, out var deed))
            {
                if (Deleted(deed!.ShuttleUid))
                {
                    RemComp<ShuttleDeedComponent>(targetId!.Value);
                    continue;
                }
            }

            var fullName = deed != null ? ShipyardSystem.GetFullName(deed) : null;
            RefreshState(uid,
                fullName,
                targetId,
                (GarageConsoleUiKey)uiComp.Key,
                player);

        }
    }

    public void OnStoreMessage(EntityUid uid, GarageConsoleComponent component, GarageConsoleStoreMessage args)
    {
        if (args.Actor is not { Valid: true } player)
            return;

        if (component.TargetIdSlot.ContainerSlot?.ContainedEntity is not { Valid: true } targetId || !TryComp<IdCardComponent>(targetId, out var idCard))
        {
            ConsolePopup(player, Loc.GetString("shipyard-console-no-idcard"));
            PlayDenySound(player, uid, component);
            return;
        }

        if (!TryComp<ShuttleDeedComponent>(targetId, out var deed) || deed.ShuttleUid is not { Valid: true } shuttleUid)
        {
            ConsolePopup(player, Loc.GetString("shipyard-console-no-deed"));
            PlayDenySound(player, uid, component);
            return;
        }

        if (_station.GetOwningStation(uid) is not { Valid: true } stationUid)
        {
            ConsolePopup(player, Loc.GetString("shipyard-console-invalid-station"));
            PlayDenySound(player, uid, component);
            return;
        }

        //Shipyard version messes with station records at this point, TODO: research how that works and port it here

        var shuttleName = ToPrettyString(shuttleUid); // Grab the name before it gets 1984'd
        var shuttleNetEntity = _entityManager.GetNetEntity(shuttleUid); // same with the netEntity for shuttle records

        var disableSaleQuery = GetEntityQuery<ShipyardSellConditionComponent>();
        var xformQuery = GetEntityQuery<TransformComponent>();
        var disableSaleMsg = _shipyardSystem.FindDisableShipyardSaleObjects(shuttleUid, (ShipyardConsoleUiKey)args.UiKey, disableSaleQuery, xformQuery);
        if (disableSaleMsg != null)
        {
            ConsolePopup(player, Loc.GetString(disableSaleMsg));
            PlayDenySound(player, uid, component);
            return;
        }

        //Code formerly found in TrySellShuttle
        ShipyardSaleResult saleResult = new ShipyardSaleResult();
        saleResult = _shipyardSystem.PreSaleShuttleCheck(stationUid, shuttleUid, uid);
        if (saleResult.Error != ShipyardSaleError.Success)
        {
            switch (saleResult.Error)
            {
                case ShipyardSaleError.Undocked:
                    ConsolePopup(player, Loc.GetString("shipyard-console-sale-not-docked"));
                    break;
                case ShipyardSaleError.OrganicsAboard:
                    ConsolePopup(player, Loc.GetString("shipyard-console-sale-organic-aboard", ("name", saleResult.OrganicName ?? "Somebody")));
                    break;
                case ShipyardSaleError.InvalidShip:
                    ConsolePopup(player, Loc.GetString("shipyard-console-sale-invalid-ship"));
                    break;
                default:
                    ConsolePopup(player, Loc.GetString("shipyard-console-sale-unknown-reason", ("reason", saleResult.Error.ToString())));
                    break;
            }
            PlayDenySound(player, uid, component);
            return;
        }

        //if it would be able to sell (passed all checks), clean up the grid, save to file, and delete
        if (_station.GetOwningStation(shuttleUid) is { Valid: true } shuttleStationUid)
        {
            _station.DeleteStation(shuttleStationUid);
        }

        _shipyardSystem.CleanGrid(shuttleUid, uid);

        var name = deed.ShuttleName;
        var suffix = deed.ShuttleNameSuffix;


        if(!TryGetCharacterData(player, out var userId, out var slot)) //we dont really want to proceed if there isnt an actual user doing this, i assume
            return; //TODO: better exit handling here

        //remove any elements that shouldnt be serialized
        _docking.UndockDocks(shuttleUid); // TODO: introduce some kind of delay between this and saving the grid, as it stands the doors dont close fully and then get saved in that halfway state
        RemComp<LinkedLifecycleGridParentComponent>(shuttleUid);
        RemComp<IFFComponent>(shuttleUid);
        RemComp<NavMapComponent>(shuttleUid);
        RemComp<ShuttleDeedComponent>(shuttleUid);
        RemComp<StationMemberComponent>(shuttleUid);


        if (name != null && suffix != null)
        {
            Guid? existingShipId = null;
            if (TryComp<ShuttlePersistenceTrackerComponent>(shuttleUid, out var persistence))
            {
                existingShipId = persistence.ShipId;
                RemComp<ShuttlePersistenceTrackerComponent>(shuttleUid); //Because Guids dont serialize, if we dont remove this component it will fail to save the file
            }

            var filepath = "/ships/" + name + suffix + ".yml";

            var saveResult = _mapLoader.TrySaveGrid(shuttleUid, new ResPath(filepath));
            if (!saveResult)
            {
                ConsolePopup(player, Loc.GetString("shipyard-console-sale-invalid-ship")); //suffice it to say, if this happens, thats catastrophically bad, we've already deleted a bunch of stuff...
                _adminLogger.Add(LogType.ShipYardUsage, LogImpact.High, $"{ToPrettyString(player):actor} failed to save shuttle grid {ToPrettyString(shuttleUid)} via {ToPrettyString(uid)}. Admin intervention is likely necessary.");
                PlayDenySound(player, uid, component);
                return;
            }

            //Database save
            if (existingShipId != null)
            {
                var i = Ships.FindIndex(s => s.ShipId == existingShipId.Value);
                Ships[i] = new ShipData {ShipId = existingShipId.Value, ShipName = name, ShipNameSuffix = suffix, FilePath = filepath, ProfileData = new List<ProfileIdentifier> {new ProfileIdentifier {UserId = userId!.Value.UserId, Slot = slot!.Value}}};
                //TODO: update database here
            }
            else
            {
                Guid newShipId = Guid.NewGuid();
                Ships.Add(new ShipData {ShipId = newShipId, ShipName = name, ShipNameSuffix = suffix, FilePath = filepath, ProfileData = new List<ProfileIdentifier> {new ProfileIdentifier {UserId = userId!.Value.UserId, Slot = slot!.Value}}});
                _db.RegisterShip(newShipId, name, suffix, userId!.Value, filepath, null);
            }
        }
        else
        {
            ConsolePopup(player, Loc.GetString("shipyard-console-no-deed"));
            PlayDenySound(player, uid, component);
            return;
        }

        QueueDel(shuttleUid);

        // Update shuttle records
        _shuttleRecordsSystem.RefreshStateForAll(true);
        _shuttleRecordsSystem.TrySetSaleTime(shuttleNetEntity);

        RemComp<ShuttleDeedComponent>(targetId);

        PlayConfirmSound(player, uid, component);

        //TODO: Send sell message

        _adminLogger.Add(LogType.ShipYardUsage, LogImpact.Low, $"{ToPrettyString(player):actor} used {ToPrettyString(targetId)} to return {shuttleName} to the garage via {ToPrettyString(uid)}");

        RefreshState(uid, null, targetId, (GarageConsoleUiKey)args.UiKey, player);
    }

    public void OnRetrieveMessage(EntityUid uid, GarageConsoleComponent component, GarageConsoleRetrieveMessage args)
    {
        if (args.Actor is not { Valid: true } player)
            return;

        if(!TryGetCharacterData(player, out var userId, out var slot)) //we dont really want to proceed if there isnt an actual user doing this, i assume
            return;

        var requestedShip = Ships.SingleOrDefault(s => s.ShipId == args.ShipId);

        if (component.TargetIdSlot.ContainerSlot?.ContainedEntity is not { Valid: true } targetId || !TryComp<IdCardComponent>(targetId, out var idCard))
        {
            ConsolePopup(player, Loc.GetString("shipyard-console-no-idcard"));
            PlayDenySound(player, uid, component);
            return;
        }

        if (HasComp<ShuttleDeedComponent>(targetId))
        {
            ConsolePopup(player, Loc.GetString("shipyard-console-already-deeded"));
            PlayDenySound(player, uid, component);
            return;
        }

        //do we need an access reader component check? probably was there for nfsd stuff

        if (requestedShip == null || String.IsNullOrEmpty(requestedShip.FilePath) || requestedShip.ShipId == Guid.Empty)
        {
            ConsolePopup(player, Loc.GetString("shipyard-console-invalid-vessel")); //TODO: needs different message
            _adminLogger.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(player):player} tried to retrieve a ship that does not exist.");
            PlayDenySound(player, uid, component);
            return;
        }

        if (!requestedShip!.ProfileData.Where(p => p.UserId == userId).Any(p => p.Slot == slot))
        {
            ConsolePopup(player, Loc.GetString("comms-console-permission-denied"));
            _adminLogger.Add(LogType.ShipYardUsage, LogImpact.Medium, $"{ToPrettyString(player):player} attempted to retrieve ship with Id {args.ShipId} via {ToPrettyString(uid)}, but they do not own that ship");
            PlayDenySound(player, uid, component);
            return;
        }

        if (_station.GetOwningStation(uid) is not { Valid: true } station)
        {
            ConsolePopup(player, Loc.GetString("shipyard-console-invalid-station"));
            PlayDenySound(player, uid, component);
            return;
        }

        //Spawn the shuttle from the filepath
        if (!_shipyardSystem.TryPurchaseShuttle(station, new ResPath(requestedShip.FilePath), out var shuttleUidOut))
        {
            PlayDenySound(player, uid, component);
            return;
        }

        var shuttleUid = shuttleUidOut.Value;
        if (!TryComp<ShuttleComponent>(shuttleUid, out var shuttle))
        {
            PlayDenySound(player, uid, component);
            return;
        }

        //TODO: rework of the latejoin code, the existing solution depends on an existing prototype

        var shuttlePersistenceTracker = EnsureComp<ShuttlePersistenceTrackerComponent>(shuttleUid);
        shuttlePersistenceTracker.ShipId = requestedShip.ShipId;

        var deedID = EnsureComp<ShuttleDeedComponent>(targetId);

        var fullName = requestedShip.ShipName + " " + requestedShip.ShipNameSuffix;

        var shuttleOwner = Name(player).Trim();
        _shipyardSystem.AssignShuttleDeedProperties((targetId, deedID), shuttleUid, fullName, shuttleOwner, false);

        var deedShuttle = EnsureComp<ShuttleDeedComponent>(shuttleUid);
        _shipyardSystem.AssignShuttleDeedProperties((shuttleUid, deedShuttle), shuttleUid, fullName, shuttleOwner, false);

        if (component.NewJobTitle != null)
        {
            _idSystem.TryChangeJobTitle(targetId, Loc.GetString(component.NewJobTitle), idCard, player);
        }

        //TODO: implement station records stuff

        EnsureComp<LinkedLifecycleGridParentComponent>(shuttleUid);

        //TODO: send purchase message
        PlayConfirmSound(player, uid, component);
        _adminLogger.Add(LogType.ShipYardUsage, LogImpact.Low, $"{ToPrettyString(player):actor} used {ToPrettyString(targetId)} to retrieve ship {ToPrettyString(shuttleUid)} from garage via {ToPrettyString(uid)}");

        //TODO: shuttle records code

        RefreshState(uid, fullName, targetId, (GarageConsoleUiKey)args.UiKey, player);
    }

    public async void GetShipsFromDb(NetUserId userId)
    {
        var ships = _db.GetShipsByUser(userId);
    }

    private void ConsolePopup(EntityUid uid, string text)
    {
        _popup.PopupEntity(text, uid);
    }
    private void PlayDenySound(EntityUid playerUid, EntityUid consoleUid, GarageConsoleComponent component)
    {
        if (_timing.CurTime >= component.NextDenySoundTime)
        {
            component.NextDenySoundTime = _timing.CurTime + component.DenySoundDelay;
            _audio.PlayPvs(_audio.ResolveSound(component.ErrorSound), consoleUid);
        }
    }

    private void PlayConfirmSound(EntityUid playerUid, EntityUid consoleUid, GarageConsoleComponent component)
    {
        _audio.PlayEntity(component.ConfirmSound, playerUid, consoleUid);
    }

    private bool TryGetNetUserIdOfPlayerUid(EntityUid uid, out NetUserId? userId)
    {
        if (!TryComp<MindContainerComponent>(uid, out var mindContainer) ||
            !TryComp<MindComponent>(mindContainer.Mind, out var mind) ||
            mind.UserId == null)
        {
            userId = null;
            return false;
        }

        userId = mind.UserId.Value;
        return true;
    }

    private bool TryGetCharacterData(EntityUid uid, out NetUserId? userId, out int? slot)
    {
        userId = null;
        slot = null;

        if (!_playerManager.TryGetSessionByEntity(uid, out var session))
        {
            //_log.Info($"TryBankDeposit: {mobUid} has no attached session");
            return false;
        }
        if (!_prefsManager.TryGetCachedPreferences(session.UserId, out var prefs))
        {
            //_log.Info($"TryBankDeposit: {mobUid} has no cached prefs");
            return false;
        }
        if (prefs.SelectedCharacter is not HumanoidCharacterProfile profile)
        {
            //_log.Info($"TryBankDeposit: {mobUid} has the wrong prefs type");
            return false;
        }

        userId = session.UserId;
        slot = prefs.SelectedCharacterIndex;
        return true;
    }
}
