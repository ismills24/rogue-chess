using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.RuleSets
{
    /// <summary>
    /// Defines the rules for move legality and game termination.
    /// </summary>
    public interface IRuleSet
    {
        /// <summary>
        /// Get all legal moves for a piece in the current state.
        /// Filters pseudo-legal moves based on game rules (e.g., king safety).
        /// </summary>
        IEnumerable<Move> GetLegalMoves(GameState state, IPiece piece);

        /// <summary>
        /// Check if the game is over and determine the winner.
        /// </summary>
        /// <param name="state">Current game state</param>
        /// <param name="winner">Winner if game is over, null if draw/stalemate</param>
        /// <returns>True if game is over</returns>
        bool IsGameOver(GameState state, out PlayerColor? winner);
    }
}
