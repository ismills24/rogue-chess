public interface IPiece
{
    PlayerColor Owner { get; }
    Vector2Int Position { get; set; }
    string Name { get; }

    /// <summary>
    /// Returns all legal moves for this piece in the given state.
    /// </summary>
    IEnumerable<Move> GetLegalMoves(GameState state);

    /// <summary>
    /// Hook for custom behavior after moving (buffs, AOE, spawning, etc).
    /// </summary>
    void OnMove(Move move, GameState state);

    /// <summary>
    /// Hook for custom behavior when captured.
    /// </summary>
    void OnCapture(GameState state);
}
