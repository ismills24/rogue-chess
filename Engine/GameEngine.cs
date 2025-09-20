using RogueChess.Engine.Controllers;
using RogueChess.Engine.Events;
using RogueChess.Engine.Hooks;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
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
            IRuleSet ruleset)
        {
            _history = new List<(GameEvent, GameState)>();
            _currentIndex = -1;
            _ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));
            _whiteController = whiteController ?? throw new ArgumentNullException(nameof(whiteController));
            _blackController = blackController ?? throw new ArgumentNullException(nameof(blackController));

            // Add initial state to history
            var initialEvent = new GameEvent(
                Guid.NewGuid(),
                GameEventType.TurnAdvanced,
                false, // Not a player action
                null, // No parent
                new TurnAdvancedPayload(PlayerColor.White, 1)
            );
            _history.Add((initialEvent, initialState));
            _currentIndex = 0;
        }

        /// <summary>
        /// Current game state.
        /// </summary>
        public GameState CurrentState => _history[_currentIndex].State;

        /// <summary>
        /// Current history index.
        /// </summary>
        public int CurrentIndex => _currentIndex;

        /// <summary>
        /// Get the current history count.
        /// </summary>
        public int HistoryCount => _history.Count;

        /// <summary>
        /// Event published when a canonical event is created.
        /// </summary>
        public event Action<GameEvent>? OnEventPublished;

        /// <summary>
        /// Run one complete turn.
        /// </summary>
        public void RunTurn()
        {
            if (IsGameOver())
                return;

            var currentState = CurrentState;
            var currentPlayer = currentState.CurrentPlayer;

            // 1. Tick turn start effects
            var turnStartEvents = TickTurnStart(currentState).ToList();
            foreach (var candidateEvent in turnStartEvents)
            {
                Commit(candidateEvent);
            }

            // 2. Get player move
            var controller = currentPlayer == PlayerColor.White ? _whiteController : _blackController;
            var move = controller.SelectMove(currentState);

            if (move == null)
            {
                // Player couldn't make a move - this might be checkmate or stalemate
                return;
            }

            // 3. Process the move
            ProcessMove(move, currentState);
        }

        /// <summary>
        /// Process a move by generating candidate events and committing them.
        /// </summary>
        private void ProcessMove(Move move, GameState currentState)
        {
            var piece = currentState.Board.GetPieceAt(move.From);
            if (piece == null)
                return;

            // Generate candidate events from the move
            var candidateEvents = new List<CandidateEvent>();

            // 1. Capture event (if applicable)
            var capturedPiece = currentState.Board.GetPieceAt(move.To);
            if (capturedPiece != null)
            {
                candidateEvents.Add(new CandidateEvent(
                    GameEventType.PieceCaptured,
                    true, // Player action
                    new CapturePayload(capturedPiece)
                ));
            }

            // 2. Move event
            candidateEvents.Add(new CandidateEvent(
                GameEventType.MoveApplied,
                true, // Player action
                new MovePayload(piece, move.From, move.To)
            ));

            // 3. Piece-specific events
            foreach (var ev in piece.OnMove(move, currentState))
            {
                candidateEvents.Add(ev);
            }

            // 4. Tile entry events
            var destinationTile = currentState.Board.GetTile(move.To);
            foreach (var ev in destinationTile.OnEnter(piece, move.To, currentState))
            {
                candidateEvents.Add(ev);
            }

            // Commit all candidate events
            foreach (var candidateEvent in candidateEvents)
            {
                Commit(candidateEvent);
            }

            // Advance turn
            var turnAdvancedEvent = new CandidateEvent(
                GameEventType.TurnAdvanced,
                false, // Not a player action
                new TurnAdvancedPayload(
                    currentState.CurrentPlayer == PlayerColor.White ? PlayerColor.Black : PlayerColor.White,
                    currentState.TurnNumber + 1
                )
            );
            Commit(turnAdvancedEvent);
        }

        /// <summary>
        /// Tick turn start effects for all pieces and tiles.
        /// </summary>
        private IEnumerable<CandidateEvent> TickTurnStart(GameState state)
        {
            var currentPlayer = state.CurrentPlayer;

            // Tick pieces
            foreach (var piece in state.Board.GetAllPieces(currentPlayer))
            {
                foreach (var ev in piece.OnMove(new Move(piece.Position, piece.Position, piece), state))
                {
                    yield return ev;
                }
            }

            // Tick tiles
            foreach (var piece in state.Board.GetAllPieces(currentPlayer))
            {
                var tile = state.Board.GetTile(piece.Position);
                foreach (var ev in tile.OnTurnStart(piece, piece.Position, state))
                {
                    yield return ev;
                }
            }
        }

        /// <summary>
        /// Commit a candidate event through the hook pipeline.
        /// </summary>
        public GameEvent? Commit(CandidateEvent candidate, Guid? parentEventId = null)
        {
            var currentState = CurrentState;
            var processedCandidate = candidate;

            // 1. Pass through global hooks
            var hooks = HookCollector.CollectHooks(currentState);
            foreach (var hook in hooks)
            {
                var hookResult = hook.BeforeEvent(processedCandidate, currentState);
                if (hookResult == null)
                {
                    // Event was cancelled by a hook
                    return null;
                }
                processedCandidate = hookResult;
            }

            // 2. Create canonical event
            var canonicalEvent = new GameEvent(
                Guid.NewGuid(),
                processedCandidate.Type,
                processedCandidate.IsPlayerAction,
                parentEventId,
                processedCandidate.Payload
            );

            // 3. Apply the event to create new state
            var newState = ApplyEventToState(canonicalEvent, currentState);

            // 4. Add to history
            _history.Add((canonicalEvent, newState));
            _currentIndex++;

            // 5. Publish the event
            OnEventPublished?.Invoke(canonicalEvent);

            return canonicalEvent;
        }

        /// <summary>
        /// Apply a canonical event to create a new game state.
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
                        newBoard.MovePiece(movePayload.From, movePayload.To);
                        movePayload.Piece.Position = movePayload.To;
                    }
                    break;

                case GameEventType.PieceCaptured:
                    if (gameEvent.Payload is CapturePayload capturePayload)
                    {
                        newBoard.RemovePiece(capturePayload.Target.Position);
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
                        newBoard.MovePiece(slidePayload.From, slidePayload.To);
                        slidePayload.Piece.Position = slidePayload.To;
                    }
                    break;

                case GameEventType.StatusEffectTriggered:
                    if (gameEvent.Payload is StatusApplyPayload statusPayload)
                    {
                        // Apply status effect to the piece
                        // For now, we'll handle this by wrapping the piece with a StatusEffectDecorator
                        // In a more sophisticated implementation, we might have a different approach
                        var piece = statusPayload.Target;
                        var statusEffect = statusPayload.Effect;
                        
                        // Find the piece on the board and wrap it with StatusEffectDecorator
                        var pieceAtPosition = newBoard.GetPieceAt(piece.Position);
                        if (pieceAtPosition != null)
                        {
                            var statusDecorator = new StatusEffectDecorator(pieceAtPosition);
                            if (statusEffect is BurningStatus burningStatus)
                            {
                                statusDecorator.AddStatus(burningStatus);
                            }
                            newBoard.RemovePiece(piece.Position);
                            newBoard.PlacePiece(statusDecorator, piece.Position);
                        }
                    }
                    break;
            }

            return new GameState(newBoard, newCurrentPlayer, newTurnNumber);
        }

        /// <summary>
        /// Check if the game is over.
        /// </summary>
        public bool IsGameOver()
        {
            return _ruleset.IsGameOver(CurrentState, out _);
        }

        /// <summary>
        /// Get the winner if the game is over.
        /// </summary>
        public PlayerColor? GetWinner()
        {
            if (_ruleset.IsGameOver(CurrentState, out var winner))
            {
                return winner;
            }
            return null;
        }

        /// <summary>
        /// Undo the last move.
        /// </summary>
        public void Undo()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
            }
        }

        /// <summary>
        /// Redo the next move.
        /// </summary>
        public void Redo()
        {
            if (_currentIndex < _history.Count - 1)
            {
                _currentIndex++;
            }
        }

        /// <summary>
        /// Jump to a specific point in history.
        /// </summary>
        public void JumpTo(int index)
        {
            if (index >= 0 && index < _history.Count)
            {
                _currentIndex = index;
            }
        }
    }

    /// <summary>
    /// Payload for turn advancement events.
    /// </summary>
    public record TurnAdvancedPayload(PlayerColor NewPlayer, int TurnNumber);
}
