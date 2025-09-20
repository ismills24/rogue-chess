using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.WinConditions
{
    /// <summary>
    /// Defines a specific win condition for the game.
    /// </summary>
    public interface IWinCondition
    {
        /// <summary>
        /// Check if this win condition is met and determine the winner.
        /// </summary>
        /// <param name="state">Current game state</param>
        /// <param name="winner">Winner if condition is met, null if draw/stalemate</param>
        /// <returns>True if this win condition is met</returns>
        bool IsGameOver(GameState state, out PlayerColor? winner);
    }
}
