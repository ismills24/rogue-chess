using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;
using RogueChess.Engine.GameModes.PiecePlacementInit;

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
        /// Get the piece placement configuration for this game mode
        /// </summary>
        RogueChess.Engine.GameModes.PiecePlacementInit.PiecePlacementInit GetPiecePlacementInit();

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
