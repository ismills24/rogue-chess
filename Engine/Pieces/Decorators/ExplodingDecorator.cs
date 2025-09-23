using System.Collections.Generic;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces.Decorators
{
    /// <summary>
    /// When captured, destroys all adjacent pieces.
    /// </summary>
    public class ExplodingDecorator : PieceDecoratorBase, IInterceptor<DestroyEvent>
    {
        public ExplodingDecorator(IPiece inner)
            : base(inner) { }

        public int Priority => 0;

        public IEventSequence Intercept(DestroyEvent ev, GameState state)
        {
            if (ev.Target != Inner)
                return new EventSequence(new[] { ev }, FallbackPolicy.ContinueChain);

            var events = new List<GameEvent> { ev, new DestroyEvent(Inner, "Exploded", ev.Actor) };
            var offsets = new[]
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
                var pos = Inner.Position + new Vector2Int(dx, dy);
                if (!state.Board.IsInBounds(pos))
                    continue;
                var occupant = state.Board.GetPieceAt(pos);
                if (occupant != null)
                    events.Add(new DestroyEvent(occupant, "Exploded by martyr", ev.Actor));
            }
            return new EventSequence(events, FallbackPolicy.ContinueChain);
        }

        protected override IPiece CreateDecoratorClone(IPiece inner) =>
            new ExplodingDecorator(inner);
    }
}
