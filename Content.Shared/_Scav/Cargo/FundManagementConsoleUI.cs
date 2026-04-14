using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Scav.Cargo;

[Serializable, NetSerializable]
public sealed class FundManagementConsoleWithdrawFundsMessage : BoundUserInterfaceMessage
{
    public ProtoId<CargoAccountPrototype>? SourceAccount;
    public int Amount;
    public ProtoId<CargoAccountPrototype>? DestinationAccount;


    public FundManagementConsoleWithdrawFundsMessage(ProtoId<CargoAccountPrototype>? account, int amount, ProtoId<CargoAccountPrototype>? otherAccount)
    {
        SourceAccount = account;
        Amount = amount;
        DestinationAccount = otherAccount;
    }
}

[Serializable, NetSerializable]
public sealed class FundManagementConsoleDepositFundsMessage : BoundUserInterfaceMessage
{
    public ProtoId<CargoAccountPrototype>? Account;

    public FundManagementConsoleDepositFundsMessage(ProtoId<CargoAccountPrototype>? account)
    {
        Account = account;
    }
}


[Serializable, NetSerializable]
public sealed class FundManagementConsoleBuiState : BoundUserInterfaceState
{
    public NetEntity Station;
    public ProtoId<CargoAccountPrototype> SelectedAccount;
    public int DepositAmount;
    public TimeSpan NextAccountActionTime;

    public FundManagementConsoleBuiState(NetEntity station, ProtoId<CargoAccountPrototype> selectedAccount, int depositAmount, TimeSpan nextAccountActionTime)
    {
        Station = station;
        SelectedAccount = selectedAccount;
        DepositAmount = depositAmount;
        NextAccountActionTime = nextAccountActionTime;
    }
}

[Serializable, NetSerializable]
public sealed class FundManagementConsoleUpdateSelectionMessage : BoundUserInterfaceMessage
{
    public ProtoId<CargoAccountPrototype>? Account;

    public FundManagementConsoleUpdateSelectionMessage(ProtoId<CargoAccountPrototype>? account)
    {
        Account = account;
    }
}


[Serializable, NetSerializable]
public enum FundManagementConsoleUiKey : byte
{
    Key
}
