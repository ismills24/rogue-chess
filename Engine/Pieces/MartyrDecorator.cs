using RogueChess.Engine.Events;
using RogueChess.Engine.Hooks;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Decorator that implements the Martyr ability.
    /// When a friendly piece adjacent to this piece is about to be captured,
    /// this piece sacrifices itself instead.
    /// </summary>
    public class MartyrDecorator : PieceDecoratorBase, IBeforeEventHook
    {
        public MartyrDecorator(IPiece inner) : base(inner)
        {
        }

        public CandidateEvent? BeforeEvent(CandidateEvent candidate, GameState state)
        {
            // Only intercept capture events
            if (candidate.Type != GameEventType.PieceCaptured)
                return candidate;

            // Check if the capture payload contains a friendly piece
            if (candidate.Payload is not CapturePayload capturePayload)
                return candidate;

            var targetPiece = capturePayload.Target;
            
            // Only protect friendly pieces
            if (targetPiece.Owner != Inner.Owner)
                return candidate;

            // Check if the target piece is adjacent to this piece
            if (!IsAdjacent(targetPiece.Position, Inner.Position))
                return candidate;

            // Rewrite the capture to target this piece instead
            var rewrittenPayload = new CapturePayload(Inner);
            return candidate with { Payload = rewrittenPayload };
        }

        /// <summary>
        /// Check if two positions are adjacent (including diagonally).
        /// </summary>
        private static bool IsAdjacent(Vector2Int pos1, Vector2Int pos2)
        {
            var dx = Math.Abs(pos1.X - pos2.X);
            var dy = Math.Abs(pos1.Y - pos2.Y);
            return dx <= 1 && dy <= 1 && (dx + dy) > 0; // Adjacent but not the same position
        }

        protected override IPiece CreateDecoratorClone(IPiece inner)
        {
            return new MartyrDecorator(inner);
        }
    }
}
