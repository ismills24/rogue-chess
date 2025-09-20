using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.StatusEffects;

namespace RogueChess.Engine.Pieces.Decorators
{
    /// <summary>
    /// Decorator that attaches status effects (Burning, Frozen, etc) to a piece.
    /// </summary>
    public class StatusEffectDecorator : PieceDecoratorBase
    {
        private readonly List<IStatusEffect> _statuses;

        public StatusEffectDecorator(IPiece inner)
            : base(inner)
        {
            _statuses = new List<IStatusEffect>();
        }

        private StatusEffectDecorator(IPiece inner, List<IStatusEffect> statuses)
            : base(inner)
        {
            _statuses = statuses;
        }

        public void AddStatus(IStatusEffect status) => _statuses.Add(status);

        public IEnumerable<IStatusEffect> GetStatuses() => _statuses.AsReadOnly();

        public override IEnumerable<CandidateEvent> OnTurnStart(GameState state)
        {
            foreach (var status in _statuses.ToList()) // copy in case of mutation
            {
                foreach (var ev in status.OnTurnStart(Inner, state))
                    yield return ev;
            }
        }

        public override IEnumerable<CandidateEvent> OnTurnEnd(GameState state)
        {
            foreach (var status in _statuses.ToList())
            foreach (var ev in status.OnTurnEnd(Inner, state))
                yield return ev;
        }

        public override int GetValue()
        {
            var baseValue = Inner.GetValue();
            // Simple example: penalize if piece has negative statuses
            var penalty = _statuses.Sum(s => s.ValueModifier());
            return baseValue + penalty;
        }

        protected override IPiece CreateDecoratorClone(IPiece inner)
        {
            var clonedStatuses = _statuses.Select(s => s.Clone()).ToList();
            return new StatusEffectDecorator(inner, clonedStatuses);
        }
    }
}
