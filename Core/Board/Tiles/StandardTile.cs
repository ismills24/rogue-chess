// StandardTile.cs
using ChessRogue.Core.Events;

namespace ChessRogue.Core.Board
{
    public class StandardTile : ITile
    {
        public virtual bool CanEnter(IPiece piece, Vector2Int pos, GameState state) => true;

        // No events by default — avoid re-trigger loops.
        public virtual IEnumerable<GameEvent> OnEnter(
            IPiece piece,
            Vector2Int pos,
            GameState state
        ) => Array.Empty<GameEvent>();

        public virtual IEnumerable<GameEvent> OnTurnStart(
            IPiece piece,
            Vector2Int pos,
            GameState state
        ) => Array.Empty<GameEvent>();
    }
}
