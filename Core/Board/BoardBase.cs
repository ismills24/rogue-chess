namespace ChessRogue.Core.Board
{
    public abstract class BoardBase : IBoard
    {
        protected IPiece[,] pieces;
        protected ITile[,] tiles;

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        protected BoardBase(int width, int height)
        {
            Width = width;
            Height = height;
            pieces = new IPiece[width, height];
            tiles = new ITile[width, height];
        }

        public virtual IPiece GetPieceAt(Vector2Int pos) =>
            IsInBounds(pos) ? pieces[pos.x, pos.y] : null;

        public virtual void PlacePiece(IPiece piece, Vector2Int pos)
        {
            if (!IsInBounds(pos))
                return;

            pieces[pos.x, pos.y] = piece;
            piece.Position = pos;
        }

        public virtual void RemovePiece(Vector2Int pos)
        {
            if (!IsInBounds(pos))
                return;
            pieces[pos.x, pos.y] = null;
        }

        public virtual void MovePiece(Vector2Int from, Vector2Int to)
        {
            if (!IsInBounds(from) || !IsInBounds(to))
                return;
            var piece = pieces[from.x, from.y];
            if (piece == null)
                return;

            pieces[from.x, from.y] = null;
            pieces[to.x, to.y] = piece;
            piece.Position = to;
        }

        public virtual IEnumerable<IPiece> GetAllPieces(PlayerColor color)
        {
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                var piece = pieces[x, y];
                if (piece != null && piece.Owner == color)
                    yield return piece;
            }
        }

        public virtual bool IsInBounds(Vector2Int pos) =>
            pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y < Height;

        public virtual ITile GetTile(Vector2Int pos) =>
            IsInBounds(pos) ? tiles[pos.x, pos.y] : null;

        public virtual void SetTile(Vector2Int pos, ITile tile)
        {
            if (IsInBounds(pos))
                tiles[pos.x, pos.y] = tile;
        }

        protected abstract BoardBase CreateEmpty(int width, int height);

        public virtual IBoard Clone()
        {
            var clone = CreateEmpty(Width, Height);
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                // copy tile (reuse instance if stateless)
                if (tiles[x, y] != null)
                    clone.tiles[x, y] = tiles[x, y];

                var p = pieces[x, y];
                if (p != null)
                {
                    var pc = p.Clone();
                    clone.pieces[x, y] = pc;
                    pc.Position = new Vector2Int(x, y);
                }
            }
            return clone;
        }
    }
}
