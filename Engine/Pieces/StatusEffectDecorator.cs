using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.StatusEffects;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Decorator that adds status effects to a piece.
    /// </summary>
    public class StatusEffectDecorator : PieceDecoratorBase
    {
        private readonly List<BurningStatus> statusEffects;

        public StatusEffectDecorator(IPiece inner) : base(inner)
        {
            statusEffects = new List<BurningStatus>();
        }

        private StatusEffectDecorator(IPiece inner, List<BurningStatus> statusEffects) : base(inner)
        {
            this.statusEffects = new List<BurningStatus>(statusEffects);
        }

        public void AddStatus(BurningStatus status)
        {
            statusEffects.Add(status);
        }

        public IEnumerable<BurningStatus> GetStatuses()
        {
            return statusEffects.AsReadOnly();
        }

        protected override IEnumerable<CandidateEvent> OnMoveDecorator(Move move, GameState state)
        {
            // Process all status effects on move
            foreach (var status in statusEffects.ToList())
            {
                foreach (var ev in status.OnTurnStart(Inner, state))
                {
                    yield return ev;
                }
            }
        }

        public override int GetValue()
        {
            // Status effects might modify piece value
            var baseValue = Inner.GetValue();
            var statusValue = statusEffects.Sum(s => s.Duration > 0 ? -1 : 0); // Burning reduces value
            return baseValue + statusValue;
        }

        protected override IPiece CreateDecoratorClone(IPiece inner)
        {
            var clonedStatuses = statusEffects.Select(s => s.Clone()).ToList();
            return new StatusEffectDecorator(inner, clonedStatuses);
        }
    }
}
