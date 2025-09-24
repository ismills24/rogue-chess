// Engine/GameEngine/GameEngine.cs
using System;
using System.Collections.Generic;
using RogueChess.Engine.Controllers;
using RogueChess.Engine.Events;
using RogueChess.Engine.Pieces.Decorators;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;

namespace RogueChess.Engine
{
    /// <summary>
    /// Central orchestrator for the game engine.
    /// Manages canonical pipeline and history.
    /// Partial type; see EventPipeline partial for Dispatch().
    /// </summary>
    public partial class GameEngine
    {
        private readonly List<(GameEvent Event, GameState State)> _history =
            new List<(GameEvent Event, GameState State)>();

        private int _currentIndex = -1;

        private readonly IRuleSet _ruleset;
        private readonly IPlayerController _whiteController;
        private readonly IPlayerController _blackController;

        public event Action<GameEvent>? OnEventPublished;

        public GameEngine(
            GameState initialState,
            IPlayerController whiteController,
            IPlayerController blackController,
            IRuleSet ruleset
        )
        {
            _ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));
            _whiteController =
                whiteController ?? throw new ArgumentNullException(nameof(whiteController));
            _blackController =
                blackController ?? throw new ArgumentNullException(nameof(blackController));

            // Seed with a synthetic TurnAdvancedEvent describing the starting player/turn.
            var seed = new TurnAdvancedEvent(initialState.CurrentPlayer, initialState.TurnNumber);
            _history.Add((seed, initialState));
            _currentIndex = 0;
        }

        public GameState CurrentState => _history[_currentIndex].State;
        public int CurrentIndex => _currentIndex;
        public int HistoryCount => _history.Count;
        public IReadOnlyList<(GameEvent Event, GameState State)> History => _history.AsReadOnly();

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

        /// <summary>
        /// Undo back to the TurnAdvanced event preceding the last player action.
        /// </summary>
        public void UndoLastMove()
        {
            if (_history.Count == 0)
                return;

            int seen = 0;
            int rewindTo = -1;

            // walk backwards from the current index
            for (int i = _currentIndex; i >= 0; i--)
            {
                if (_history[i].Event is TurnAdvancedEvent)
                {
                    seen++;
                    if (seen == 3)
                    {
                        rewindTo = i;
                        break;
                    }
                }
            }

            if (rewindTo >= 0)
                _currentIndex = rewindTo;
            else
                _currentIndex = -1; // nothing to undo
        }

        public void RedoLastMove()
        {
            if (_history.Count == 0)
                return;

            int seen = 0;
            int redoTo = -1;

            // walk forwards from the current index
            for (int i = _currentIndex + 1; i < _history.Count; i++)
            {
                if (_history[i].Event is TurnAdvancedEvent)
                {
                    seen++;
                    if (seen == 3)
                    {
                        redoTo = i;
                        break;
                    }
                }
            }

            if (redoTo >= 0)
                _currentIndex = redoTo;
        }

        /// <summary>
        /// Applies a single canonical GameEvent to state and pushes to history.
        /// Used by the pipeline after an event survives interception.
        /// </summary>
        private void ApplyCanonical(GameEvent ev, bool simulation)
        {
            var newState = ApplyEventToState(ev, CurrentState);

            // Trim redo branch if needed
            if (_currentIndex < _history.Count - 1)
                _history.RemoveRange(_currentIndex + 1, _history.Count - (_currentIndex + 1));

            _history.Add((ev, newState));
            _currentIndex++;

            if (!simulation)
                OnEventPublished?.Invoke(ev);
        }

        /// <summary>
        /// Translate a canonical GameEvent into a new GameState.
        /// </summary>
        private GameState ApplyEventToState(GameEvent ev, GameState current)
        {
            var board = current.Board.Clone();
            var nextPlayer = current.CurrentPlayer;
            var turn = current.TurnNumber;
            Console.WriteLine(
                $"[Engine] ApplyEventToState: {ev.GetType().Name} ({ev.Description})"
            );

            switch (ev)
            {
                case MoveEvent m:
                {
                    // Re-resolve from the cloned board, do not trust payload object identity
                    var piece = board.GetPieceAt(m.From);
                    if (piece != null)
                    {
                        board.MovePiece(m.From, m.To);
                    }
                    break;
                }
                case CaptureEvent c:
                {
                    var pos = c.Target.Position;
                    if (board.GetPieceAt(pos) != null)
                        board.RemovePiece(pos);
                    break;
                }
                case DestroyEvent d:
                {
                    var pos = d.Target.Position;
                    if (board.GetPieceAt(pos) != null)
                        board.RemovePiece(pos);
                    break;
                }
                case StatusAppliedEvent s:
                {
                    var pos = s.Target.Position;
                    var onBoard = board.GetPieceAt(pos);
                    if (onBoard != null)
                    {
                        if (onBoard is StatusEffectDecorator deco)
                        {
                            deco.AddStatus(s.Effect);
                        }
                        else
                        {
                            var wrapped = new StatusEffectDecorator(onBoard);
                            wrapped.AddStatus(s.Effect);
                            board.RemovePiece(pos);
                            board.PlacePiece(wrapped, pos);
                        }
                    }
                    break;
                }
                case StatusRemovedEvent sr:
                {
                    var pos = sr.Target.Position;
                    var onBoard = board.GetPieceAt(pos);
                    if (onBoard is StatusEffectDecorator deco)
                    {
                        deco.RemoveStatus(sr.Effect);
                        // Optionally unwrap if empty; interceptor can also handle this policy.
                        if (!deco.HasAnyStatus)
                        {
                            board.RemovePiece(pos);
                            board.PlacePiece(deco.Inner, pos);
                        }
                    }
                    break;
                }
                case TurnAdvancedEvent t:
                {
                    nextPlayer = t.NextPlayer;
                    turn = t.TurnNumber;
                    break;
                }
                case TileChangedEvent tce:
                {
                    board.SetTile(tce.Position, tce.NewTile.Clone());
                    break;
                }

                case PieceChangedEvent pce:
                {
                    board.RemovePiece(pce.OldPiece.Position);
                    board.PlacePiece(pce.NewPiece, pce.Position);
                    break;
                }

                case TurnStartEvent _:
                    // no board change
                    break;

                case TurnEndEvent _:
                    // no board change
                    break;
            }

            return new GameState(board, nextPlayer, turn);
        }
    }
}
