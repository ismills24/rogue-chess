using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;

namespace RogueChess.Engine.GameModes
{
    /// <summary>
    /// Interface for different game modes (Standard Chess, Random Chess, etc.)
    /// </summary>
    public interface IGameMode
    {
        string Name { get; }
        string Description { get; }

        /// <summary>
        /// Set up the initial board with pieces and tiles for this game mode
        /// </summary>
        IBoard SetupBoard();

        /// <summary>
        /// Get the ruleset for this game mode
        /// </summary>
        IRuleSet GetRuleSet();
    }
}
