using ChessRogue.Core.Events;

namespace ChessRogue.Core.StatusEffects
{
    public class BurningStatus : IStatusEffect
    {
        public string Name => "Burning";
        public int Duration { get; private set; }

        public BurningStatus(int duration) => Duration = duration;

        public IEnumerable<GameEvent> OnTurnStart(IPiece piece, GameState state)
        {
            Duration--;

            yield return new GameEvent(
                GameEventType.StatusEffectTriggered,
                piece,
                piece.Position,
                piece.Position,
                $"Burning! {Duration} turns left"
            );

            if (Duration <= 0)
            {
                state.Board.RemovePiece(piece.Position);
                yield return new GameEvent(
                    GameEventType.PieceCaptured,
                    piece,
                    piece.Position,
                    piece.Position,
                    "Burned to ash!"
                );
            }
        }

        public IEnumerable<GameEvent> OnRemove(IPiece piece, GameState state)
        {
            yield break;
        }

        public IStatusEffect Clone() => new BurningStatus(Duration);
    }
}
