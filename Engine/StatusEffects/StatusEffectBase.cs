using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;

namespace RogueChess.Engine.StatusEffects
{
    /// <summary>
    /// Base class with common helpers for status effects.
    /// Subclasses override OnTurnStart and may override ValueModifier.
    /// </summary>
    public abstract class StatusEffectBase : IStatusEffect
    {
        public abstract string Name { get; }

        public abstract IEnumerable<CandidateEvent> OnTurnStart(IPiece piece, GameState state);

        public virtual int ValueModifier() => 0;

        public abstract IStatusEffect Clone();

        public override string ToString() => Name;
    }
}
