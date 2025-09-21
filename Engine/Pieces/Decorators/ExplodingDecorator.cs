using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces.Decorators
{
    /// <summary>
    /// When captured, destroys all adjacent pieces.
    /// </summary>
    public class ExplodingDecorator : PieceDecoratorBase
    {
        public ExplodingDecorator(IPiece inner)
            : base(inner) { }

        protected override IEnumerable<CandidateEvent> OnCaptureDecorator(GameState state)
        {
            var offsets = new[]
            {
                new Vector2Int(-1, -1),
                new Vector2Int(0, -1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, 0),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 1),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
            };

            foreach (var offset in offsets)
            {
                var pos = Inner.Position + offset;
                if (!state.Board.IsInBounds(pos))
                    continue;

                var occupant = state.Board.GetPieceAt(pos);
                if (occupant != null)
                {
                    yield return new CandidateEvent(
                        GameEventType.PieceDestroyed,
                        false,
                        new PieceDestroyedPayload(occupant)
                    );
                }
            }
        }

        protected override IPiece CreateDecoratorClone(IPiece inner) =>
            new ExplodingDecorator(inner);
    }
}



