using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Provides helper methods for common chess movement patterns (sliding, jumping).
    /// </summary>
    public static class MovementHelper
    {
        /// <summary>
        /// Generate sliding moves in given directions until blocked.
        /// </summary>
        public static IEnumerable<Move> GetSlidingMoves(
            IPiece piece,
            GameState state,
            params Vector2Int[] directions
        )
        {
            foreach (var dir in directions)
            {
                var pos = piece.Position + dir;

                while (state.Board.IsInBounds(pos))
                {
                    var target = state.Board.GetPieceAt(pos);

                    if (target == null)
                    {
                        yield return new Move(piece.Position, pos, piece);
                    }
                    else
                    {
                        if (target.Owner != piece.Owner)
                            yield return new Move(piece.Position, pos, piece, true);
                        break; // stop at first piece
                    }

                    pos += dir;
                }
            }
        }

        /// <summary>
        /// Generate single-step jump moves in given offsets.
        /// </summary>
        public static IEnumerable<Move> GetJumpMoves(
            IPiece piece,
            GameState state,
            params Vector2Int[] offsets
        )
        {
            foreach (var offset in offsets)
            {
                var pos = piece.Position + offset;
                if (!state.Board.IsInBounds(pos))
                    continue;

                var target = state.Board.GetPieceAt(pos);
                if (target == null)
                {
                    yield return new Move(piece.Position, pos, piece);
                }
                else if (target.Owner != piece.Owner)
                {
                    yield return new Move(piece.Position, pos, piece, true);
                }
            }
        }
    }
}



