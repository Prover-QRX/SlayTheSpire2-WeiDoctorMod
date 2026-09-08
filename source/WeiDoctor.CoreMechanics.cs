using System.Collections.Generic;
using System.Linq;

namespace WeiDoctor.Core;

public sealed class DeploymentZone
{
    private readonly int _slotCount;
    private readonly Queue<DeployedOperator> _operators = new();

    public DeploymentZone(int slotCount = 3)
    {
        _slotCount = slotCount;
    }

    public DeployedOperator? Deploy(string cardId, int turns)
    {
        DeployedOperator? retreated = null;
        if (_operators.Count >= _slotCount)
        {
            retreated = _operators.Dequeue();
        }
        _operators.Enqueue(new DeployedOperator(cardId, turns));
        return retreated;
    }

    public IReadOnlyList<DeployedOperator> EndTurn()
    {
        var expired = new List<DeployedOperator>();
        var remaining = new Queue<DeployedOperator>();
        while (_operators.TryDequeue(out var op))
        {
            var next = op with { TurnsRemaining = op.TurnsRemaining - 1 };
            if (next.TurnsRemaining <= 0) expired.Add(next);
            else remaining.Enqueue(next);
        }
        while (remaining.TryDequeue(out var op)) _operators.Enqueue(op);
        return expired;
    }

    public IReadOnlyList<DeployedOperator> Operators => _operators.ToList();
}

public sealed record DeployedOperator(string CardId, int TurnsRemaining);

public sealed class PromotionService
{
    public IEnumerable<string> Promote(IEnumerable<string> masterDeckCardIds, string cardId, int copiesRequired = 3)
    {
        var removed = 0;
        foreach (var id in masterDeckCardIds)
        {
            if (id == cardId && removed < copiesRequired)
            {
                removed++;
                continue;
            }
            yield return id;
        }
        if (removed == copiesRequired) yield return cardId + "_Promoted";
    }
}
