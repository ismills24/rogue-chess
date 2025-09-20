using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Board
{
    /// <summary>
    /// Basic board implementation for Step 3.
    /// This is a placeholder that will be expanded in Step 4.
    /// </summary>
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
            
            // Initialize all tiles with StandardTile
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tiles[x, y] = new StandardTile();
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
            tiles[pos.X, pos.Y] = tile;
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
    }

    /// <summary>
    /// Basic tile implementation for Step 3.
    /// This is a placeholder that will be expanded in Step 4.
    /// </summary>
    public class StandardTile : ITile
    {
        public Vector2Int Position { get; set; }

        public IEnumerable<CandidateEvent> OnEnter(IPiece piece, Vector2Int pos, GameState state)
        {
            yield break; // No effects
        }

        public IEnumerable<CandidateEvent> OnTurnStart(
            IPiece piece,
            Vector2Int pos,
            GameState state
        )
        {
            yield break; // No effects
        }

        public ITile Clone()
        {
            return new StandardTile { Position = Position };
        }
    }

    /// <summary>
    /// Scorched tile that applies burning status to pieces that enter it.
    /// </summary>
    public class ScorchedTile : ITile
    {
        public Vector2Int Position { get; set; }

        public IEnumerable<CandidateEvent> OnEnter(IPiece piece, Vector2Int pos, GameState state)
        {
            // Emit candidate event to apply burning status
            yield return new CandidateEvent(
                GameEventType.StatusEffectTriggered,
                false, // Not a player action
                new StatusApplyPayload(piece, new StatusEffects.BurningStatus())
            );
        }

        public IEnumerable<CandidateEvent> OnTurnStart(IPiece piece, Vector2Int pos, GameState state)
        {
            // Keep burning any piece that starts its turn here
            yield return new CandidateEvent(
                GameEventType.StatusEffectTriggered,
                false, // Not a player action
                new StatusApplyPayload(piece, new StatusEffects.BurningStatus())
            );
        }

        public ITile Clone()
        {
            return new ScorchedTile { Position = Position };
        }
    }

    /// <summary>
    /// Slippery tile that forces pieces to slide one extra step in their movement direction.
    /// </summary>
    public class SlipperyTile : ITile
    {
        public Vector2Int Position { get; set; }

        public IEnumerable<CandidateEvent> OnEnter(IPiece piece, Vector2Int pos, GameState state)
        {
            // Find direction of the last move that landed here
            var lastMove = state.MoveHistory.LastOrDefault();
            if (lastMove == null || lastMove.To != pos)
                yield break;

            var dir = pos - lastMove.From;
            if (dir.X == 0 && dir.Y == 0)
                yield break;

            // Normalize to one square step
            if (dir.X != 0)
                dir = new Vector2Int(Math.Sign(dir.X), dir.Y);
            if (dir.Y != 0)
                dir = new Vector2Int(dir.X, Math.Sign(dir.Y));

            var next = pos + dir;

            if (state.Board.IsInBounds(next) && state.Board.GetPieceAt(next) == null)
            {
                // Emit candidate event for forced slide
                yield return new CandidateEvent(
                    GameEventType.TileEffectTriggered,
                    false, // Not a player action
                    new ForcedSlidePayload(piece, pos, next)
                );
            }
        }

        public IEnumerable<CandidateEvent> OnTurnStart(IPiece piece, Vector2Int pos, GameState state)
        {
            yield break; // No turn start effects
        }

        public ITile Clone()
        {
            return new SlipperyTile { Position = Position };
        }
    }
}
