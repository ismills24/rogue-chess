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
    }

    public record TurnAdvancedPayload(PlayerColor NewPlayer, int TurnNumber);
}
