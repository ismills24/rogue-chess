using ChessRogue.Core.WinConditions;

namespace ChessRogue.Core.RuleSets
{
    public class StandardChessRuleSet : IRuleSet
    {
        private readonly CheckmateCondition checkmate = new();

        public IEnumerable<Move> GetLegalMoves(GameState state, IPiece piece)
        {
            foreach (var move in piece.GetPseudoLegalMoves(state))
            {
                var clone = state.Clone();
                clone.ApplyMove(move);

                if (!CheckRules.IsKingInCheck(clone, piece.Owner))
                    yield return move;
            }
        }

        public bool IsGameOver(GameState state, out PlayerColor winner)
        {
            return checkmate.IsGameOver(state, out winner);
        }
    }
}
