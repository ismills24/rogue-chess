namespace ChessRogue.Core.Rules
{
    public class LastPieceStanding : IWinCondition
    {
        public bool IsGameOver(GameState state, out PlayerColor winner)
        {
            bool whiteAlive = state.Board.GetAllPieces(PlayerColor.White).Any();
            bool blackAlive = state.Board.GetAllPieces(PlayerColor.Black).Any();

            if (whiteAlive && !blackAlive)
            {
                winner = PlayerColor.White;
                return true;
            }

            if (blackAlive && !whiteAlive)
            {
                winner = PlayerColor.Black;
                return true;
            }

            winner = default;
            return false;
        }
    }

    public class CheckmateCondition : IWinCondition
    {
        public bool IsGameOver(GameState state, out PlayerColor winner)
        {
            var currentPlayer = state.CurrentPlayer;
            var opponent =
                currentPlayer == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;

            // Check if current player's king is under attack
            bool kingInCheck = IsKingInCheck(state, currentPlayer);

            // See if current player has any legal moves
            var pieces = state.Board.GetAllPieces(currentPlayer);
            var legalMoves = pieces.SelectMany(p => p.GetLegalMoves(state));
            bool hasLegalMoves = legalMoves.Any();

            if (kingInCheck && !hasLegalMoves)
            {
                // Checkmate
                winner = opponent;
                return true;
            }
            else if (!kingInCheck && !hasLegalMoves)
            {
                // Stalemate = draw (for now, we’ll say "no winner")
                winner = default;
                return true;
            }

            winner = default;
            return false;
        }

        private bool IsKingInCheck(GameState state, PlayerColor kingColor)
        {
            var king = state.Board.GetAllPieces(kingColor).FirstOrDefault(p => p is King);

            if (king == null)
                return true; // dead king = checkmate

            var opponent = kingColor == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;
            var opponentPieces = state.Board.GetAllPieces(opponent);

            foreach (var piece in opponentPieces)
            {
                if (piece.GetLegalMoves(state).Any(m => m.To == king.Position))
                    return true;
            }

            return false;
        }
    }
}
