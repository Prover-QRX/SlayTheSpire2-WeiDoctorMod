using System;
using System.Collections.Generic;

namespace WeiDoctor.BetaAdapter;

// This file is intentionally isolated from the game API.
// Replace ISts2BetaApi calls with the concrete Slay the Spire 2 beta API once the game assemblies are available.
public interface ISts2BetaApi
{
    void RegisterCharacter(DoctorCharacterDefinition definition);
    void RegisterCard(DoctorCardDefinition definition);
    void RegisterRelic(DoctorRelicDefinition definition);
    void RegisterSettings(DoctorModSettings settings);
    void AddCombatState(string stateId, int amount);
    void AddToDrawPile(string cardId);
    void AddToDiscardPile(string cardId);
}

public sealed record DoctorCharacterDefinition(string Id, string Name, string StarterRelicId, IReadOnlyList<string> StarterDeck);
public sealed record DoctorCardDefinition(string Id, string Name, string Type, int Cost, int Tier, string BattleEffect, string DeployEffect, string TraitEffect);
public sealed record DoctorRelicDefinition(string Id, string Name, string Effect);
public sealed record DoctorModSettings(bool RandomMode, int DeploySlots, int InitialDraftOfferCount, int PromotionCopiesRequired);
