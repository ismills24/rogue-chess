using RogueChess.Engine.Primitives;
using RogueChess.Engine.Tiles;

namespace RogueChess.Engine.Interfaces
{
    /// <summary>
    /// A board is a 2D grid of tiles + pieces.
    /// Provides spatial queries and mutation methods.
    /// </summary>
    public interface IBoard
    {
        int Width { get; }
        int Height { get; }

        IPiece? GetPieceAt(Vector2Int pos);
        void PlacePiece(IPiece piece, Vector2Int pos);
        void RemovePiece(Vector2Int pos);
        void MovePiece(Vector2Int from, Vector2Int to);

        ITile GetTile(Vector2Int pos);
        void SetTile(Vector2Int pos, ITile tile);
        IEnumerable<ITile> GetAllTiles();

        IEnumerable<IPiece> GetAllPieces(PlayerColor? owner = null);

        /// <summary>
        /// Returns true if the coordinate is inside the board.
        /// </summary>
        bool IsInBounds(Vector2Int pos);

        /// <summary>
        /// Deep clone the board, including tiles and pieces.
        /// </summary>
        IBoard Clone();
    }
}
