namespace ChessRogue.Core
{
    public class Board
    {
        private readonly int width;
        private readonly int height;
        private readonly Cell[,] cells;

        public int Width => width;
        public int Height => height;

        public Board(int width = 8, int height = 8)
        {
            this.width = width;
            this.height = height;
            cells = new Cell[width, height];

            // Initialize every cell as playable & empty
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                cells[x, y] = new Cell();
        }

        public bool IsInside(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
        }

        public bool IsBlocked(Vector2Int pos)
        {
            return !IsInside(pos) || cells[pos.x, pos.y].IsBlocked;
        }

        public IPiece GetPieceAt(Vector2Int pos)
        {
            if (!IsInside(pos) || IsBlocked(pos))
                return null;
            return cells[pos.x, pos.y].Piece;
        }

        public void PlacePiece(IPiece piece, Vector2Int pos)
        {
            if (!IsInside(pos) || IsBlocked(pos))
                return;
            cells[pos.x, pos.y].Piece = piece;
            piece.Position = pos;
        }

        public IPiece RemovePiece(Vector2Int pos)
        {
            if (!IsInside(pos) || IsBlocked(pos))
                return null;
            var piece = cells[pos.x, pos.y].Piece;
            cells[pos.x, pos.y].Piece = null;
            return piece;
        }

        public void MovePiece(Vector2Int from, Vector2Int to)
        {
            if (!IsInside(from) || !IsInside(to) || IsBlocked(to))
                return;

            var piece = cells[from.x, from.y].Piece;
            cells[from.x, from.y].Piece = null;
            cells[to.x, to.y].Piece = piece;

            if (piece != null)
                piece.Position = to;
        }

        public void SetBlocked(Vector2Int pos, bool blocked = true)
        {
            if (!IsInside(pos))
                return;
            cells[pos.x, pos.y].IsBlocked = blocked;
            if (blocked)
                cells[pos.x, pos.y].Piece = null; // no piece allowed
        }

        public IEnumerable<IPiece> GetAllPieces(PlayerColor owner)
        {
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (cells[x, y].IsBlocked)
                    continue;
                var piece = cells[x, y].Piece;
                if (piece != null && piece.Owner == owner)
                    yield return piece;
            }
        }

        public IEnumerable<IPiece> GetAllPieces()
        {
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (cells[x, y].IsBlocked)
                    continue;
                var piece = cells[x, y].Piece;
                if (piece != null)
                    yield return piece;
            }
        }

        public Board Clone()
        {
            var clone = new Board(width, height);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                clone.cells[x, y].IsBlocked = cells[x, y].IsBlocked;
                var piece = cells[x, y].Piece;
                if (piece != null)
                {
                    var copy = piece.Clone();
                    clone.cells[x, y].Piece = copy;
                    copy.Position = new Vector2Int(x, y);
                }
            }
            return clone;
        }

        private class Cell
        {
            public IPiece Piece { get; set; }
            public bool IsBlocked { get; set; }
        }
    }
}
