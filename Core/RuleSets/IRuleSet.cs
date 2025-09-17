namespace ChessRogue.Core.RuleSets
{
    public interface IRuleSet
    {
        IEnumerable<Move> GetLegalMoves(GameState state, IPiece piece);
        bool IsGameOver(GameState state, out PlayerColor winner);
    }
}
