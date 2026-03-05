using Content.Client._Scav.Cargo.UI;
using Content.Shared._Scav.Cargo;
using Content.Shared.Cargo.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Scav.Cargo.BUI;

[UsedImplicitly]
public sealed class FundManagementConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private FundManagementConsoleMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<FundManagementConsoleMenu>();

        _menu.OnAllocationSave += (dicts, primary, lockbox) =>
        {
            SendMessage(new SetFundingAllocationBuiMessage(dicts, primary, lockbox));
        };
        _menu.OnWithdraw += (account, amount, otherAccount) =>
        {
            SendMessage(new FundManagementConsoleWithdrawFundsMessage(account, amount, otherAccount));
        };
        _menu.OnDeposit += account =>
        {
            SendMessage(new FundManagementConsoleDepositFundsMessage(account));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState message)
    {
        base.UpdateState(message);

        if (message is not FundManagementConsoleBuiState state)
            return;

        _menu?.Update(state);
    }
}
