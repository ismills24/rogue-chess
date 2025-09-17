using ChessRogue.Core.Rules;

namespace ChessRogue.Core.RuleSets
{
    public class StandardChessRuleSet : IRuleSet
    {
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
    }
}
