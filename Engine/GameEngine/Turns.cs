// Engine/GameEngine/Turns.cs
using RogueChess.Engine.Controllers;
using RogueChess.Engine.Events;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine
{
    public partial class GameEngine
    {
        /// <summary>
        /// One complete turn with the new pipeline:
        /// 1) ask controller for a move
        /// 2) dispatch the move package (interceptors may replace/expand)
        /// 3) append TurnAdvancedEvent
        ///
        /// Turn-start/end effects should be implemented as interceptors (e.g. on MoveEvent
        /// or on a custom TurnStart/TurnEnd event type you may add later).
        /// </summary>
        public void RunTurn()
        {
            if (IsGameOver())
                return;

            var controller =
                CurrentState.CurrentPlayer == PlayerColor.White
                    ? _whiteController
                    : _blackController;
            Dispatch(
                ActionPackages.Single(
                    new TurnStartEvent(CurrentState.CurrentPlayer, CurrentState.TurnNumber)
                ),
                simulation: false
            );

            var move = controller.SelectMove(CurrentState);
            if (move == null)
                return;

            // Build and dispatch the move package
            var pkg = BuildMoveSequence(move, CurrentState);
            var completed = Dispatch(pkg, simulation: false);
            // If an interceptor aborted the chain (e.g., illegal/cancelled), stop here.
            if (!completed)
                return;

            Dispatch(
                ActionPackages.Single(
                    new TurnEndEvent(CurrentState.CurrentPlayer, CurrentState.TurnNumber)
                ),
                simulation: false
            );

            // Advance the turn
            var after = CurrentState;
            var nextPlayer =
                after.CurrentPlayer == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;
            var advance = ActionPackages.Single(
                new TurnAdvancedEvent(nextPlayer, after.TurnNumber + 1)
            );
            Dispatch(advance, simulation: false);
        }
    }
}
