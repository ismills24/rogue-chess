using System.Collections.Generic;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;

namespace RogueChess.Engine.StatusEffects
{
    public abstract class StatusEffectBase : IStatusEffect
    {
        public abstract string Name { get; }

        public virtual IEnumerable<GameEvent> OnTurnStart(IPiece piece, GameState state)
        {
            yield break;
        }

        public virtual IEnumerable<GameEvent> OnTurnEnd(IPiece piece, GameState state)
        {
            yield break;
        }

        public virtual int ValueModifier() => 0;

        public abstract IStatusEffect Clone();

        public override string ToString() => Name;
    }
}
