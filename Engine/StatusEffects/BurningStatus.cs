using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.StatusEffects
{
    /// <summary>
    /// Status effect that burns a piece over time.
    /// This is a placeholder implementation for Step 4.
    /// </summary>
    public class BurningStatus
    {
        public string Name => "Burning";
        public int Duration { get; private set; } = 2;

        public IEnumerable<CandidateEvent> OnTurnStart(IPiece piece, GameState state)
        {
            Duration--;
            
            // Always emit a status effect event to show burning is active
            yield return new CandidateEvent(
                GameEventType.StatusEffectTriggered,
                false, // Not a player action
                new StatusTickPayload(piece, Name, Duration)
            );
            
            if (Duration <= 0)
            {
                // Emit candidate event to destroy the piece
                yield return new CandidateEvent(
                    GameEventType.StatusEffectTriggered,
                    false, // Not a player action
                    new PieceDestroyedPayload(piece, "Burned to ashes!")
                );
            }
        }

        public BurningStatus Clone()
        {
            return new BurningStatus { Duration = this.Duration };
        }
    }

    /// <summary>
    /// Payload for when a piece is destroyed by a status effect.
    /// </summary>
    public record PieceDestroyedPayload(IPiece Piece, string Reason);

    /// <summary>
    /// Payload for status effect ticking (duration updates).
    /// </summary>
    public record StatusTickPayload(IPiece Piece, string StatusName, int RemainingDuration);
}
