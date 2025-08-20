using Content.Client._NF.Shipyard.UI;
using Content.Shared.Containers.ItemSlots;
using Content.Shared._NF.Shipyard.BUI;
using Content.Shared._NF.Shipyard.Events;
using static Robust.Client.UserInterface.Controls.BaseButton;
using Robust.Client.UserInterface;
using Content.Client._Scav.Shipyard.UI;
using Content.Shared._Scav._Shipyard;
using Content.Shared._Scav.Shipyard.BUI;
using Content.Shared._Scav.Shipyard.Events;

namespace Content.Client._Scav.Shipyard.BUI;

public sealed class GarageConsoleBoundUserInterface : BoundUserInterface
{
    private GarageConsoleMenu? _menu;
    // private ShipyardRulesPopup? _rulesWindow; // Frontier
    public int Balance { get; private set; }

    public int? ShipSellValue { get; private set; }

    public GarageConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        if (_menu == null)
        {
            _menu = this.CreateWindow<GarageConsoleMenu>();
            _menu.OnShipRetrieved += RetrieveShip;
            _menu.OnStoreShip += StoreShip;
            _menu.TargetIdButton.OnPressed += _ => SendMessage(new ItemSlotButtonPressedEvent("ShipyardConsole-targetId"));

            // Disable the NFSD popup for now.
            // var rules = new FormattedMessage();
            // _rulesWindow = new ShipyardRulesPopup(this);
            // if (ShipyardConsoleUiKey.Security == (ShipyardConsoleUiKey) UiKey)
            // {
            //     rules.AddText(Loc.GetString($"shipyard-rules-default1"));
            //     rules.PushNewline();
            //     rules.AddText(Loc.GetString($"shipyard-rules-default2"));
            //     _rulesWindow.ShipRules.SetMessage(rules);
            //     _rulesWindow.OpenCentered();
            // }
        }
    }

    private void Populate(List<ShipData> ships)
    {
        if (_menu == null)
            return;

        _menu.PopulateShips(ships);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not GarageConsoleInterfaceState cState)
            return;

        var castState = (GarageConsoleInterfaceState)state;
        Populate(castState.Ships);
        _menu?.UpdateState(castState);
    }


    private void RetrieveShip(ButtonEventArgs args)
    {
        if (args.Button.Parent?.Parent is not ShipRow row)
        {
            return;
        }

        var shipId = row.ShipId;
        SendMessage(new GarageConsoleRetrieveMessage(shipId));
    }


    private void StoreShip(ButtonEventArgs args)
    {
        //reserved for a sanity check, but im not sure what since we check all the important stuffs on server already
        SendMessage(new GarageConsoleStoreMessage());
    }
}
