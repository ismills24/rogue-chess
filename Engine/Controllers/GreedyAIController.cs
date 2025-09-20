using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;

namespace RogueChess.Engine.Controllers
{
    /// <summary>
    /// Depth-1 greedy AI: evaluates all legal moves and picks the one
    /// that maximizes the piece value difference for the current player.
    /// </summary>
    public class GreedyAIController : IPlayerController
    {
        private readonly IRuleSet _ruleset;

        public GreedyAIController(IRuleSet ruleset)
        {
            _ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));
        }

        public Move? SelectMove(GameState state)
        {
            var currentPlayer = state.CurrentPlayer;
            var pieces = state.Board.GetAllPieces(currentPlayer).ToList();

            if (!pieces.Any())
                return null;

            // Collect all legal moves
            var allLegalMoves = new List<Move>();
            foreach (var piece in pieces)
            {
                var legalMoves = _ruleset.GetLegalMoves(state, piece).ToList();
                allLegalMoves.AddRange(legalMoves);
            }

            if (!allLegalMoves.Any())
                return null;

            Move? bestMove = null;
            int bestScore = int.MinValue;

            foreach (var move in allLegalMoves)
            {
                // Simulate the move by cloning state and applying the move
                var clonedState = state.Clone(); // You’ll need Clone() on GameState
                var engine = new GameEngine(
                    clonedState,
                    new NullController(), // dummy controllers, won’t be used
                    new NullController(),
                    _ruleset
                );

                engine.ProcessMove(move);

                var eval = EvaluateBoard(engine.CurrentState, currentPlayer);

                if (eval > bestScore)
                {
                    bestScore = eval;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private int EvaluateBoard(GameState state, PlayerColor player)
        {
            int whiteScore = state
                .Board.GetAllPieces(PlayerColor.White)
                .Sum(p => PieceValueCalculator.GetTotalValue(p));

            int blackScore = state
                .Board.GetAllPieces(PlayerColor.Black)
                .Sum(p => PieceValueCalculator.GetTotalValue(p));

            return player == PlayerColor.White ? whiteScore - blackScore : blackScore - whiteScore;
        }
    }

    /// <summary>
    /// Dummy controller that never selects a move.
    /// Used when simulating moves inside the AI.
    /// </summary>
    public class NullController : IPlayerController
    {
        public Move? SelectMove(GameState state) => null;
    }
}
