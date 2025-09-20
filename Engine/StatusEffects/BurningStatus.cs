// File: Engine/StatusEffects/BurningStatus.cs
using System;
using System.Collections.Generic;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.StatusEffects
{
    public class BurningStatus : StatusEffectBase
    {
        public override string Name => "Burning";
        public int Duration { get; private set; } = 2;

        public BurningStatus() { }

        private BurningStatus(int duration) => Duration = duration;

        public override IEnumerable<CandidateEvent> OnTurnEnd(IPiece piece, GameState state)
        {
            Duration--;

            yield return new CandidateEvent(
                GameEventType.StatusTick,
                false,
                new StatusTickPayload(piece, Name, Duration)
            );

            if (Duration <= 0)
            {
                yield return new CandidateEvent(
                    GameEventType.PieceDestroyed,
                    false,
                    new PieceDestroyedPayload(piece, "Burned to ashes!")
                );
            }
        }

        public override int ValueModifier() => -1 * Math.Max(0, Duration);

        public override IStatusEffect Clone() => new BurningStatus(Duration);
    }
}
