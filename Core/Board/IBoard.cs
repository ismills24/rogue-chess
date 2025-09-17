namespace ChessRogue.Core.Board
{
    public interface IBoard
    {
        int Width { get; }
        int Height { get; }

        IPiece GetPieceAt(Vector2Int pos);
        void PlacePiece(IPiece piece, Vector2Int pos);
        void RemovePiece(Vector2Int pos);
        void MovePiece(Vector2Int from, Vector2Int to);
        IEnumerable<IPiece> GetAllPieces(PlayerColor color);

        bool IsInBounds(Vector2Int pos);
        ITile GetTile(Vector2Int pos);
        void SetTile(Vector2Int pos, ITile tile);

        IBoard Clone();
    }
}
