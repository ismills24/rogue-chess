using System;
using System.Collections.Generic;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;

namespace RogueChess.Engine.StatusEffects
{
    public abstract class StatusEffectBase : IStatusEffect
    {
        public abstract string Name { get; }
        public Guid ID { get; }

        protected StatusEffectBase() => ID = Guid.NewGuid();

        protected StatusEffectBase(StatusEffectBase original) => ID = original.ID;

        public virtual IEnumerable<GameEvent> OnTurnStart(IPiece piece, GameState state)
        {
            yield break;
        }

        public virtual IEnumerable<GameEvent> OnTurnEnd(IPiece piece, GameState state)
        {
            yield break;
        }

        public virtual int ValueModifier() => 0;

        public virtual IStatusEffect Clone()
        {
            var ctor = GetType().GetConstructor(new[] { GetType() });
            if (ctor != null)
                return (IStatusEffect)ctor.Invoke(new object[] { this });

            throw new InvalidOperationException($"No copy constructor for {GetType().Name}");
        }

        public override string ToString() => Name;
    }
}
