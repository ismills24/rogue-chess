using System;
using System.Collections.Generic;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;

namespace RogueChess.Engine.StatusEffects
{
    public class BurningStatus : StatusEffectBase
    {
        public override string Name => "Burning";
        public int Duration { get; private set; } = 2;

        public BurningStatus() { }

        public BurningStatus(BurningStatus original)
            : base(original) => Duration = original.Duration;

        private BurningStatus(int duration) => Duration = duration;

        public override IEnumerable<GameEvent> OnTurnEnd(IPiece piece, GameState state)
        {
            Duration--;

            // Optional telemetry event for UI:
            yield return new StatusTickEvent(piece, this, Duration, piece.Owner);

            if (Duration <= 0)
                yield return new DestroyEvent(piece, "Burned to ashes!", piece.Owner, ID);
        }

        public override int ValueModifier() => -1 * Math.Max(0, Duration);
    }
}
