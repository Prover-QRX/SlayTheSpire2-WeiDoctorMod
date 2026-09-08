using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;

namespace WeiDoctor.Content;

[RegisterCharacter]
public sealed class DoctorCharacter : ModCharacterTemplate<DoctorCardPool, DoctorRelicPool, DoctorPotionPool>
{
    public override CharacterGender Gender => CharacterGender.Neutral;

    public override Color NameColor => new("1B8A72");

    public override int StartingHp => 70;

    public override int StartingGold => 99;

    public override float AttackAnimDelay => 0.15f;

    public override float CastAnimDelay => 0.25f;

    public override Color EnergyLabelOutlineColor => new("0E3A36FF");

    public override Color DialogueColor => new("1B8A72");

    public override VfxColor SpeechBubbleColor => VfxColor.Cyan;

    public override Color MapDrawingColor => new("1B8A72");

    public override Color RemoteTargetingLineColor => new("49C7A9FF");

    public override Color RemoteTargetingLineOutline => new("0E3A36FF");

    public override List<string> GetArchitectAttackVfx()
    {
        return new List<string>
        {
            "vfx/vfx_attack_blunt",
            "vfx/vfx_attack_slash",
        };
    }
}
