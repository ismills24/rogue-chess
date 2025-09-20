using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Hooks
{
    /// <summary>
    /// Utility class for collecting all active hooks from the current game state.
    /// Provides deterministic ordering of hooks for consistent behavior.
    /// </summary>
    public static class HookCollector
    {
        /// <summary>
        /// Collect all active hooks from the current game state.
        /// Order: Ruleset → Tiles → Pieces → Status Effects
        /// </summary>
        public static IEnumerable<IBeforeEventHook> CollectHooks(GameState state)
        {
            var hooks = new List<IBeforeEventHook>();

            // 1. Ruleset hooks (if any)
            // Note: Rulesets don't typically provide hooks, but this is the first priority

            // 2. Tile hooks
            foreach (var tile in GetAllTiles(state.Board))
            {
                if (tile is IBeforeEventHook tileHook)
                {
                    hooks.Add(tileHook);
                }
            }

            // 3. Piece hooks (including decorators)
            foreach (var piece in state.Board.GetAllPieces())
            {
                CollectPieceHooks(piece, hooks);
            }

            // 4. Status effect hooks (if any)
            // Note: Status effects are typically handled through piece decorators

            return hooks;
        }

        /// <summary>
        /// Recursively collect hooks from a piece and all its decorators.
        /// </summary>
        private static void CollectPieceHooks(IPiece piece, List<IBeforeEventHook> hooks)
        {
            // Add the piece itself if it implements IBeforeEventHook
            if (piece is IBeforeEventHook pieceHook)
            {
                hooks.Add(pieceHook);
            }

            // If it's a decorator, also check the inner piece
            if (piece is PieceDecoratorBase decorator)
            {
                CollectPieceHooks(decorator.Inner, hooks);
            }
        }

        /// <summary>
        /// Get all tiles from the board.
        /// </summary>
        private static IEnumerable<ITile> GetAllTiles(IBoard board)
        {
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    var pos = new Vector2Int(x, y);
                    var tile = board.GetTile(pos);
                    if (tile != null)
                    {
                        yield return tile;
                    }
                }
            }
        }
    }
}
