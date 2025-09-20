using RogueChess.Engine;
using RogueChess.Engine.Controllers;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;

namespace RogueChess.UI
{
    /// <summary>
    /// Human controller that works with the new Engine architecture.
    /// Handles move selection through UI interaction.
    /// </summary>
    public class EngineHumanController : IPlayerController
    {
        private readonly IRuleSet _ruleset;

        private TaskCompletionSource<Move?>? _tcs;
        private Move? _queuedMove;
        private readonly object _lock = new();

        public EngineHumanController(IRuleSet ruleset) => _ruleset = ruleset;

        public Move? SelectMove(GameState state)
        {
            lock (_lock)
            {
                if (_queuedMove is Move m)
                {
                    _queuedMove = null;
                    return m;
                }
                _tcs = new TaskCompletionSource<Move?>();
            }

            return _tcs.Task.Result; // blocks game loop thread until SubmitMove/CancelPending
        }

        public void SubmitMove(Move? move)
        {
            lock (_lock)
            {
                if (_tcs is { Task.IsCompleted: false })
                {
                    _tcs.TrySetResult(move);
                }
                else
                {
                    _queuedMove = move; // user clicked before SelectMove() started waiting
                }
            }
        }

        /// <summary>
        /// Unblocks SelectMove() if it's currently waiting.
        /// Called when restarting/disposing a game so the old loop can exit.
        /// </summary>
        public void CancelPending()
        {
            lock (_lock)
            {
                _tcs?.TrySetResult(null);
                _queuedMove = null;
            }
        }

        /// <summary>
        /// Get all legal moves for a piece at the given position.
        /// Used by the UI to highlight legal moves.
        /// </summary>
        public IEnumerable<Move> GetLegalMoves(GameState state, Vector2Int position)
        {
            var piece = state.Board.GetPieceAt(position);
            if (piece == null)
                return Enumerable.Empty<Move>();

            return _ruleset.GetLegalMoves(state, piece);
        }
    }
}
