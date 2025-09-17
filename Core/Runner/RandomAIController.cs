using System;
using System.Linq;
using ChessRogue.Core.RuleSets;

namespace ChessRogue.Core.Runner
{
    public class RandomAIController : IPlayerController
    {
        private readonly Random random = new();
        private readonly IRuleSet ruleset;

        public RandomAIController(IRuleSet ruleset)
        {
            this.ruleset = ruleset;
        }

        public Move SelectMove(GameState state)
        {
            var pieces = state.Board.GetAllPieces(state.CurrentPlayer);
            var legalMoves = pieces.SelectMany(p => ruleset.GetLegalMoves(state, p)).ToList();

            if (legalMoves.Count == 0)
                return null;

            return legalMoves[random.Next(legalMoves.Count)];
        }
    }
}
