using ChessRogue.Core.RuleSets;

namespace ChessRogue.Core.Rules
{
    public class CheckmateCondition : IWinCondition
    {
        private readonly IRuleSet ruleSet;

        public CheckmateCondition(IRuleSet ruleSet)
        {
            this.ruleSet = ruleSet;
        }

        public bool IsGameOver(GameState state, out PlayerColor winner)
        {
            var currentPlayer = state.CurrentPlayer;
            var opponent =
                currentPlayer == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;

            bool kingInCheck = CheckRules.IsKingInCheck(state, currentPlayer);

            var pieces = state.Board.GetAllPieces(currentPlayer);
            bool hasLegalMoves = pieces.SelectMany(p => ruleSet.GetLegalMoves(state, p)).Any();

            if (kingInCheck && !hasLegalMoves)
            {
                winner = opponent; // checkmate
                return true;
            }

            if (!kingInCheck && !hasLegalMoves)
            {
                winner = default; // stalemate
                return true;
            }

            winner = default;
            return false;
        }
    }
}
