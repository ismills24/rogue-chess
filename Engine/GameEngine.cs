using RogueChess.Engine.Controllers;
using RogueChess.Engine.Events;
using RogueChess.Engine.Hooks;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces.Decorators;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;
using RogueChess.Engine.StatusEffects;

namespace RogueChess.Engine
{
    /// <summary>
    /// Central orchestrator for the game engine.
    /// Manages the candidate → hooks → commit → canonical pipeline.
    /// Maintains history and provides undo/redo functionality.
    /// </summary>
    public class GameEngine
    {
        private readonly List<(GameEvent Event, GameState State)> _history;
        private int _currentIndex;
        private readonly IRuleSet _ruleset;
        private readonly IPlayerController _whiteController;
        private readonly IPlayerController _blackController;

        public GameEngine(
            GameState initialState,
            IPlayerController whiteController,
            IPlayerController blackController,
            IRuleSet ruleset
        )
        {
            _history = new List<(GameEvent, GameState)>();
            _currentIndex = -1;
            _ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));
            _whiteController =
                whiteController ?? throw new ArgumentNullException(nameof(whiteController));
            _blackController =
                blackController ?? throw new ArgumentNullException(nameof(blackController));

            // Add initial state to history (White to move, Turn 1)
            var initialEvent = new GameEvent(
                Guid.NewGuid(),
                GameEventType.TurnAdvanced,
                false,
                null,
                new TurnAdvancedPayload(PlayerColor.White, 1)
            );
            _history.Add((initialEvent, initialState));
            _currentIndex = 0;
        }

        public GameState CurrentState => _history[_currentIndex].State;
        public int CurrentIndex => _currentIndex;
        public int HistoryCount => _history.Count;
        public IReadOnlyList<(GameEvent Event, GameState State)> History => _history.AsReadOnly();

        public event Action<GameEvent>? OnEventPublished;

        /// <summary>
        /// Run one complete turn: TurnStart → Select/Process Move → TurnEnd → Advance Turn.
        /// </summary>
        public void RunTurn()
        {
            if (IsGameOver())
                return;

            // Turn start effects for the CURRENT player
            foreach (var candidateEvent in TickTurnStart(CurrentState))
                Commit(candidateEvent);

            // Get move from the CURRENT player
            var controller =
                CurrentState.CurrentPlayer == PlayerColor.White
                    ? _whiteController
                    : _blackController;

            var move = controller.SelectMove(CurrentState);
            if (move != null)
            {
                // Process the move (no advancing or end-of-turn here)
                ProcessMove(move, CurrentState);
            }

            // End-of-turn effects for the SAME player who just took the turn
            foreach (var ev in TickTurnEnd(CurrentState))
                Commit(ev);

            // Finally, advance the turn to the other player
            var afterEndState = CurrentState;
            var nextPlayer =
                afterEndState.CurrentPlayer == PlayerColor.White
                    ? PlayerColor.Black
                    : PlayerColor.White;

            var turnAdvancedEvent = new CandidateEvent(
                GameEventType.TurnAdvanced,
                false,
                new TurnAdvancedPayload(nextPlayer, afterEndState.TurnNumber + 1)
            );
            Commit(turnAdvancedEvent);
        }

        public void ProcessMove(Move move) => ProcessMove(move, CurrentState);

        /// <summary>
        /// Apply the consequences of a single move (captures, movement, tiles, piece hooks).
        /// NOTE: No end-of-turn and no turn-advance in here.
        /// </summary>
        private void ProcessMove(Move move, GameState currentState)
        {
            var piece = currentState.Board.GetPieceAt(move.From);
            if (piece == null)
                return;

            // 1) Capture (emit PieceCaptured and target's OnCapture)
            var capturedPiece = currentState.Board.GetPieceAt(move.To);
            if (capturedPiece != null)
            {
                var captureEvent = new CandidateEvent(
                    GameEventType.PieceCaptured,
                    true,
                    new CapturePayload(capturedPiece)
                );

                var result = Commit(captureEvent);
                if (result == null)
                    return; // cancelled by a hook (e.g., guardian nullifies)
                if (result.Type == GameEventType.MoveCancelled)
                    return; // martyr/guardian cancelled the move

                foreach (var ev in capturedPiece.OnCapture(CurrentState))
                {
                    var extra = Commit(ev);
                    if (extra == null || extra.Type == GameEventType.MoveCancelled)
                        return;
                }
            }

            // 2) Apply the move
            var moveEvent = new CandidateEvent(
                GameEventType.MoveApplied,
                true,
                new MovePayload(piece, move.From, move.To)
            );

            var moveResult = Commit(moveEvent);
            if (moveResult == null || moveResult.Type == GameEventType.MoveCancelled)
                return;

            // 3) Piece-specific post-move effects
            foreach (var ev in piece.OnMove(move, CurrentState))
            {
                var extra = Commit(ev);
                if (extra == null || extra.Type == GameEventType.MoveCancelled)
                    return;
            }

            // 4) Tile entry effects (on the destination tile)
            var destinationTile = CurrentState.Board.GetTile(move.To);
            foreach (var ev in destinationTile.OnEnter(piece, move.To, CurrentState))
            {
                var extra = Commit(ev);
                if (extra == null || extra.Type == GameEventType.MoveCancelled)
                    return;
            }

            // 5) Optional: immediate "mid-turn" ticks if your design needs them.
            // In our model, we keep start and end ticks only, so nothing here.
        }

        /// <summary>
        /// Tick turn-start effects for all pieces and tiles of the current player.
        /// </summary>
        private IEnumerable<CandidateEvent> TickTurnStart(GameState state)
        {
            var currentPlayer = state.CurrentPlayer;

            // Tiles tick
            foreach (var piece in state.Board.GetAllPieces(currentPlayer))
            {
                var tile = state.Board.GetTile(piece.Position);
                foreach (var ev in tile.OnTurnStart(piece, piece.Position, state))
                    yield return ev;
            }

            // Pieces tick
            foreach (var piece in state.Board.GetAllPieces(currentPlayer))
            {
                foreach (var ev in piece.OnTurnStart(state))
                    yield return ev;
            }
        }

        /// <summary>
        /// Tick end-of-turn effects for all pieces of the current player.
        /// </summary>
        private IEnumerable<CandidateEvent> TickTurnEnd(GameState state)
        {
            foreach (var piece in state.Board.GetAllPieces(state.CurrentPlayer))
            {
                foreach (var ev in piece.OnTurnEnd(state))
                    yield return ev;
            }
        }

        /// <summary>
        /// Commit a candidate event through hooks into a canonical event.
        /// </summary>
        public GameEvent? Commit(CandidateEvent candidate, Guid? parentEventId = null)
        {
            var currentState = CurrentState;
            var pending = new List<CandidateEvent> { candidate };

            // Run hooks; each hook can cancel or replace events
            foreach (var hook in HookCollector.CollectHooks(currentState))
            {
                var nextPending = new List<CandidateEvent>();

                foreach (var cand in pending)
                {
                    var result = hook.BeforeEvent(cand, currentState);

                    if (result == null)
                    {
                        // Entirely cancelled
                        continue;
                    }

                    var asList = result.ToList();
                    if (asList.Count == 0)
                    {
                        // Keep original
                        nextPending.Add(cand);
                    }
                    else
                    {
                        // Replace with hook output
                        nextPending.AddRange(asList);
                    }
                }

                pending = nextPending;
                if (pending.Count == 0)
                    return null; // everything cancelled
            }

            GameEvent? lastCanonical = null;

            // Apply each resulting candidate in order
            foreach (var cand in pending)
            {
                var canonical = new GameEvent(
                    Guid.NewGuid(),
                    cand.Type,
                    cand.IsPlayerAction,
                    parentEventId,
                    cand.Payload
                );

                var newState = ApplyEventToState(canonical, currentState);

                // Trim redo branch if needed
                if (_currentIndex < _history.Count - 1)
                    _history.RemoveRange(_currentIndex + 1, _history.Count - (_currentIndex + 1));

                _history.Add((canonical, newState));
                _currentIndex++;

                OnEventPublished?.Invoke(canonical);
                lastCanonical = canonical;
            }

            return lastCanonical;
        }

        /// <summary>
        /// Apply a canonical event to produce a new GameState.
        /// Always resolves pieces from the cloned board, not from payload refs.
        /// </summary>
        private GameState ApplyEventToState(GameEvent gameEvent, GameState currentState)
        {
            var newBoard = currentState.Board.Clone();
            var newCurrentPlayer = currentState.CurrentPlayer;
            var newTurnNumber = currentState.TurnNumber;

            switch (gameEvent.Type)
            {
                case GameEventType.MoveApplied:
                    if (gameEvent.Payload is MovePayload movePayload)
                    {
                        var pieceOnBoard = newBoard.GetPieceAt(movePayload.From);
                        if (pieceOnBoard != null)
                            newBoard.MovePiece(movePayload.From, movePayload.To);
                    }
                    break;

                case GameEventType.PieceCaptured:
                    if (gameEvent.Payload is CapturePayload capturePayload)
                    {
                        var pos = capturePayload.Target.Position;
                        if (newBoard.GetPieceAt(pos) != null)
                            newBoard.RemovePiece(pos);
                    }
                    break;

                case GameEventType.TurnAdvanced:
                    if (gameEvent.Payload is TurnAdvancedPayload turnPayload)
                    {
                        newCurrentPlayer = turnPayload.NewPlayer;
                        newTurnNumber = turnPayload.TurnNumber;
                    }
                    break;

                case GameEventType.TileEffectTriggered:
                    if (gameEvent.Payload is TileChangePayload tilePayload)
                    {
                        newBoard.SetTile(tilePayload.Position, tilePayload.NewTile);
                    }
                    else if (gameEvent.Payload is ForcedSlidePayload slidePayload)
                    {
                        var p = newBoard.GetPieceAt(slidePayload.From);
                        if (p != null)
                            newBoard.MovePiece(slidePayload.From, slidePayload.To);
                    }
                    break;

                case GameEventType.StatusEffectTriggered:
                    if (gameEvent.Payload is StatusApplyPayload statusPayload)
                    {
                        var targetPos = statusPayload.Target.Position;
                        var pieceAtPosition = newBoard.GetPieceAt(targetPos);
                        if (pieceAtPosition != null)
                        {
                            if (pieceAtPosition is StatusEffectDecorator existingDecorator)
                            {
                                existingDecorator.AddStatus(statusPayload.Effect);
                            }
                            else
                            {
                                var statusDecorator = new StatusEffectDecorator(pieceAtPosition);
                                statusDecorator.AddStatus(statusPayload.Effect);
                                newBoard.RemovePiece(targetPos);
                                newBoard.PlacePiece(statusDecorator, targetPos);
                            }
                        }
                    }
                    else if (gameEvent.Payload is PieceDestroyedPayload destroyedPayload)
                    {
                        var pos = destroyedPayload.Piece.Position;
                        if (newBoard.GetPieceAt(pos) != null)
                            newBoard.RemovePiece(pos);
                    }
                    // StatusTickPayload = no board mutation
                    break;

                case GameEventType.PieceDestroyed:
                    if (gameEvent.Payload is PieceDestroyedPayload destroyedPayload2)
                    {
                        var pos = destroyedPayload2.Piece.Position;
                        if (newBoard.GetPieceAt(pos) != null)
                            newBoard.RemovePiece(pos);
                    }
                    break;

                case GameEventType.StatusTick:
                    // no board mutation
                    break;

                case GameEventType.MoveCancelled:
                    // no board mutation; attacker remains at From
                    break;
            }

            return new GameState(newBoard, newCurrentPlayer, newTurnNumber);
        }

        public bool IsGameOver() => _ruleset.IsGameOver(CurrentState, out _);

        public PlayerColor? GetWinner()
        {
            if (_ruleset.IsGameOver(CurrentState, out var winner))
                return winner;
            return null;
        }

        public void Undo()
        {
            if (_currentIndex > 0)
                _currentIndex--;
        }

        public void Redo()
        {
            if (_currentIndex < _history.Count - 1)
                _currentIndex++;
        }

        public void JumpTo(int index)
        {
            if (index >= 0 && index < _history.Count)
                _currentIndex = index;
        }
    }

    /// <summary>
    /// Payload for turn advancement events.
    /// </summary>
    public record TurnAdvancedPayload(PlayerColor NewPlayer, int TurnNumber);
}
