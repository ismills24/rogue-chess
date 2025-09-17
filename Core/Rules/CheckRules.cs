namespace ChessRogue.Core.Rules
{
    public static class CheckRules
    {
        public static bool IsKingInCheck(GameState state, PlayerColor kingColor)
        {
            var king = state.Board.GetAllPieces(kingColor).FirstOrDefault(p => p is King);
            if (king == null)
                return true;

            var opponent = kingColor == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;
            foreach (var piece in state.Board.GetAllPieces(opponent))
            {
                if (piece.GetPseudoLegalMoves(state).Any(m => m.To == king.Position))
                    return true;
            }
            return false;
        }
    }
}
