// Engine/Controllers/GreedyAIController.cs
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;

namespace RogueChess.Engine.Controllers
{
    public class GreedyAIController : IPlayerController
    {
        private readonly IRuleSet _ruleset;
        private readonly int _depth;
        private readonly Random _rng = new();

        public GreedyAIController(IRuleSet ruleset, int depth = 5)
        {
            _ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));
            _depth = Math.Max(1, depth);
        }

        public Move? SelectMove(GameState state)
        {
            var moves = state.GetAllLegalMoves(_ruleset).ToList();
            if (moves.Count == 0)
                return null;

            int bestScore = int.MinValue;
            var bestMoves = new List<Move>();

            foreach (var move in moves)
            {
                var next = GameEngine.SimulateTurn(state, move, _ruleset); // ✅ direct call
                int score = -Negamax(next, _depth - 1, int.MinValue / 2, int.MaxValue / 2);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMoves.Clear();
                    bestMoves.Add(move);
                }
                else if (score == bestScore)
                {
                    bestMoves.Add(move);
                }
            }

            if (bestMoves.Count == 0)
                return null;
            return bestMoves[_rng.Next(bestMoves.Count)];
        }

        // Standard negamax with alpha-beta
        private int Negamax(GameState node, int depth, int alpha, int beta)
        {
            // Base eval is always “white POV”
            if (depth == 0 || IsTerminal(node))
                return EvalFromSideToMove(node);

            int value = int.MinValue;
            foreach (var move in node.GetAllLegalMoves(_ruleset))
            {
                var child = GameEngine.SimulateTurn(node, move, _ruleset);

                int score = -Negamax(child, depth - 1, -beta, -alpha);
                if (score > value)
                    value = score;
                if (value > alpha)
                    alpha = value;
                if (alpha >= beta)
                    break; // alpha-beta cutoff
            }

            // No legal moves (terminal); return leaf eval
            if (value == int.MinValue)
                return EvalFromSideToMove(node);

            return value;
        }

        private bool IsTerminal(GameState s) => _ruleset.IsGameOver(s, out _);

        // Convert white-POV eval into “side-to-move POV” (negamax canonical form)
        private int EvalFromSideToMove(GameState s)
        {
            int baseEval = s.Evaluate(); // + for White, - for Black
            return s.CurrentPlayer == PlayerColor.White ? baseEval : -baseEval;
        }

        // Dummy controllers for the simulation engine (never used interactively)
        private static IPlayerController Dummy() => new NoopController();

        private sealed class NoopController : IPlayerController
        {
            public Move? SelectMove(GameState state) => null;
        }
    }
}
