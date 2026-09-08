using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WeiDoctor.Content;

public abstract class DoctorCard : ModCardTemplate
{
    protected const string DefaultAttackPortrait = "res://images/atlases/card_atlas.sprites/ironclad/strike_ironclad.tres";
    protected const string DefaultSkillPortrait = "res://images/atlases/card_atlas.sprites/ironclad/defend_ironclad.tres";

    public override CardPoolModel VisualCardPool
    {
        get
        {
            CardPoolModel? pool = Owner?.Character?.CardPool;
            return pool ?? base.VisualCardPool;
        }
    }

    protected DoctorCard(int cost, CardType type, CardRarity rarity, TargetType target)
        : base(cost, type, rarity, target)
    {
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class TacticalFront : DoctorCard
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Basic;
    private const TargetType CardTarget = TargetType.AnyEnemy;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: DefaultAttackPortrait,
        BetaPortraitPath: DefaultAttackPortrait);

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DamageVar(6m, ValueProp.Move),
    };

    public TacticalFront()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class TacticalBack : DoctorCard
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Basic;
    private const TargetType CardTarget = TargetType.Self;

    public override bool GainsBlock => true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: DefaultSkillPortrait,
        BetaPortraitPath: DefaultSkillPortrait);

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new BlockVar(5m, ValueProp.Move),
    };

    public TacticalBack()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
