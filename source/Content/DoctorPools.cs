using Godot;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace WeiDoctor.Content;

public sealed class DoctorCardPool : TypeListCardPoolModel
{
    private static readonly Material? PoolFrameTintMaterial = MaterialUtils.CreateRgbShaderMaterial(0.15f, 0.55f, 0.65f);

    public override string Title => "Doctor";
    public override string EnergyColorName => "ironclad";
    public override Color DeckEntryCardColor => new(0.45f, 0.9f, 1f);
    public override Color EnergyOutlineColor => new(0.05f, 0.25f, 0.3f);
    public override Material? PoolFrameMaterial => PoolFrameTintMaterial;
    public override bool IsColorless => false;
}

public sealed class DoctorRelicPool : TypeListRelicPoolModel
{
    public override string EnergyColorName => "ironclad";
    public override Color LabOutlineColor => new(0.45f, 0.9f, 1f);
}

public sealed class DoctorPotionPool : TypeListPotionPoolModel
{
    public override string EnergyColorName => "ironclad";

    public override string BigEnergyIconPath => "res://images/ui/top_panel/energy_icon_ironclad.png";

    public override string TextEnergyIconPath => "res://images/ui/top_panel/energy_icon_ironclad.png";
}
