using ChessRogue.Core.Events;

namespace ChessRogue.Core.Board
{
    public interface ITile
    {
        // Can a piece enter this square?
        bool CanEnter(IPiece piece, Vector2Int pos, GameState state);

        // What happens when a piece lands here?
        IEnumerable<GameEvent> OnEnter(IPiece piece, Vector2Int pos, GameState state);

        // Optional: effect while standing on it
        IEnumerable<GameEvent> OnTurnStart(IPiece piece, Vector2Int pos, GameState state);
    }
}
