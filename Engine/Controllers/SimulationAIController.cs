using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;

namespace RogueChess.Engine.Controllers
{
    /// <summary>
    /// AI controller that uses simulation to evaluate moves.
    /// This demonstrates how AI developers can use the simulation API
    /// to build sophisticated chess engines.
    /// </summary>
    public class SimulationAIController : IPlayerController
    {
        private readonly IRuleSet _ruleset;
        private readonly int _searchDepth;
        private readonly Random _random;

        public SimulationAIController(IRuleSet ruleset, int searchDepth = 2)
        {
            _ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));
            _searchDepth = searchDepth;
            _random = new Random();
        }

        public Move? SelectMove(GameState state)
        {
            var legalMoves = state.GetAllLegalMoves(_ruleset).ToList();
            if (!legalMoves.Any())
                return null;

            // For demonstration, we'll use a simple minimax approach
            // In a real implementation, this would be much more sophisticated
            var bestMove = legalMoves[0];
            var bestScore = int.MinValue;

            foreach (var move in legalMoves)
            {
                var score = EvaluateMove(state, move, _searchDepth, true);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        /// <summary>
        /// Evaluate a move using minimax with the specified depth.
        /// This is a simplified implementation for demonstration.
        /// </summary>
        private int EvaluateMove(GameState state, Move move, int depth, bool maximizingPlayer)
        {
            // Simulate the move
            var simulatedState = state.Simulate(move, _ruleset);
            if (simulatedState == null)
                return maximizingPlayer ? int.MinValue : int.MaxValue;

            // Base case: if we've reached the search depth or game is over
            if (depth == 0 || _ruleset.IsGameOver(simulatedState, out _))
            {
                return simulatedState.Evaluate();
            }

            // Recursive case: evaluate opponent's best response
            var opponentMoves = simulatedState.GetAllLegalMoves(_ruleset).ToList();
            if (!opponentMoves.Any())
            {
                // Opponent has no moves - this is checkmate or stalemate
                return _ruleset.IsGameOver(simulatedState, out var winner) && winner == state.CurrentPlayer
                    ? int.MaxValue - depth // Checkmate in our favor
                    : 0; // Stalemate
            }

            var bestScore = maximizingPlayer ? int.MinValue : int.MaxValue;
            foreach (var opponentMove in opponentMoves)
            {
                var score = EvaluateMove(simulatedState, opponentMove, depth - 1, !maximizingPlayer);
                if (maximizingPlayer)
                {
                    bestScore = Math.Max(bestScore, score);
                }
                else
                {
                    bestScore = Math.Min(bestScore, score);
                }
            }

            return bestScore;
        }

        /// <summary>
        /// Get a quick evaluation of the current position.
        /// This can be used for debugging or quick assessments.
        /// </summary>
        public int QuickEvaluate(GameState state)
        {
            return state.Evaluate();
        }

        /// <summary>
        /// Get all possible moves with their evaluations.
        /// This is useful for debugging and analysis.
        /// </summary>
        public IEnumerable<(Move Move, int Score)> EvaluateAllMoves(GameState state)
        {
            var legalMoves = state.GetAllLegalMoves(_ruleset);
            foreach (var move in legalMoves)
            {
                var simulatedState = state.Simulate(move, _ruleset);
                var score = simulatedState?.Evaluate() ?? int.MinValue;
                yield return (move, score);
            }
        }
    }
}
