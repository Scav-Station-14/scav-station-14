using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars // Scav
{
    /// <summary>
    ///     Whether or not the station binding mechanics are disabled.
    /// </summary>
    public static readonly CVarDef<bool> DisableStationBinding =
        CVarDef.Create("scav.DisableStationBinding", true, CVar.SERVER);

}
