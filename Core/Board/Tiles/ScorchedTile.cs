using ChessRogue.Core.Events;
using ChessRogue.Core.Pieces.Decorators;
using ChessRogue.Core.StatusEffects;

namespace ChessRogue.Core.Board.Tiles
{
    public class ScorchedTile : StandardTile
    {
        // Helper: ensure the piece is a carrier; if not, wrap it on the board
        private IPiece EnsureCarrier(IPiece piece, Vector2Int pos, GameState state)
        {
            if (piece is IStatusEffectCarrier)
                return piece;

            var wrapped = new StatusEffectDecorator(piece);
            // Replace the piece reference on the board at the same position
            state.Board.PlacePiece(wrapped, pos);
            return wrapped;
        }

        // Apply burning immediately on landing
        public override IEnumerable<GameEvent> OnEnter(
            IPiece piece,
            Vector2Int pos,
            GameState state
        )
        {
            var carrierPiece = EnsureCarrier(piece, pos, state);
            var carrier = (IStatusEffectCarrier)carrierPiece;

            if (!carrier.GetStatuses().Any(s => s.Name == "Burning"))
            {
                carrier.AddStatus(new BurningStatus());
                yield return new GameEvent(
                    GameEventType.StatusEffectTriggered,
                    carrierPiece,
                    pos,
                    null,
                    "Piece is burning!"
                );
            }
        }

        // Keep burning any piece that starts its turn here
        public override IEnumerable<GameEvent> OnTurnStart(
            IPiece piece,
            Vector2Int pos,
            GameState state
        )
        {
            var carrierPiece = EnsureCarrier(piece, pos, state);
            var carrier = (IStatusEffectCarrier)carrierPiece;

            if (!carrier.GetStatuses().Any(s => s.Name == "Burning"))
            {
                carrier.AddStatus(new BurningStatus());
                yield return new GameEvent(
                    GameEventType.StatusEffectTriggered,
                    carrierPiece,
                    pos,
                    null,
                    "Piece is burning!"
                );
            }
        }
    }
}
