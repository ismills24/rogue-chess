using ChessRogue.Core.Events;

namespace ChessRogue.Core.StatusEffects
{
    public class BurningStatus : IStatusEffect
    {
        public string Name => "Burning";
        public int Duration => turnsRemaining;

        private int turnsRemaining = 2;

        public IEnumerable<GameEvent> OnTurnStart(IPiece piece, GameState state)
        {
            turnsRemaining--;

            if (turnsRemaining <= 0)
            {
                state.Board.RemovePiece(piece.Position);

                yield return new GameEvent(
                    GameEventType.StatusEffectTriggered,
                    piece,
                    piece.Position,
                    null,
                    "Burned to ashes!"
                );
            }
        }

        public IEnumerable<GameEvent> OnRemove(IPiece piece, GameState state)
        {
            // Optional cleanup, e.g. visual/sound effects
            yield break;
        }

        public IStatusEffect Clone()
        {
            return new BurningStatus { turnsRemaining = this.turnsRemaining };
        }
    }
}
