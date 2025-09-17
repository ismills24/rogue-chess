namespace ChessRogue.Core.Events
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

    public record GameEvent(
        GameEventType Type,
        IPiece? Piece = null,
        Vector2Int? From = null,
        Vector2Int? To = null,
        string? Message = null
    );
}
