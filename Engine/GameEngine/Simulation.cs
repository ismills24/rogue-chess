using RogueChess.Engine.Controllers;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;

namespace RogueChess.Engine
{
    public partial class GameEngine
    {
        /// <summary>
        /// Simulate one complete turn (including hooks and turn advance) without
        /// mutating this engine's canonical history.
        /// </summary>
        public static GameState SimulateTurn(GameState startingState, Move move, IRuleSet ruleset)
        {
            // Dummy controllers, not used since we directly provide the move
            var dummy = new NullController();

            var engine = new GameEngine(startingState, dummy, dummy, ruleset);

            // Run turn start hooks
            foreach (var ev in engine.TickTurnStart(engine.CurrentState))
                engine.Commit(ev, simulation: true);

            // Apply the move
            engine.ProcessMove(move, engine.CurrentState, simulation: true);

            // Run end-of-turn hooks
            foreach (var ev in engine.TickTurnEnd(engine.CurrentState))
                engine.Commit(ev, simulation: true);

            // Advance the turn
            var afterEndState = engine.CurrentState;
            var nextPlayer =
                afterEndState.CurrentPlayer == PlayerColor.White
                    ? PlayerColor.Black
                    : PlayerColor.White;

            var advance = new CandidateEvent(
                GameEventType.TurnAdvanced,
                false,
                new TurnAdvancedPayload(nextPlayer, afterEndState.TurnNumber + 1)
            );
            engine.Commit(advance, simulation: true);

            return engine.CurrentState;
        }

        /// <summary>
        /// Null controller used only for simulation.
        /// </summary>
        private class NullController : IPlayerController
        {
            public Move? SelectMove(GameState state) => null;
        }
    }
}
