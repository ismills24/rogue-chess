namespace ChessRogue.Core
{
    public interface IPiece
    {
        PlayerColor Owner { get; }
        Vector2Int Position { get; set; }
        string Name { get; }

        IEnumerable<Move> GetLegalMoves(GameState state);
        void OnMove(Move move, GameState state);
        void OnCapture(GameState state);
    }
}
