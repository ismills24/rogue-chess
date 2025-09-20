using RogueChess.Engine.Events;
using RogueChess.Engine.Hooks;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces.Decorators
{
    /// <summary>
    /// Sacrifices itself if an adjacent friendly piece would be captured.
    /// The attacking move is canceled; the martyr dies, ally survives,
    /// and the attacker remains on its original square.
    /// </summary>
    public class MartyrDecorator : PieceDecoratorBase, IBeforeEventHook
    {
        public MartyrDecorator(IPiece inner)
            : base(inner) { }

        public IEnumerable<CandidateEvent>? BeforeEvent(CandidateEvent candidate, GameState state)
        {
            // --- intercept captures targeting adjacent allies ---
            if (
                candidate.Type == GameEventType.PieceCaptured
                && candidate.Payload is CapturePayload capturePayload
            )
            {
                var target = capturePayload.Target;
                if (target.Owner == Inner.Owner && IsAdjacent(target.Position, Inner.Position))
                {
                    // martyr dies instead of ally, and attacker’s move is cancelled
                    return new[]
                    {
                        // Martyr dies
                        new CandidateEvent(
                            GameEventType.PieceDestroyed,
                            false,
                            new PieceDestroyedPayload(Inner, "Died protecting an ally")
                        ),
                        // Ally is teleported to martyr’s old square
                        new CandidateEvent(
                            GameEventType.MoveApplied,
                            false,
                            new MovePayload(target, target.Position, Inner.Position)
                        ),
                    };
                }
            }

            // Otherwise leave unchanged
            return new[] { candidate };
        }

        private static bool IsAdjacent(Vector2Int a, Vector2Int b)
        {
            var dx = Math.Abs(a.X - b.X);
            var dy = Math.Abs(a.Y - b.Y);
            return dx <= 1 && dy <= 1 && (dx + dy) > 0;
        }

        protected override IPiece CreateDecoratorClone(IPiece inner) => new MartyrDecorator(inner);
    }

    /// <summary>
    /// Payload for cancelling a move (attacker stays in place).
    /// </summary>
    public record MoveCancelledPayload(IPiece Attacker, Vector2Int From, Vector2Int To);
}
