using System.Collections.Generic;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces.Decorators
{
    /// <summary>
    /// When destroyed (either captured or otherwise), explodes and destroys all adjacent pieces.
    /// </summary>
    public class ExplodingDecorator
        : PieceDecoratorBase,
            IInterceptor<CaptureEvent>,
            IInterceptor<DestroyEvent>
    {
        public ExplodingDecorator(IPiece inner)
            : base(inner) { }

        public ExplodingDecorator(PieceDecoratorBase original, IPiece innerClone)
            : base(original, innerClone) { }

        public int Priority => 0;

        public IEventSequence Intercept(CaptureEvent ev, GameState state)
        {
            // Only trigger if this piece is the target
            if (!ReferenceEquals(ev.Target, this) || ev.SourceID == ID)
            {
                return EventSequences.Continue;
            }

            var events = BuildExplosionEvents(ev.Actor, state);
            return new EventSequence(events, FallbackPolicy.AbortChain);
        }

        public IEventSequence Intercept(DestroyEvent ev, GameState state)
        {
            if (!ReferenceEquals(ev.Target, this) || ev.SourceID == ID)
                return EventSequences.Continue;

            var events = BuildExplosionEvents(ev.Actor, state);
            return new EventSequence(events, FallbackPolicy.AbortChain);
        }

        private List<GameEvent> BuildExplosionEvents(PlayerColor actor, GameState state)
        {
            var events = new List<GameEvent> { new DestroyEvent(this, "Exploded", actor, ID) };

            // Adjacent offsets
            var offsets = new (int dx, int dy)[]
            {
                (-1, -1),
                (0, -1),
                (1, -1),
                (-1, 0),
                (1, 0),
                (-1, 1),
                (0, 1),
                (1, 1),
            };

            foreach (var (dx, dy) in offsets)
            {
                var pos = Position + new Vector2Int(dx, dy);
                if (!state.Board.IsInBounds(pos))
                    continue;

                var occupant = state.Board.GetPieceAt(pos);
                if (occupant != null)
                {
                    events.Add(new DestroyEvent(occupant, "Exploded by ", actor, ID));
                }
            }

            return events;
        }

        protected override IPiece CreateDecoratorClone(IPiece inner) =>
            new ExplodingDecorator(inner);
    }
}
