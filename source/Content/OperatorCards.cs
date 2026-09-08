using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WeiDoctor.Content;

public abstract class OperatorCard : DoctorCard
{
    public int OperatorTier { get; }
    public int DeployTurns { get; }
    public abstract string OperatorName { get; }
    public abstract string ArtRelativePath { get; }

    public bool CanDeploy => Type != CardType.Power && DeployTurns > 0;

    protected OperatorCard(int cost, CardType type, CardRarity rarity, TargetType target, int tier, int deployTurns)
        : base(cost, type, rarity, target)
    {
        OperatorTier = tier;
        DeployTurns = deployTurns;
    }

    protected override CardLocation GetResultLocationForCardPlay()
    {
        if (CanDeploy)
        {
            return new CardLocation(Owner, PileType.None, CardPilePosition.Bottom);
        }

        return base.GetResultLocationForCardPlay();
    }

    public virtual Task OnDeployTick(PlayerChoiceContext choiceContext, PlayerCombatTargeting targeting)
    {
        return Task.CompletedTask;
    }

    protected static Creature? LowestHpEnemy(ICombatState? combatState)
    {
        return combatState?.HittableEnemies
            .Where(enemy => enemy.IsAlive)
            .OrderBy(enemy => enemy.CurrentHp)
            .FirstOrDefault();
    }

    protected async Task Attack(PlayerChoiceContext choiceContext, CardPlay? cardPlay, Creature target, decimal amount, string hitFx = "vfx/vfx_attack_slash")
    {
        await DamageCmd.Attack(amount)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .WithHitFx(hitFx)
            .Execute(choiceContext);
    }

    protected async Task AttackAll(PlayerChoiceContext choiceContext, CardPlay? cardPlay, decimal amount, string hitFx = "vfx/vfx_attack_slash")
    {
        ICombatState? combatState = Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        await DamageCmd.Attack(amount)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(combatState)
            .SpawningHitVfxOnEachCreature()
            .WithHitFx(hitFx)
            .Execute(choiceContext);
    }

    protected async Task ApplyPower<T>(PlayerChoiceContext choiceContext, Creature? target, decimal amount) where T : PowerModel
    {
        if (target == null)
        {
            return;
        }

        await PowerCmd.Apply<T>(choiceContext, target, amount, Owner.Creature, this);
    }

    protected async Task ApplyPowerToAll<T>(PlayerChoiceContext choiceContext, decimal amount) where T : PowerModel
    {
        ICombatState? combatState = Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        await PowerCmd.Apply<T>(choiceContext, combatState.HittableEnemies.Where(enemy => enemy.IsAlive), amount, Owner.Creature, this);
    }

    protected async Task DrawCards(PlayerChoiceContext choiceContext, decimal amount)
    {
        await CardPileCmd.Draw(choiceContext, amount, Owner);
    }

    protected Creature? RandomEnemy(ICombatState? combatState)
    {
        List<Creature> enemies = combatState?.HittableEnemies
            .Where(enemy => enemy.IsAlive)
            .ToList() ?? new List<Creature>();

        if (enemies.Count == 0)
        {
            return null;
        }

        return enemies[Random.Shared.Next(enemies.Count)];
    }
}

public sealed class PlayerCombatTargeting
{
    public required ICombatState CombatState { get; init; }
    public required Creature PlayerCreature { get; init; }

    public Creature? LowestHpEnemy => CombatState.HittableEnemies
        .Where(enemy => enemy.IsAlive)
        .OrderBy(enemy => enemy.CurrentHp)
        .FirstOrDefault();
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class YinxianOperator : OperatorCard
{
    public override string OperatorName => "隐现";
    public override string ArtRelativePath => "assets/operators/01_I阶/001_Ⅰ阶_隐现.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultAttackPortrait, BetaPortraitPath: DefaultAttackPortrait);

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(10m, ValueProp.Move),
        new PowerVar<WeakPower>(1m),
    };

