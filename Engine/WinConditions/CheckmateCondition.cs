using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;

namespace RogueChess.Engine.WinConditions
{
    /// <summary>
    /// Win condition for standard chess checkmate and stalemate.
    /// </summary>
    public class CheckmateCondition : IWinCondition
    {
        public bool IsGameOver(GameState state, out PlayerColor? winner)
        {
            var currentPlayer = state.CurrentPlayer;
            var opponent =
                currentPlayer == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;

            bool kingInCheck = CheckRules.IsKingInCheck(state, currentPlayer);

            // Get all legal moves for the current player
            var pieces = state.Board.GetAllPieces(currentPlayer);
            var hasLegalMoves = false;

            foreach (var piece in pieces)
            {
                var legalMoves = GetLegalMovesForPiece(state, piece);
                if (legalMoves.Any())
                {
                    hasLegalMoves = true;
                    break;
                }
            }

            if (kingInCheck && !hasLegalMoves)
            {
                winner = opponent; // Checkmate
                return true;
            }
            else if (!kingInCheck && !hasLegalMoves)
            {
                winner = null; // Stalemate (draw)
                return true;
            }

            winner = null;
            return false;
        }

        /// <summary>
        /// Get legal moves for a specific piece (helper method).
        /// </summary>
        private IEnumerable<Move> GetLegalMovesForPiece(GameState state, IPiece piece)
        {
            foreach (var move in piece.GetPseudoLegalMoves(state))
            {
                if (!CheckRules.WouldMovePutKingInCheck(state, move, piece.Owner))
                {
                    yield return move;
                }
            }
        }
    }
}



