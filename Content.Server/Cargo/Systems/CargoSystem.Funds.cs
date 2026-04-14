using System.Linq;
using Content.Shared._Scav.Cargo; // Scav
using Content.Shared._Scav.Cargo.Components; // Scav
using Content.Shared.Cargo.Components;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.Emag.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Stacks; // Scav
using Content.Shared.UserInterface;
using Robust.Shared.Containers; // Scav

namespace Content.Server.Cargo.Systems;

public sealed partial class CargoSystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;

    private bool _allowPrimaryAccountAllocation;
    private bool _allowPrimaryCutAdjustment;

    public void InitializeFunds()
    {
        // Scav: Changed console components to FundManagementConsoleComponent, added additional subscriptions
        SubscribeLocalEvent<FundManagementConsoleComponent, FundManagementConsoleWithdrawFundsMessage>(OnWithdrawFunds);
        SubscribeLocalEvent<FundManagementConsoleComponent, FundManagementConsoleDepositFundsMessage>(OnDepositFunds);
        SubscribeLocalEvent<FundManagementConsoleComponent, FundManagementConsoleUpdateSelectionMessage>(OnUpdateSelection);
        SubscribeLocalEvent<FundManagementConsoleComponent, SetFundingAllocationBuiMessage>(OnSetFundingAllocation);
        SubscribeLocalEvent<FundManagementConsoleComponent, BeforeActivatableUIOpenEvent>(OnFundAllocationBuiOpen);
        SubscribeLocalEvent<FundManagementConsoleComponent, EntInsertedIntoContainerMessage>(OnCashSlotChanged);
        SubscribeLocalEvent<FundManagementConsoleComponent, EntRemovedFromContainerMessage>(OnCashSlotChanged);
        // End Scav

        _cfg.OnValueChanged(CCVars.AllowPrimaryAccountAllocation, enabled => { _allowPrimaryAccountAllocation = enabled; }, true);
        _cfg.OnValueChanged(CCVars.AllowPrimaryCutAdjustment, enabled => { _allowPrimaryCutAdjustment = enabled; }, true);
    }

    private void OnWithdrawFunds(Entity<FundManagementConsoleComponent> ent, ref FundManagementConsoleWithdrawFundsMessage args)
    {
        if (_station.GetOwningStation(ent) is not { } station ||
            !TryComp<StationBankAccountComponent>(station, out var bank))
            return;

        // Scav: Account is no longer stored on console component, source and destination in params
        if (args.SourceAccount is null)
            return;
        var sourceAccount = args.SourceAccount!.Value;

        if (args.Amount <= 0 ||
            args.Amount > GetBalanceFromAccount((station, bank), sourceAccount)) //Scav: removed transfer limit
            return;

        if (Timing.CurTime < ent.Comp.NextAccountActionTime)
            return;

        if (!_accessReaderSystem.IsAllowed(args.Actor, ent))
        {
            ConsolePopup(args.Actor, Loc.GetString("cargo-console-order-not-allowed"));
            PlayDenySound(ent, ent.Comp);
            return;
        }

        // Scav: additional validation
        if (!_protoMan.TryIndex(sourceAccount, out var sourceAccountProto) ||
            (args.DestinationAccount is not null && !_protoMan.TryIndex(args.DestinationAccount.Value, out _)))
        {
            PlayDenySound(ent, ent.Comp);
            return;
        }
        // End Scav

        ent.Comp.NextAccountActionTime = Timing.CurTime + ent.Comp.AccountActionDelay;
        UpdateBankAccount((station, bank), -args.Amount,  sourceAccount, dirty: true); //Scav: now dirties
        _audio.PlayPvs(ApproveSound, ent);

        var tryGetIdentityShortInfoEvent = new TryGetIdentityShortInfoEvent(ent, args.Actor);
        RaiseLocalEvent(tryGetIdentityShortInfoEvent);

        // Scav: significant rewrites to support new args
        if (args.DestinationAccount == null)
        {
            var stackPrototype = _protoMan.Index(ent.Comp.CashType);
            _stack.Spawn(args.Amount, stackPrototype, Transform(ent).Coordinates);

            var msg = Loc.GetString("cargo-console-fund-withdraw-broadcast",
                ("name", tryGetIdentityShortInfoEvent.Title ?? Loc.GetString("cargo-console-fund-transfer-user-unknown")),
                ("amount", args.Amount),
                ("name1", Loc.GetString(sourceAccountProto.Name)),
                ("code1", Loc.GetString(sourceAccountProto.Code)));
            _radio.SendRadioMessage(ent, msg, sourceAccountProto.RadioChannel, ent, escapeMarkup: false);
        }
        else
        {
            var otherAccount = args.DestinationAccount!.Value;
            var otherAccountProto = _protoMan.Index(otherAccount);
            UpdateBankAccount((station, bank), args.Amount, otherAccount, dirty: true);

            var msg = Loc.GetString("cargo-console-fund-transfer-broadcast",
                ("name", tryGetIdentityShortInfoEvent.Title ?? Loc.GetString("cargo-console-fund-transfer-user-unknown")),
                ("amount", args.Amount),
                ("name1", Loc.GetString(sourceAccountProto.Name)),
                ("code1", Loc.GetString(sourceAccountProto.Code)),
                ("name2", Loc.GetString(otherAccountProto.Name)),
                ("code2", Loc.GetString(otherAccountProto.Code)));
            _radio.SendRadioMessage(ent, msg, sourceAccountProto.RadioChannel, ent, escapeMarkup: false);
            _radio.SendRadioMessage(ent, msg, otherAccountProto.RadioChannel, ent, escapeMarkup: false);
        }
        // End Scav

        UpdateUi(ent); //Scav
    }

    // Scav: Added additional functions to support fund transfer, deposit, etc functionality.
    private void OnUpdateSelection(Entity<FundManagementConsoleComponent> ent, ref FundManagementConsoleUpdateSelectionMessage args)
    {
        if (args.Account != null && ent.Comp.SelectedAccount != args.Account.Value)
        {
            ent.Comp.SelectedAccount = args.Account.Value;
            UpdateUi(ent);
        }
    }

    private void UpdateUi(Entity<FundManagementConsoleComponent> ent)
    {
        if (_station.GetOwningStation(ent) is not { } station)
            return;

        GetInsertedCashAmount(ent.Comp, out var amount);

        _uiSystem.SetUiState(ent.Owner, FundManagementConsoleUiKey.Key, new FundManagementConsoleBuiState(GetNetEntity(station), ent.Comp.SelectedAccount, amount, ent.Comp.NextAccountActionTime));
    }

    private void OnDepositFunds(Entity<FundManagementConsoleComponent> ent, ref FundManagementConsoleDepositFundsMessage args)
    {
        if (_station.GetOwningStation(ent) is not { } station ||
            !TryComp<StationBankAccountComponent>(station, out var bank))
            return;

        if (args.Account is null || !_protoMan.TryIndex(args.Account!.Value, out var sourceAccountProto))
        {
            PlayDenySound(ent, ent.Comp);
            return;
        }

        if (Timing.CurTime < ent.Comp.NextAccountActionTime)
            return;

        if (!_accessReaderSystem.IsAllowed(args.Actor, ent))
        {
            ConsolePopup(args.Actor, Loc.GetString("cargo-console-order-not-allowed"));
            PlayDenySound(ent, ent.Comp);
            return;
        }

        GetInsertedCashAmount(ent.Comp, out var deposit);

        if (deposit <= 0)
        {
            return;
        }

        ent.Comp.NextAccountActionTime = Timing.CurTime + ent.Comp.AccountActionDelay;
        UpdateBankAccount((station, bank), deposit, args.Account!.Value, dirty: true);
        _audio.PlayPvs(ApproveSound, ent);

        SetInsertedCashAmount(ent.Comp, deposit);

        UpdateUi(ent);
    }

    private void GetInsertedCashAmount(FundManagementConsoleComponent component, out int amount)
    {
        amount = 0;
        var cashEntity = component.CashSlot.ContainerSlot?.ContainedEntity;

        // Nothing inserted: amount should be 0.
        if (cashEntity == null)
            return;

        // Invalid item inserted (doubloons, FUC, telecrystals...): amount should be negative (to denote an error)
        if (!TryComp<StackComponent>(cashEntity, out var cashStack) ||
            cashStack.StackTypeId != component.CashType)
        {
            amount = -1;
            return;
        }

        // Valid amount: output the stack's value.
        amount = cashStack.Count;
    }

    private void SetInsertedCashAmount(FundManagementConsoleComponent component, int amount)
    {
        var empty = false;
        var cashEntity = component.CashSlot.ContainerSlot?.ContainedEntity;

        if (!TryComp<StackComponent>(cashEntity, out var cashStack) ||
            cashStack.StackTypeId != component.CashType)
        {
            return;
        }

        int newAmount = cashStack.Count;
        cashStack.Count = newAmount - amount;

        if (cashStack.Count <= 0)
            empty = true;

        if (empty)
            _containerSystem.CleanContainer(component.CashSlot.ContainerSlot!);
    }

    private void OnCashSlotChanged(EntityUid uid, FundManagementConsoleComponent component, ContainerModifiedMessage args)
    {
        UpdateUi((uid, component));
    }
    // End Scav

    // Scav: Removed OnToggleLimit
    /*
    private void OnToggleLimit(Entity<CargoOrderConsoleComponent> ent, ref CargoConsoleToggleLimitMessage args)
    {
        if (!_accessReaderSystem.FindAccessTags(args.Actor).Intersect(ent.Comp.RemoveLimitAccess).Any())
        {
            ConsolePopup(args.Actor, Loc.GetString("cargo-console-order-not-allowed"));
            PlayDenySound(ent, ent.Comp);
            return;
        }

        _audio.PlayPvs(ent.Comp.ToggleLimitSound, ent);
        ent.Comp.TransferUnbounded = !ent.Comp.TransferUnbounded;
        Dirty(ent);
    }
    */

    private void OnSetFundingAllocation(Entity<FundManagementConsoleComponent> ent, ref SetFundingAllocationBuiMessage args)
    {
        if (_station.GetOwningStation(ent) is not { } station ||
            !TryComp<StationBankAccountComponent>(station, out var bank))
            return;

        var expectedCount = _allowPrimaryAccountAllocation ? bank.RevenueDistribution.Count : bank.RevenueDistribution.Count - 1;
        if (args.Percents.Count != expectedCount)
            return;

        var differs = false;
        foreach (var (account, percent) in args.Percents)
        {
            if (percent != (int) Math.Round(bank.RevenueDistribution[account] * 100))
            {
                differs = true;
                break;
            }
        }
        differs = differs || args.PrimaryCut != bank.PrimaryCut || args.LockboxCut != bank.LockboxCut;

        if (!differs)
            return;

        if (args.Percents.Values.Sum() != 100)
            return;

        var primaryCut = bank.RevenueDistribution[bank.PrimaryAccount];
        bank.RevenueDistribution.Clear();
        foreach (var (account, percent )in args.Percents)
        {
            bank.RevenueDistribution.Add(account, percent / 100.0);
        }
        if (!_allowPrimaryAccountAllocation)
        {
            bank.RevenueDistribution.Add(bank.PrimaryAccount, 0);
        }

        if (_allowPrimaryCutAdjustment && args.PrimaryCut is >= 0.0 and <= 1.0)
        {
            bank.PrimaryCut = args.PrimaryCut;
        }
        if (_lockboxCutEnabled && args.LockboxCut is >= 0.0 and <= 1.0)
        {
            bank.LockboxCut = args.LockboxCut;
        }

        Dirty(station, bank);

        _audio.PlayPvs(ent.Comp.SetDistributionSound, ent);
        _adminLogger.Add(
            LogType.Action,
            LogImpact.Medium,
            $"{ToPrettyString(args.Actor):player} set station {ToPrettyString(station)} fund distribution: {string.Join(',', bank.RevenueDistribution.Select(p => $"{p.Key}: {p.Value}").ToList())}, primary cut: {bank.PrimaryCut}, lockbox cut: {bank.LockboxCut}");
    }

    private void OnFundAllocationBuiOpen(Entity<FundManagementConsoleComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateUi(ent); //Scav: moved logic to UpdateUi helper function
    }
}
