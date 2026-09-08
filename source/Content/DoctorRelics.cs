using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WeiDoctor.Content;

[RegisterRelic(typeof(DoctorRelicPool))]
public sealed class DispatchCenterRelic : ModRelicTemplate
{
    private const int MaxDeploymentSlots = 3;

    private Queue<DeploymentEntry> _deployments = new();

    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool ShowCounter => true;
    public override int DisplayAmount => _deployments.Count;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://images/atlases/relic_atlas.sprites/burning_blood.tres",
        IconOutlinePath: "res://images/atlases/relic_outline_atlas.sprites/burning_blood.tres",
        BigIconPath: "res://images/relics/burning_blood.png");

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _deployments = new Queue<DeploymentEntry>(_deployments);
    }

    public override Task AfterObtained()
    {
        Entry.Logger.Info("[DispatchCenterRelic] obtained.");
        return Task.CompletedTask;
    }

    public override Task BeforeCombatStart()
    {
        _deployments.Clear();
        InvokeDisplayAmountChanged();
        DeploymentCombatUi.Refresh(GetDeploymentDisplayEntries(), MaxDeploymentSlots);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card is not OperatorCard operatorCard || !operatorCard.CanDeploy || !cardPlay.IsLastInSeries)
        {
            return;
        }

        while (_deployments.Count >= MaxDeploymentSlots)
        {
            DeploymentEntry retreated = _deployments.Dequeue();
            await ReturnCopyToPile(choiceContext, retreated, PileType.Discard);
            Entry.Logger.Info($"[DispatchCenterRelic] retreated {retreated.CardType.Name} to discard.");
        }

        _deployments.Enqueue(new DeploymentEntry(operatorCard.GetType(), operatorCard.DeployTurns, operatorCard.OperatorName, operatorCard.ArtRelativePath));
        Flash();
        InvokeDisplayAmountChanged();
        DeploymentCombatUi.Refresh(GetDeploymentDisplayEntries(), MaxDeploymentSlots);
        DeploymentCombatUi.Pulse();
        Entry.Logger.Info($"[DispatchCenterRelic] deployed {operatorCard.GetType().Name} for {operatorCard.DeployTurns} turns.");
    }

    public override async Task AfterAutoPostPlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (_deployments.Count == 0 || player != Owner)
        {
            return;
        }

        ICombatState? combatState = Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        Flash();
        Queue<DeploymentEntry> nextDeployments = new();
        PlayerCombatTargeting targeting = new()
        {
            CombatState = combatState,
            PlayerCreature = Owner.Creature,
        };

        while (_deployments.Count > 0)
        {
            DeploymentEntry deployment = _deployments.Dequeue();
            OperatorCard? operatorCard = CreateTemporaryOperatorCard(combatState, deployment.CardType);
            if (operatorCard != null)
            {
                await operatorCard.OnDeployTick(choiceContext, targeting);
            }

            int remainingTurns = deployment.RemainingTurns - 1;
            if (remainingTurns <= 0)
            {
                await ReturnCopyToPile(choiceContext, deployment, PileType.Draw);
                Entry.Logger.Info($"[DispatchCenterRelic] {deployment.CardType.Name} skill ended and returned to draw pile.");
            }
            else
            {
                nextDeployments.Enqueue(deployment with { RemainingTurns = remainingTurns });
            }
        }

        _deployments = nextDeployments;
        InvokeDisplayAmountChanged();
        DeploymentCombatUi.Refresh(GetDeploymentDisplayEntries(), MaxDeploymentSlots);
        DeploymentCombatUi.Pulse();
    }

    private OperatorCard? CreateTemporaryOperatorCard(ICombatState combatState, Type cardType)
    {
        CardModel canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));
        CardModel mutable = combatState.CreateCard(canonical, Owner);
        return mutable as OperatorCard;
    }

    private async Task ReturnCopyToPile(PlayerChoiceContext choiceContext, DeploymentEntry deployment, PileType pileType)
    {
        ICombatState? combatState = Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        CardModel canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(deployment.CardType));
        CardModel returnedCard = combatState.CreateCard(canonical, Owner);
        await CardPileCmd.AddGeneratedCardToCombat(returnedCard, pileType, Owner, CardPilePosition.Top);
    }

    private IReadOnlyList<DeploymentDisplayEntry> GetDeploymentDisplayEntries()
    {
        return _deployments
            .Select(entry => new DeploymentDisplayEntry(entry.Name, entry.RemainingTurns, entry.ArtRelativePath))
            .ToList();
    }

    private readonly record struct DeploymentEntry(Type CardType, int RemainingTurns, string Name, string ArtRelativePath);
}
