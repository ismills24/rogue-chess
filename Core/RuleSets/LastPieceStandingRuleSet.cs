namespace ChessRogue.Core.RuleSets
{
    public class LastPieceStandingRuleSet : IRuleSet
    {
        public IEnumerable<Move> GetLegalMoves(GameState state, IPiece piece)
        {
            // In this variant, just rely on the piece’s pseudo-legal moves
            return piece.GetPseudoLegalMoves(state);
        }

        public bool IsGameOver(GameState state, out PlayerColor winner)
        {
            var whitePieces = state.Board.GetAllPieces(PlayerColor.White).ToList();
            var blackPieces = state.Board.GetAllPieces(PlayerColor.Black).ToList();

            if (!whitePieces.Any())
            {
                winner = PlayerColor.Black;
                return true;
            }

            if (!blackPieces.Any())
            {
                winner = PlayerColor.White;
                return true;
            }

            winner = default;
            return false;
        }
    }
}
