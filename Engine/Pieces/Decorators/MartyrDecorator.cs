using System;
using System.Collections.Generic;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces.Decorators
{
    /// <summary>
    /// Sacrifices itself if an adjacent friendly piece would be captured.
    /// The attacking move is canceled; the martyr dies, ally survives,
    /// and the attacker remains on its original square.
    /// </summary>
    public class MartyrDecorator : PieceDecoratorBase, IInterceptor<CaptureEvent>
    {
        public MartyrDecorator(IPiece inner)
            : base(inner) { }

        public MartyrDecorator(PieceDecoratorBase original, IPiece innerClone)
            : base(original, innerClone) { }

        public int Priority => 0;

        public IEventSequence Intercept(CaptureEvent ev, GameState state)
        {
            var target = ev.Target;
            if (target.Owner == Inner.Owner && IsAdjacent(target.Position, Inner.Position))
            {
                var martyrDies = new DestroyEvent(Inner, "Died protecting ally", ev.Actor, ID);
                return new EventSequence(new[] { martyrDies }, FallbackPolicy.AbortChain);
            }

            // Let capture proceed unchanged
            return EventSequences.Continue;
        }

        private static bool IsAdjacent(Vector2Int a, Vector2Int b) =>
            Math.Abs(a.X - b.X) <= 1 && Math.Abs(a.Y - b.Y) <= 1 && !(a.X == b.X && a.Y == b.Y);

        protected override IPiece CreateDecoratorClone(IPiece inner) => new MartyrDecorator(inner);
    }
}
