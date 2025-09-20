using RogueChess.Engine.Interfaces;

namespace RogueChess.Engine.Primitives
{
    /// <summary>
    /// Represents a single attempted move of a piece.
    /// Note: this is a proposition until confirmed by the Runner.
    /// </summary>
    public record Move(
        Vector2Int From,
        Vector2Int To,
        IPiece Piece,
        bool IsCapture = false,
        bool IsPromotion = false
    );
}
