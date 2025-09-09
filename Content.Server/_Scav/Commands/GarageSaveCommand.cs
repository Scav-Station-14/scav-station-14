using Content.Server._Scav.Shipyard.Systems;
using Content.Server.Administration;
using Content.Shared._Scav.Persistence;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Scav.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class GarageSave : LocalizedEntityCommands
{
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly GarageSystem _garage = default!;

    public override string Command => "garagesave";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError("Wrong number of arguments.");
            return;
        }

        if (!NetEntity.TryParse(args[0], out var uidNet))
        {
            shell.WriteError("Not a valid entity ID.");
            return;
        }

        var uid = _ent.GetEntity(uidNet);

        // no saving default grid
        if (!_ent.EntityExists(uid))
        {
            shell.WriteError("That grid does not exist.");
            return;
        }

        if (_ent.TryGetComponent<ShuttlePersistenceTrackerComponent>(uid, out var persistence) && !String.IsNullOrEmpty(persistence.ShipGuid))
        {
            bool saveSuccess = _garage.TryStoreShip(uid, persistence);
            if(saveSuccess)
            {
                shell.WriteLine("Save successful.");
            }
            else
            {
                shell.WriteError("Save unsuccessful!");
            }
        }
        else
        {
            shell.WriteError("Grid is not marked as a persistent ship.");
        }
    }
}
