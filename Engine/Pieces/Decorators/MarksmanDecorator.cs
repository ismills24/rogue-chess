using System;
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

        public MarksmanDecorator(MarksmanDecorator original, IPiece innerClone)
            : base(original, innerClone)
        {
            _rangedAttacksLeft = original._rangedAttacksLeft;
        }

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

        /// <summary>
        /// Intercepts the capture event and destroys the target if the marksman has charges left.
        /// This causes the initial EventSequence to be aborted and the movement sequence to be skipped.
        /// </summary>
        public IEventSequence Intercept(CaptureEvent ev, GameState state)
        {
            Console.WriteLine(
                $"[Marksman] Intercepting CaptureEvent: Attacker={ev.Attacker.Name}, Inner={Inner.Name}, This={this.GetType().Name}, ShotsLeft={_rangedAttacksLeft}"
            );

            if (_rangedAttacksLeft > 0 && this.IsAttacker(ev))
            {
                Console.WriteLine("[Marksman] Condition matched → firing ranged shot!");
                _rangedAttacksLeft--;

                var destroy = new DestroyEvent(ev.Target, "Marksman ranged attack", ev.Actor, ID);
                return new EventSequence(new[] { destroy }, FallbackPolicy.AbortChain);
            }
            else
            {
                Console.WriteLine(
                    "[Marksman] Condition not met → letting capture proceed normally"
                );
                return new EventSequence(Array.Empty<GameEvent>(), FallbackPolicy.ContinueChain);
            }
        }

        protected override IPiece CreateDecoratorClone(IPiece inner) =>
            new MarksmanDecorator(this, inner);
    }
}
