using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.RuleSets
{
    /// <summary>
    /// Simple ruleset where the game ends when only one player has pieces remaining.
    /// No check/checkmate rules - just survival.
    /// </summary>
    public class LastPieceStandingRuleSet : IRuleSet
    {
        public IEnumerable<Move> GetLegalMoves(GameState state, IPiece piece)
        {
            // All pseudo-legal moves are legal in this ruleset
            return piece.GetPseudoLegalMoves(state);
        }

        public bool IsGameOver(GameState state, out PlayerColor? winner)
        {
            var whitePieces = state.Board.GetAllPieces(PlayerColor.White).ToList();
            var blackPieces = state.Board.GetAllPieces(PlayerColor.Black).ToList();

            if (whitePieces.Count == 0 && blackPieces.Count == 0)
            {
                winner = null; // Draw - no pieces left
                return true;
            }
            else if (whitePieces.Count == 0)
            {
                winner = PlayerColor.Black; // Black wins
                return true;
            }
            else if (blackPieces.Count == 0)
            {
                winner = PlayerColor.White; // White wins
                return true;
            }

            winner = null;
            return false;
        }
    }
}
