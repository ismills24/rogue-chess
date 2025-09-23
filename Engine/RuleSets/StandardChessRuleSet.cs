using System.Collections.Generic;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.WinConditions;

namespace RogueChess.Engine.RuleSets
{
    /// <summary>
    /// Standard chess ruleset with check/checkmate and king safety.
    /// </summary>
    public class StandardChessRuleSet : IRuleSet
    {
        private readonly CheckmateCondition checkmateCondition;

        public StandardChessRuleSet()
        {
            checkmateCondition = new CheckmateCondition();
        }

        public IEnumerable<Move> GetLegalMoves(GameState state, IPiece piece)
        {
            // Get all pseudo-legal moves from the piece
            foreach (var move in piece.GetPseudoLegalMoves(state))
            {
                // Check if the move would put the king in check
                if (!CheckRules.WouldMovePutKingInCheck(state, move, piece.Owner))
                {
                    yield return move;
                }
            }
        }

        public bool IsGameOver(GameState state, out PlayerColor? winner)
        {
            return checkmateCondition.IsGameOver(state, out winner);
        }
    }
}
