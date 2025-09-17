namespace ChessRogue.Core.Board
{
    public interface ITile
    {
        // Can a piece enter this square?
        bool CanEnter(IPiece piece, Vector2Int pos, GameState state);

        // What happens when a piece lands here?
        void OnEnter(IPiece piece, Vector2Int pos, GameState state);

        // Optional: effect while standing on it
        void OnTurnStart(IPiece piece, Vector2Int pos, GameState state);
    }
}