    public YinxianOperator() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, 1, 3)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await Attack(choiceContext, cardPlay, cardPlay.Target, DynamicVars.Damage.BaseValue);
        await ApplyPower<WeakPower>(choiceContext, cardPlay.Target, DynamicVars.Weak.BaseValue);
    }

    public override async Task OnDeployTick(PlayerChoiceContext choiceContext, PlayerCombatTargeting targeting)
    {
        Creature? target = targeting.LowestHpEnemy;
        if (target == null)
        {
            return;
        }

        await Attack(choiceContext, null, target, 6m);
        await ApplyPower<WeakPower>(choiceContext, target, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class JiaofengOperator : OperatorCard
{
    public override bool GainsBlock => true;

    public override string OperatorName => "角峰";
    public override string ArtRelativePath => "assets/operators/01_I阶/002_Ⅰ阶_角峰.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultSkillPortrait, BetaPortraitPath: DefaultSkillPortrait);

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(9m, ValueProp.Move),
        new PowerVar<WeakPower>(1m),
    };

    public JiaofengOperator() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, 1, 3)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await ApplyPower<WeakPower>(choiceContext, cardPlay.Target, DynamicVars.Weak.BaseValue);
    }

    public override async Task OnDeployTick(PlayerChoiceContext choiceContext, PlayerCombatTargeting targeting)
    {
        await CreatureCmd.GainBlock(targeting.PlayerCreature, 6m, ValueProp.Move, null);
        await ApplyPower<WeakPower>(choiceContext, targeting.LowestHpEnemy, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class JingzheOperator : OperatorCard
{
    public override string OperatorName => "惊蛰";
    public override string ArtRelativePath => "assets/operators/01_I阶/003_Ⅰ阶_惊蛰.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultAttackPortrait, BetaPortraitPath: DefaultAttackPortrait);

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(7m, ValueProp.Move),
        new PowerVar<SlowPower>(1m),
    };

    public JingzheOperator() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, 1, 3)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AttackAll(choiceContext, cardPlay, DynamicVars.Damage.BaseValue, "vfx/vfx_lightning_orb");
        await ApplyPowerToAll<SlowPower>(choiceContext, DynamicVars["SlowPower"].BaseValue);
    }

    public override async Task OnDeployTick(PlayerChoiceContext choiceContext, PlayerCombatTargeting targeting)
    {
        await AttackAll(choiceContext, null, 4m, "vfx/vfx_lightning_orb");
        await ApplyPowerToAll<SlowPower>(choiceContext, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class ShenxunOperator : OperatorCard
{
    public override string OperatorName => "深巡";
    public override string ArtRelativePath => "assets/operators/01_I阶/004_Ⅰ阶_深巡.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultAttackPortrait, BetaPortraitPath: DefaultAttackPortrait);

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(11m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1m),
    };

    public ShenxunOperator() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, 1, 3)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await Attack(choiceContext, cardPlay, cardPlay.Target, DynamicVars.Damage.BaseValue, "vfx/vfx_attack_blunt");
        await ApplyPower<VulnerablePower>(choiceContext, cardPlay.Target, DynamicVars.Vulnerable.BaseValue);
    }

    public override async Task OnDeployTick(PlayerChoiceContext choiceContext, PlayerCombatTargeting targeting)
    {
        Creature? target = targeting.LowestHpEnemy;
        if (target == null)
        {
            return;
        }

        await Attack(choiceContext, null, target, 7m, "vfx/vfx_attack_blunt");
        await ApplyPower<VulnerablePower>(choiceContext, target, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class ProvenceOperator : OperatorCard
{
    public override string OperatorName => "普罗旺斯";
    public override string ArtRelativePath => "assets/operators/01_I阶/006_Ⅰ阶_普罗旺斯.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultAttackPortrait, BetaPortraitPath: DefaultAttackPortrait);

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(12m, ValueProp.Move),
        new PowerVar<WeakPower>(1m),
    };

    public ProvenceOperator() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, 1, 3)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await Attack(choiceContext, cardPlay, cardPlay.Target, DynamicVars.Damage.BaseValue);
        await ApplyPower<WeakPower>(choiceContext, cardPlay.Target, DynamicVars.Weak.BaseValue);
    }

    public override async Task OnDeployTick(PlayerChoiceContext choiceContext, PlayerCombatTargeting targeting)
    {
        Creature? target = targeting.LowestHpEnemy;
        if (target == null)
        {
            return;
        }

        await Attack(choiceContext, null, target, 7m);
        await ApplyPower<WeakPower>(choiceContext, target, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class GummyOperator : OperatorCard
{
    public override bool GainsBlock => true;

    public override string OperatorName => "古米";
    public override string ArtRelativePath => "assets/operators/01_I阶/009_Ⅰ阶_古米.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultSkillPortrait, BetaPortraitPath: DefaultSkillPortrait);

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(10m, ValueProp.Move),
        new PowerVar<WeakPower>(1m),
    };

    public GummyOperator() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, 1, 3)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await ApplyPower<WeakPower>(choiceContext, cardPlay.Target, DynamicVars.Weak.BaseValue);
    }

    public override async Task OnDeployTick(PlayerChoiceContext choiceContext, PlayerCombatTargeting targeting)
    {
        await CreatureCmd.GainBlock(targeting.PlayerCreature, 6m, ValueProp.Move, null);
        await ApplyPower<WeakPower>(choiceContext, targeting.LowestHpEnemy, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class CimeiOperator : OperatorCard
{
    public override string OperatorName => "刺玫";
    public override string ArtRelativePath => "assets/operators/01_I阶/005_Ⅰ阶_刺玫.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultSkillPortrait, BetaPortraitPath: DefaultSkillPortrait);

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(8m, ValueProp.Move),
    };

    public CimeiOperator() : base(1, CardType.Power, CardRarity.Common, TargetType.Self, 1, 0)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class TexasOperator : OperatorCard
{
    public override string OperatorName => "德克萨斯";
    public override string ArtRelativePath => "assets/operators/01_I阶/007_Ⅰ阶_德克萨斯.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultSkillPortrait, BetaPortraitPath: DefaultSkillPortrait);

    public TexasOperator() : base(1, CardType.Power, CardRarity.Common, TargetType.Self, 1, 0)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await DrawCards(choiceContext, 1m);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class YueyueOperator : OperatorCard
{
    public override string OperatorName => "跃跃";
    public override string ArtRelativePath => "assets/operators/01_I阶/008_Ⅰ阶_跃跃.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultAttackPortrait, BetaPortraitPath: DefaultAttackPortrait);

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(12m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1m),
    };

    public YueyueOperator() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, 1, 3)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        await Attack(choiceContext, cardPlay, cardPlay.Target, DynamicVars.Damage.BaseValue);
        await ApplyPower<VulnerablePower>(choiceContext, cardPlay.Target, DynamicVars.Vulnerable.BaseValue);
    }

    public override async Task OnDeployTick(PlayerChoiceContext choiceContext, PlayerCombatTargeting targeting)
    {
        Creature? target = targeting.LowestHpEnemy;
        if (target == null)
        {
            return;
        }

        await Attack(choiceContext, null, target, 8m);
        await ApplyPower<VulnerablePower>(choiceContext, target, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class EstelleOperator : OperatorCard
{
    public override string OperatorName => "艾丝黛尔";
    public override string ArtRelativePath => "assets/operators/01_I阶/010_Ⅰ阶_艾丝黛尔.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultAttackPortrait, BetaPortraitPath: DefaultAttackPortrait);

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(8m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1m),
    };

    public EstelleOperator() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, 1, 3)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AttackAll(choiceContext, cardPlay, DynamicVars.Damage.BaseValue, "vfx/vfx_attack_blunt");
        await ApplyPowerToAll<VulnerablePower>(choiceContext, DynamicVars.Vulnerable.BaseValue);
    }

    public override async Task OnDeployTick(PlayerChoiceContext choiceContext, PlayerCombatTargeting targeting)
    {
        await AttackAll(choiceContext, null, 5m, "vfx/vfx_attack_blunt");
        await ApplyPowerToAll<VulnerablePower>(choiceContext, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class PodencoOperator : OperatorCard
{
    public override string OperatorName => "波登可";
    public override string ArtRelativePath => "assets/operators/01_I阶/011_Ⅰ阶_波登可.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultSkillPortrait, BetaPortraitPath: DefaultSkillPortrait);

    public PodencoOperator() : base(1, CardType.Power, CardRarity.Common, TargetType.Self, 1, 0)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await DrawCards(choiceContext, 1m);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class GreyyOperator : OperatorCard
{
    public override bool GainsBlock => true;

    public override string OperatorName => "格雷伊";
    public override string ArtRelativePath => "assets/operators/01_I阶/012_Ⅰ阶_格雷伊.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultSkillPortrait, BetaPortraitPath: DefaultSkillPortrait);

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(9m, ValueProp.Move),
        new PowerVar<SlowPower>(1m),
    };

    public GreyyOperator() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, 1, 3)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await DrawCards(choiceContext, 1m);
        await ApplyPower<SlowPower>(choiceContext, cardPlay.Target, DynamicVars["SlowPower"].BaseValue);
    }

    public override async Task OnDeployTick(PlayerChoiceContext choiceContext, PlayerCombatTargeting targeting)
    {
        await CreatureCmd.GainBlock(targeting.PlayerCreature, 6m, ValueProp.Move, null);
        await ApplyPower<SlowPower>(choiceContext, targeting.LowestHpEnemy, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class IndigoOperator : OperatorCard
{
    public override string OperatorName => "深靛";
    public override string ArtRelativePath => "assets/operators/01_I阶/014_Ⅰ阶_深靛.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultAttackPortrait, BetaPortraitPath: DefaultAttackPortrait);

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(12m, ValueProp.Move),
        new PowerVar<WeakPower>(1m),
    };

    public IndigoOperator() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, 1, 3)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature? target = RandomEnemy(Owner.Creature.CombatState);
        if (target == null)
        {
            return;
        }

        await Attack(choiceContext, cardPlay, target, DynamicVars.Damage.BaseValue, "vfx/vfx_lightning_orb");
        await ApplyPower<WeakPower>(choiceContext, target, DynamicVars.Weak.BaseValue);
    }

    public override async Task OnDeployTick(PlayerChoiceContext choiceContext, PlayerCombatTargeting targeting)
    {
        Creature? target = RandomEnemy(targeting.CombatState);
        if (target == null)
        {
            return;
        }

        await Attack(choiceContext, null, target, 8m, "vfx/vfx_lightning_orb");
        await ApplyPower<WeakPower>(choiceContext, target, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class UtageOperator : OperatorCard
{
    public override bool GainsBlock => true;

    public override string OperatorName => "宴";
    public override string ArtRelativePath => "assets/operators/01_I阶/015_Ⅰ阶_宴.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultSkillPortrait, BetaPortraitPath: DefaultSkillPortrait);

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(9m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1m),
    };

    public UtageOperator() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, 1, 3)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await DrawCards(choiceContext, 1m);
        await ApplyPower<VulnerablePower>(choiceContext, cardPlay.Target, DynamicVars.Vulnerable.BaseValue);
    }

    public override async Task OnDeployTick(PlayerChoiceContext choiceContext, PlayerCombatTargeting targeting)
    {
        await CreatureCmd.GainBlock(targeting.PlayerCreature, 6m, ValueProp.Move, null);
        await ApplyPower<VulnerablePower>(choiceContext, targeting.LowestHpEnemy, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class WildManeOperator : OperatorCard
{
    public override string OperatorName => "野鬃";
    public override string ArtRelativePath => "assets/operators/01_I阶/016_Ⅰ阶_野鬃.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultSkillPortrait, BetaPortraitPath: DefaultSkillPortrait);

    public WildManeOperator() : base(1, CardType.Power, CardRarity.Common, TargetType.Self, 1, 0)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}

[RegisterCard(typeof(DoctorCardPool))]
public sealed class LiskarmOperator : OperatorCard
{
    public override bool GainsBlock => true;

    public override string OperatorName => "雷蛇";
    public override string ArtRelativePath => "assets/operators/01_I阶/017_Ⅰ阶_雷蛇.png";
    public override CardAssetProfile AssetProfile => new(PortraitPath: DefaultSkillPortrait, BetaPortraitPath: DefaultSkillPortrait);

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(10m, ValueProp.Move),
        new PowerVar<WeakPower>(1m),
    };

    public LiskarmOperator() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, 1, 3)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await ApplyPower<WeakPower>(choiceContext, cardPlay.Target, DynamicVars.Weak.BaseValue);
    }

    public override async Task OnDeployTick(PlayerChoiceContext choiceContext, PlayerCombatTargeting targeting)
    {
        await CreatureCmd.GainBlock(targeting.PlayerCreature, 6m, ValueProp.Move, null);
        await ApplyPower<WeakPower>(choiceContext, targeting.LowestHpEnemy, 1m);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
