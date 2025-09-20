using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Events
{
    /// <summary>Payload for when a piece is destroyed by an effect (explosion, burning, etc.).</summary>
    public record PieceDestroyedPayload(IPiece Piece, string? Reason = null);

    /// <summary>Payload for status effect ticking (duration updates).</summary>
    public record StatusTickPayload(IPiece Piece, string StatusName, int RemainingDuration);

    /// <summary>Payload for when a move is cancelled.</summary>
    public record MoveCancelledPayload(IPiece Attacker, Move OriginalMove);
}
