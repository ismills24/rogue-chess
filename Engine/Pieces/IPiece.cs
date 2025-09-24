using System.Collections.Generic;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Interfaces
{
    /// <summary>
    /// Base contract for all pieces (pawns, rooks, custom roguelike units, etc).
    /// Pieces can move, be cloned, and may expose abilities via decorators.
    /// </summary>
    public interface IPiece
    {
        string Name { get; }
        PlayerColor Owner { get; }
        Vector2Int Position { get; set; }
        int MovesMade { get; set; }
        int CapturesMade { get; set; }
        System.Guid ID { get; }

        IEnumerable<Move> GetPseudoLegalMoves(GameState state);

        int GetValue();
        IPiece Clone();
    }
}
