namespace ChessRogue.Core
{
    public interface IPiece
    {
        PlayerColor Owner { get; }
        Vector2Int Position { get; set; }
        string Name { get; }

        // PSEUDO-LEGAL (no king-safety filtering)
        IEnumerable<Move> GetPseudoLegalMoves(GameState state);

        void OnMove(Move move, GameState state);
        void OnCapture(GameState state);

        // NEW: deep clone this piece
        IPiece Clone();
    }
}
