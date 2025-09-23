// Engine/GameEngine/Simulation.cs
using RogueChess.Engine.Controllers;
using RogueChess.Engine.Events;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;

namespace RogueChess.Engine
{
    public partial class GameEngine
    {
        /// <summary>
        /// Simulate a full turn (move package + turn advance) without mutating canonical history.
        /// Uses a temporary engine instance and Dispatch(simulation:true).
        /// </summary>
        public static GameState SimulateTurn(GameState startingState, Move move, IRuleSet ruleset)
        {
            var dummy = new NullController();
            var engine = new GameEngine(startingState, dummy, dummy, ruleset);

            var pkg = engine.BuildMoveSequence(move, engine.CurrentState);
            var completed = engine.Dispatch(pkg, simulation: true);
            if (!completed)
                return engine.CurrentState;

            var after = engine.CurrentState;
            var next =
                after.CurrentPlayer == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;
            var advance = ActionPackages.Single(new TurnAdvancedEvent(next, after.TurnNumber + 1));
            engine.Dispatch(advance, simulation: true);

            return engine.CurrentState;
        }

        private class NullController : IPlayerController
        {
            public Move? SelectMove(GameState state) => null;
        }
    }
}
