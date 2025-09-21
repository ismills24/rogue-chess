using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Controllers;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;

namespace RogueChess.Engine
{
    /// <summary>
    /// Central orchestrator for the game engine.
    /// Manages canonical pipeline and history.
    /// Split into partials for clarity.
    /// </summary>
    public partial class GameEngine
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

            // Seed initial state
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

        public void UndoLastMove()
        {
            // Find the last canonical event that was caused by a player action
            var lastPlayerIndex = -1;
            for (int i = _currentIndex; i >= 0; i--)
            {
                if (_history[i].Event.IsPlayerAction)
                {
                    lastPlayerIndex = i;
                    break;
                }
            }

            if (lastPlayerIndex <= 0)
                return; // no moves to undo

            // Now walk backwards until we reach the TurnAdvanced BEFORE this move
            int rewindTo = lastPlayerIndex - 1;
            for (int i = lastPlayerIndex - 1; i >= 0; i--)
            {
                if (_history[i].Event.Type == GameEventType.TurnAdvanced)
                {
                    rewindTo = i;
                    break;
                }
            }

            _currentIndex = rewindTo;
        }

        public void RedoLastMove()
        {
            // Find the next TurnAdvanced event after the current index
            int redoTo = -1;
            for (int i = _currentIndex + 1; i < _history.Count; i++)
            {
                if (_history[i].Event.Type == GameEventType.TurnAdvanced)
                {
                    redoTo = i;
                    break;
                }
            }

            if (redoTo == -1)
                return; // nothing to redo

            _currentIndex = redoTo;
        }
    }

    public class TurnAdvancedPayload
    {
        public PlayerColor NewPlayer { get; }
        public int TurnNumber { get; }

        public TurnAdvancedPayload(PlayerColor newPlayer, int turnNumber)
        {
            NewPlayer = newPlayer;
            TurnNumber = turnNumber;
        }
    }
}
