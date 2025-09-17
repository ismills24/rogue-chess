using ChessRogue.Core.Events;

namespace ChessRogue.Core.Board
{
    public class StandardTile : ITile
    {
        public virtual bool CanEnter(IPiece piece, Vector2Int pos, GameState state) => true;

        public virtual IEnumerable<GameEvent> OnEnter(IPiece piece, Vector2Int pos, GameState state)
        {
            var move = new Move(piece.Position, pos, piece);
            yield return new GameEvent(
                GameEventType.TileEffectTriggered,
                piece,
                move.From,
                move.To,
                "Moved"
            );
        }

        public virtual IEnumerable<GameEvent> OnTurnStart(
            IPiece piece,
            Vector2Int pos,
            GameState state
        ) => Array.Empty<GameEvent>();
    }
}
