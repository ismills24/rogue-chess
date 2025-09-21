using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;

namespace RogueChess.Engine.Controllers
{
    /// <summary>
    /// Human controller that prompts for input.
    /// For now, this is a placeholder that selects the first available move.
    /// In a real implementation, this would interface with the UI.
    /// </summary>
    public class HumanController : IPlayerController
    {
        private readonly IRuleSet _ruleset;

        public HumanController(IRuleSet ruleset)
        {
            _ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));
        }

        public Move? SelectMove(GameState state)
        {
            var currentPlayer = state.CurrentPlayer;
            var pieces = state.Board.GetAllPieces(currentPlayer).ToList();

            if (!pieces.Any())
                return null;

            // For now, just select the first available legal move
            // In a real implementation, this would prompt the user for input
            foreach (var piece in pieces)
            {
                var legalMoves = _ruleset.GetLegalMoves(state, piece).ToList();
                if (legalMoves.Any())
                {
                    return legalMoves.First();
                }
            }

            return null;
        }
    }
}
