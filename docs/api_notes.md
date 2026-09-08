# RitsuLib/API Notes For v1.0

Useful RitsuLib 0.5.19 surfaces found in `lib/0.111.0/STS2-RitsuLib.xml`:

- `STS2RitsuLib.RitsuLibFramework.RegisterCardOnPlayHookListener(...)`
  - Good fit for deployment timing.
  - Use the after-play hook so each operator first resolves its normal combat effect, then enters the deployment zone if it is a deployable operator card.

- `STS2RitsuLib.Cards.ICardOnPlayHookListener`
  - `BeforeCardOnPlay(...)` can skip native card `OnPlay` if needed.
  - `AfterCardOnPlay(...)` runs after the card's own play point and before later card-play processing.

- `STS2RitsuLib.Content.ModContentRegistry.For(modId)`
  - Entry point for mod-owned content registration.
  - Relevant registration methods include `RegisterCard<TCard,TCardModel>`, `RegisterRelic<TRelic,TRelicModel>`, `RegisterPotion`, model capabilities, scoped/global catalog merges, character-owned visual overrides, and keyword ID helpers.

- `STS2RitsuLib.RitsuLibFramework`
  - Framework-level helpers include `CreateContentPack(modId)`, `RegisterCardOnPlayHookListener`, `RegisterFreePlayBinding`, `GetCardTransformRegistry`, `RegisterModelCapability`, and `ConfigureDefaultModelCapabilities`.

- `STS2RitsuLib.Cards.Transforms.ModCardTransformRegistry`
  - Useful for promotion or special transform listeners if the base game treats card upgrade/transform as model replacement.
  - v1.0 promotion should still watch acquisition directly because it needs "remove 3 copies, add promoted card, then grant tier+1 draft reward".

- `STS2RitsuLib.Keywords.ModKeywordRegistry`
  - Register custom terms such as `部署`, `天资`, `筹措`, `灼燃`, `炎佑`, `缓慢`, `冻结`, and bond-layer keywords.
  - `RegisterCardKeywordOwnedByLocNamespace(...)` can bind keyword title/description from localization tables.

- `STS2RitsuLib.Cards.DynamicVars.ModCardVars`
  - Use for card text numbers such as damage, block, heal, repeat count, deployment turns, bond layers, and dispatch-center values.

- `STS2RitsuLib.RitsuLibFramework.RegisterModSettings(...)`
  - Good fit for in-game settings through RitsuLib.
  - v1.0 settings to expose: random mode, deployment slots, initial draft offer count, promotion copy count, dispatch-center cost tuning.

Implementation mapping:

- Current v1.0 skeleton uses the same current-version pattern seen in the valid reference mods under `D:/大黄算法？？？/`: a `[ModInitializer]` entry point, `RitsuLibFramework.EnsureGodotScriptsRegistered`, `ModTypeDiscoveryHub.RegisterModAssembly`, `CreateContentPack(...).SharedCardPool(...).SharedRelicPool(...).Apply()`, and Harmony patch registration.

- Character/content registration:
  - Register the Doctor character.
  - Register starter relic, bond relics, and all operator cards from `data/cards.json`.
  - Keep cards marked `pool: special` out of normal rewards.

- Deployment:
  - Ability cards do not deploy; they add a combat-duration status.
  - Non-ability operator cards resolve normal battle effect first.
  - After normal battle effect, deploy the card for its configured duration.
  - If the deployment zone is full, retreat the oldest deployed operator to the discard pile.
  - At end of turn, trigger each deployed operator's deployment effect in deployment order.
  - When deployment duration expires, return that operator card to the draw pile.

- Rewards:
  - Filter normal card rewards by current dispatch-center level.
  - Levels 1-4: highest probability should be current level.
  - Levels 5-6: level 4 should be the mode, with levels 1-3 reduced.
  - Special operators from `调和` and `协防` stay out of normal rewards.

- Random mode:
  - Generate the run's card pool at run start.
  - Each generated card combines three source fragments: portrait source, first-half-name source, second-half-name source.
  - Effects are randomly composed only from those selected source operators, not from the full operator pool.
  - Sentence fragments may be recombined aggressively, including grammatically chaotic combinations.
