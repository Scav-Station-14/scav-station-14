using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Scav.Cargo.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CargoLabelPrinterComponent : Component
{
    /// <summary>
    /// The id of the label entity spawned by the print label button.
    /// </summary>
    [DataField("labelId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string LabelId = "PaperSalvageTag";

    /// <summary>
    /// The time at which the printer will be able to print a label again.
    /// </summary>
    [DataField("nextPrintTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextPrintTime = TimeSpan.Zero;

    /// <summary>
    /// The time between prints.
    /// </summary>
    [DataField("printDelay")]
    public TimeSpan PrintDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The sound made when printing occurs
    /// </summary>
    [DataField("printSound")]
    public SoundSpecifier PrintSound = new SoundPathSpecifier("/Audio/Machines/printer.ogg");
}
