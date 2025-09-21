using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Controllers
{
    /// <summary>
    /// Interface for player controllers that select moves.
    /// </summary>
    public interface IPlayerController
    {
        /// <summary>
        /// Select a move for the current player.
        /// </summary>
        /// <param name="state">Current game state</param>
        /// <returns>Selected move, or null if no move available</returns>
        Move? SelectMove(GameState state);
    }
}
