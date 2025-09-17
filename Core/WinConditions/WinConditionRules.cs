namespace ChessRogue.Core.WinConditions
{
    public class CheckmateCondition : IWinCondition
    {
        public bool IsGameOver(GameState state, out PlayerColor winner)
        {
            var currentPlayer = state.CurrentPlayer;
            var opponent =
                currentPlayer == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;

            bool kingInCheck = CheckRules.IsKingInCheck(state, currentPlayer);

            var pieces = state.Board.GetAllPieces(currentPlayer);
            var legalMoves = pieces.SelectMany(p => p.GetPseudoLegalMoves(state));
            bool hasLegalMoves = legalMoves.Any();

            if (kingInCheck && !hasLegalMoves)
            {
                winner = opponent; // checkmate
                return true;
            }
            else if (!kingInCheck && !hasLegalMoves)
            {
                winner = default; // stalemate
                return true;
            }

            winner = default;
            return false;
        }
    }

    // Helper for reusability
    public static class CheckRules
    {
        public static bool IsKingInCheck(GameState state, PlayerColor kingColor)
        {
            var king = state.Board.GetAllPieces(kingColor).FirstOrDefault(p => p is King);

            if (king == null)
                return true; // no king = already dead = checkmate

            var opponent = kingColor == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;
            var opponentPieces = state.Board.GetAllPieces(opponent);

            foreach (var piece in opponentPieces)
            {
                if (piece is Pawn pawn)
                {
                    int dir = pawn.Owner == PlayerColor.White ? 1 : -1;
                    var attacks = new[]
                    {
                        new Vector2Int(pawn.Position.x - 1, pawn.Position.y + dir),
                        new Vector2Int(pawn.Position.x + 1, pawn.Position.y + dir),
                    };

                    if (attacks.Any(a => a == king.Position))
                        return true;
                }
                else
                {
                    if (piece.GetPseudoLegalMoves(state).Any(m => m.To == king.Position))
                        return true;
                }
            }

            return false;
        }
    }
}
