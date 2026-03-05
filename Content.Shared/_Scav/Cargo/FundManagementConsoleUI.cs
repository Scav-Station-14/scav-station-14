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

    public FundManagementConsoleBuiState(NetEntity station)
    {
        Station = station;
    }
}


[Serializable, NetSerializable]
public enum FundManagementConsoleUiKey : byte
{
    Key
}
