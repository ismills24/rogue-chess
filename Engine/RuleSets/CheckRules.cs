using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.RuleSets
{
    /// <summary>
    /// Helper class for checking chess rules like check and checkmate.
    /// </summary>
    public static class CheckRules
    {
        /// <summary>
        /// Check if the king of the given color is in check.
        /// </summary>
        public static bool IsKingInCheck(GameState state, PlayerColor kingColor)
        {
            var king = state.Board.GetAllPieces(kingColor).FirstOrDefault(p => p is King);

            if (king == null)
                return true; // No king = already dead = checkmate

            var opponent = kingColor == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;
            var opponentPieces = state.Board.GetAllPieces(opponent);

            foreach (var piece in opponentPieces)
            {
                if (piece is Pawn pawn)
                {
                    // Check pawn diagonal attacks
                    var direction = pawn.Owner == PlayerColor.White ? 1 : -1;
                    var attacks = new[]
                    {
                        new Vector2Int(pawn.Position.X - 1, pawn.Position.Y + direction),
                        new Vector2Int(pawn.Position.X + 1, pawn.Position.Y + direction),
                    };

                    if (attacks.Any(a => a == king.Position))
                        return true;
                }
                else
                {
                    // Check if any pseudo-legal move attacks the king
                    if (piece.GetPseudoLegalMoves(state).Any(m => m.To == king.Position))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if a move would put the moving player's king in check.
        /// </summary>
        public static bool WouldMovePutKingInCheck(
            GameState state,
            Move move,
            PlayerColor movingPlayer
        )
        {
            // Create a clone of the state and apply the move
            var clonedState = state.Clone();

            // Apply the move to the cloned board
            var piece = clonedState.Board.GetPieceAt(move.From);
            if (piece == null)
                return true; // Invalid move

            // Remove piece from original position
            clonedState.Board.RemovePiece(move.From);

            // Handle captures
            var capturedPiece = clonedState.Board.GetPieceAt(move.To);
            if (capturedPiece != null)
            {
                clonedState.Board.RemovePiece(move.To);
            }

            // Place piece at new position
            clonedState.Board.PlacePiece(piece, move.To);

            // Check if king is in check after the move
            return IsKingInCheck(clonedState, movingPlayer);
        }
    }
}



