using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Events
{
    public enum GameEventType
    {
        MoveApplied,
        PieceCaptured,
        PiecePromoted,
        TileEffectTriggered,
        StatusEffectTriggered,
        TurnAdvanced,
        GameOver,
    }

    /// <summary>
    /// Canonical event — published externally and stored in history.
    /// One canonical GameEvent <-> one GameState snapshot.
    /// </summary>
    public record GameEvent(
        Guid Id,
        GameEventType Type,
        bool IsPlayerAction,
        Guid? ParentEventId,
        object? Payload
    );

    /// <summary>
    /// Internal candidate event — proposed by pieces, tiles, statuses, etc.
    /// Will be validated/rewritten by hooks before becoming canonical.
    /// </summary>
    public record CandidateEvent(GameEventType Type, bool IsPlayerAction, object? Payload);

    // ----------------------------
    // Payload types
    // ----------------------------

    public record MovePayload(IPiece Piece, Vector2Int From, Vector2Int To);

    public record CapturePayload(IPiece Target);

    public record TileChangePayload(Vector2Int Position, ITile NewTile);

    public record StatusApplyPayload(IPiece Target, object Effect);

    public record ForcedSlidePayload(IPiece Piece, Vector2Int From, Vector2Int To);
}
