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

// TODO : function to calculate must take in :
// - board state that has all pieces, the ruleset
// - extra win conditions params for win condition, for example more pts given to piece moving towards the 'hill' tiles in a king of the hill gamemode
// - max calculation time returns the best move found in the time limit
// -
// function return the best move found in the time limit, returns early if no move is found in the time limit
