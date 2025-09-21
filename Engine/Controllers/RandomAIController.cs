using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;

namespace RogueChess.Engine.Controllers
{
    /// <summary>
    /// AI controller that selects random legal moves.
    /// </summary>
    public class RandomAIController : IPlayerController
    {
        private readonly IRuleSet _ruleset;
        private readonly Random _random;

        public RandomAIController(IRuleSet ruleset)
        {
            _ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));
            _random = new Random();
        }

        public Move? SelectMove(GameState state)
        {
            var currentPlayer = state.CurrentPlayer;
            var pieces = state.Board.GetAllPieces(currentPlayer).ToList();

            if (!pieces.Any())
                return null;

            // Get all legal moves for all pieces
            var allLegalMoves = new List<Move>();
            foreach (var piece in pieces)
            {
                var legalMoves = _ruleset.GetLegalMoves(state, piece).ToList();
                allLegalMoves.AddRange(legalMoves);
            }

            if (!allLegalMoves.Any())
                return null;

            // Select a random legal move
            var randomIndex = _random.Next(allLegalMoves.Count);
            return allLegalMoves[randomIndex];
        }
    }
}
