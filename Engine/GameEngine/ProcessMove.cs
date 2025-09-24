// Engine/GameEngine/ProcessMove.cs
using System.Collections.Generic;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine
{
    public partial class GameEngine
    {
        /// <summary>
        /// Build an action package (capture -> move) for the given move.
        /// No turn-advance here, and no direct state mutation — just the package.
        /// Tile effects, slides, Martyr/Guardian/Marksman, promotions, etc.
        /// should be handled by interceptors on MoveEvent/CaptureEvent.
        /// </summary>
        public IEventSequence BuildMoveSequence(Move move, GameState state)
        {
            var events = new List<GameEvent>();

            var mover = state.Board.GetPieceAt(move.From);
            if (mover == null)
                return ActionPackages.EmptyAbort; // invalid move, abort chain

            var target = state.Board.GetPieceAt(move.To);
            if (target != null)
            {
                events.Add(
                    new CaptureEvent(mover, target, state.CurrentPlayer, isPlayerAction: true)
                );
            }

            events.Add(
                new MoveEvent(move.From, move.To, mover, state.CurrentPlayer, isPlayerAction: true)
            );

            return ActionPackages.Pack(events);
        }
    }
}
