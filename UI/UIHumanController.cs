using ChessRogue.Core;
using ChessRogue.Core.RuleSets;
using ChessRogue.Core.Runner;

namespace RogueChess.UI;

public class UIHumanController : IPlayerController
{
    private readonly IRuleSet _ruleset;

    private TaskCompletionSource<Move?>? _tcs;
    private Move? _queuedMove;
    private readonly object _lock = new();

    public UIHumanController(IRuleSet ruleset) => _ruleset = ruleset;

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
            _queuedMove = null;
            _tcs?.TrySetResult(null);
            _tcs = null;
        }
    }
}
