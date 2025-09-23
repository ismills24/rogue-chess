using System.Collections.Generic;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces.Decorators
{
    /// <summary>
    /// Allows the piece to capture enemy pieces from a distance without moving.
    /// When capturing, the piece stays on its current square instead of moving to the target.
    /// </summary>
    public class MarksmanDecorator : PieceDecoratorBase, IInterceptor<CaptureEvent>
    {
        private int _rangedAttacksLeft = 1;

        public MarksmanDecorator(IPiece inner)
            : base(inner) { }

        private MarksmanDecorator(IPiece inner, int charges)
            : base(inner) => _rangedAttacksLeft = charges;

        public override IEnumerable<Move> GetPseudoLegalMoves(GameState state)
        {
            foreach (var move in Inner.GetPseudoLegalMoves(state))
            {
                yield return move;

                if (_rangedAttacksLeft > 0)
                {
                    var target = state.Board.GetPieceAt(move.To);
                    if (target != null && target.Owner != Inner.Owner)
                    {
                        // Add ranged capture option
                        yield return new Move(move.From, move.To, Inner, isCapture: true);
                    }
                }
            }
        }

        public int Priority => 0;

        public IEventSequence Intercept(CaptureEvent ev, GameState state)
        {
            if (_rangedAttacksLeft > 0 && ev.Attacker == Inner)
            {
                _rangedAttacksLeft--;
                var destroy = new DestroyEvent(ev.Target, "Marksman ranged attack", ev.Actor);
                return new EventSequence(new[] { destroy }, FallbackPolicy.AbortChain);
            }

            // Let the original capture proceed untouched
            return new EventSequence(System.Array.Empty<GameEvent>(), FallbackPolicy.ContinueChain);
        }

        protected override IPiece CreateDecoratorClone(IPiece inner) =>
            new MarksmanDecorator(inner, _rangedAttacksLeft);
    }
}
