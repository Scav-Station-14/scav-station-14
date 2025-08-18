using Content.Shared.Containers.ItemSlots;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared._Scav.Shipyard.Components;

namespace Content.Shared._Scav.Shipyard;

[Serializable, NetSerializable]
public enum GarageConsoleUiKey : byte
{
    Key
}
public abstract class SharedGarageSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GarageConsoleComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<GarageConsoleComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<GarageConsoleComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<GarageConsoleComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, GarageConsoleComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not GarageConsoleComponentState state) return;

    }

    private void OnGetState(EntityUid uid, GarageConsoleComponent component, ref ComponentGetState args)
    {

    }

    private void OnComponentInit(EntityUid uid, GarageConsoleComponent component, ComponentInit args)
    {
        _itemSlotsSystem.AddItemSlot(uid, GarageConsoleComponent.TargetIdCardSlotId, component.TargetIdSlot);
    }

    private void OnComponentRemove(EntityUid uid, GarageConsoleComponent component, ComponentRemove args)
    {
        _itemSlotsSystem.RemoveItemSlot(uid, component.TargetIdSlot);
    }

    [Serializable, NetSerializable]
    private sealed class GarageConsoleComponentState : ComponentState
    {
        public List<string> AccessLevels;

        public GarageConsoleComponentState(List<string> accessLevels)
        {
            AccessLevels = accessLevels;
        }
    }

}
