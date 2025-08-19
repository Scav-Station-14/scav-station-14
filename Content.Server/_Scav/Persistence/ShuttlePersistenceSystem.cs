namespace Content.Server._Scav.Persistence;

public sealed partial class ShuttlePersistenceSystem : EntitySystem
{
    //public static readonly MapInitEvent MapInitEventInstance = new(); //replace with GridReinitEventInstance once initial testing is done

    public override void Initialize()
    {
        base.Initialize();
    }

    public void RecursiveGridReInit(EntityUid gridEntity)
    {
        var toInitialize = new List<EntityUid> { gridEntity };
        for (var i = 0; i < toInitialize.Count; i++)
        {
            var uid = toInitialize[i];
            // toInitialize might contain deleted entities.
            //if (!_metaQuery.TryComp(uid, out var meta))
            //    continue;

            var children = Transform(uid).ChildEnumerator;
            while (children.MoveNext(out var child))
            {
                toInitialize.Add(child);
            }

            var ev = new GridReInitEvent(); //this is a LOT of variable declarations because its inside a recursive loop, was it better to define it once?
            RaiseLocalEvent(uid, ev);
        }
    }
}


public sealed class GridReInitEvent : EntityEventArgs
{
    public GridReInitEvent()
    {
    }
}
