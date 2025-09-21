using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.Tiles;

namespace RogueChess.Engine.Board
{
    public class Board : IBoard
    {
        private readonly IPiece?[,] pieces;
        private readonly ITile[,] tiles;

        public int Width { get; }
        public int Height { get; }

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            pieces = new IPiece?[width, height];
            tiles = new ITile[width, height];

            // Initialize all tiles with StandardTile by default
            // Use SetTile() to customize specific tiles
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    SetTile(new Vector2Int(x, y), new StandardTile());
                }
            }
        }

        public IPiece? GetPieceAt(Vector2Int pos)
        {
            return IsInBounds(pos) ? pieces[pos.X, pos.Y] : null;
        }

        public void PlacePiece(IPiece piece, Vector2Int pos)
        {
            if (!IsInBounds(pos))
                return;
            pieces[pos.X, pos.Y] = piece;
            piece.Position = pos;
        }

        public void RemovePiece(Vector2Int pos)
        {
            if (!IsInBounds(pos))
                return;
            pieces[pos.X, pos.Y] = null;
        }

        public void MovePiece(Vector2Int from, Vector2Int to)
        {
            if (!IsInBounds(from) || !IsInBounds(to))
                return;
            var piece = pieces[from.X, from.Y];
            if (piece == null)
                return;

            pieces[from.X, from.Y] = null;
            pieces[to.X, to.Y] = piece;
            piece.Position = to;
        }

        public ITile GetTile(Vector2Int pos)
        {
            return IsInBounds(pos) ? tiles[pos.X, pos.Y] : new StandardTile();
        }

        public void SetTile(Vector2Int pos, ITile tile)
        {
            if (!IsInBounds(pos))
                return;

            tile.Position = pos;

            tiles[pos.X, pos.Y] = tile;
        }

        /// <summary>
        /// Set a tile at the specified position with bounds checking.
        /// </summary>
        public void SetTile(int x, int y, ITile tile)
        {
            SetTile(new Vector2Int(x, y), tile);
        }

        /// <summary>
        /// Set multiple tiles in a rectangular area.
        /// </summary>
        public void SetTiles(Vector2Int topLeft, Vector2Int bottomRight, ITile tile)
        {
            for (int x = topLeft.X; x <= bottomRight.X; x++)
            {
                for (int y = topLeft.Y; y <= bottomRight.Y; y++)
                {
                    SetTile(x, y, tile);
                }
            }
        }

        /// <summary>
        /// Set tiles at specific positions.
        /// </summary>
        public void SetTiles(IEnumerable<(Vector2Int pos, ITile tile)> tilePositions)
        {
            foreach (var (pos, tile) in tilePositions)
            {
                SetTile(pos, tile);
            }
        }

        public IEnumerable<IPiece> GetAllPieces(PlayerColor? owner = null)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var piece = pieces[x, y];
                    if (piece != null && (owner == null || piece.Owner == owner))
                    {
                        yield return piece;
                    }
                }
            }
        }

        public bool IsInBounds(Vector2Int pos)
        {
            return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
        }

        public IBoard Clone()
        {
            var clone = new Board(Width, Height);

            // Clone pieces
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var piece = pieces[x, y];
                    if (piece != null)
                    {
                        clone.pieces[x, y] = piece.Clone();
                    }
                }
            }

            // Clone tiles
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    clone.tiles[x, y] = tiles[x, y].Clone();
                }
            }

            return clone;
        }

        public IEnumerable<ITile> GetAllTiles()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var pos = new Vector2Int(x, y);
                    var tile = tiles[x, y];
                    if (tile != null)
                    {
                        yield return tile;
                    }
                }
            }
        }
    }
}
