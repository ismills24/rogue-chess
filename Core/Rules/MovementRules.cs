using System.Collections.Generic;
using UnityEngine;

namespace ChessRogue.Core.Rules
{
    public static class MovementRules
    {
        /// <summary>
        /// Sliding movement in straight lines until blocked (Rook, Bishop, Queen).
        /// </summary>
        public static IEnumerable<Move> SlidingMoves(
            GameState state,
            IPiece piece,
            IEnumerable<Vector2Int> directions)
        {
            foreach (var dir in directions)
            {
                var pos = piece.Position;
                while (true)
                {
                    pos += dir;
                    if (!state.Board.IsInside(pos))
                        break;

                    var targetPiece = state.Board.GetPieceAt(pos);
                    if (targetPiece == null)
                    {
                        yield return new Move(piece.Position, pos, piece, false);
                    }
                    else
                    {
                        if (targetPiece.Owner != piece.Owner)
                            yield return new Move(piece.Position, pos, piece, true);
                        break; // stop sliding in this direction
                    }
                }
            }
        }

        /// <summary>
        /// Fixed offset “leap” moves that ignore intervening pieces (Knight, custom leapers).
        /// </summary>
        public static IEnumerable<Move> JumpMoves(
            GameState state,
            IPiece piece,
            IEnumerable<Vector2Int> offsets)
        {
            foreach (var offset in offsets)
            {
                var target = piece.Position + offset;
                if (!state.Board.IsInside(target))
                    continue;

                var targetPiece = state.Board.GetPieceAt(target);
                if (targetPiece == null || targetPiece.Owner != piece.Owner)
                    yield return new Move(piece.Position, target, piece, targetPiece != null);
            }
        }

        /// <summary>
        /// Moves to the 8 surrounding squares (or fewer if filtered).
        /// </summary>
        public static IEnumerable<Move> AdjacentMoves(
            GameState state,
            IPiece piece,
            bool includeDiagonals = true)
        {
            var directions = new List<Vector2Int>
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            if (includeDiagonals)
            {
                directions.Add(new Vector2Int(1, 1));
                directions.Add(new Vector2Int(-1, 1));
                directions.Add(new Vector2Int(1, -1));
                directions.Add(new Vector2Int(-1, -1));
            }

            foreach (var dir in directions)
            {
                var target = piece.Position + dir;
                if (!state.Board.IsInside(target))
                    continue;

                var targetPiece = state.Board.GetPieceAt(target);
                if (targetPiece == null || targetPiece.Owner != piece.Owner)
                    yield return new Move(piece.Position, target, piece, targetPiece != null);
            }
        }

        /// <summary>
        /// Forward-only pawn-style movement.
        /// </summary>
        public static IEnumerable<Move> ForwardMoves(
            GameState state,
            IPiece piece,
            int maxSteps,
            int direction) // +1 for White (up), -1 for Black (down)
        {
            var pos = piece.Position;

            for (int step = 1; step <= maxSteps; step++)
            {
                var forward = pos + new Vector2Int(0, step * direction);
                if (!state.Board.IsInside(forward))
                    break;

                if (state.Board.GetPieceAt(forward) == null)
                {
                    yield return new Move(piece.Position, forward, piece, false);
                }
                else
                {
                    break; // blocked
                }
            }
        }

        /// <summary>
        /// Pawn-style diagonal captures (does not include en passant).
        /// </summary>
        public static IEnumerable<Move> DiagonalCaptures(
            GameState state,
            IPiece piece,
            int direction) // +1 for White, -1 for Black
        {
            var captures = new[]
            {
                piece.Position + new Vector2Int(1, direction),
                piece.Position + new Vector2Int(-1, direction)
            };

            foreach (var target in captures)
            {
                if (!state.Board.IsInside(target))
                    continue;

                var targetPiece = state.Board.GetPieceAt(target);
                if (targetPiece != null && targetPiece.Owner != piece.Owner)
                    yield return new Move(piece.Position, target, piece, true);
            }
        }
        
        /// <summary>
        /// En Passant captures (if last move was a double pawn step).
        /// </summary>
        public static IEnumerable<Move> EnPassantCaptures(GameState state, IPiece pawn, int direction)
        {
            var lastMove = state.MoveHistory.LastOrDefault();
            if (lastMove.Piece is Pawn && 
                Mathf.Abs(lastMove.From.y - lastMove.To.y) == 2)
            {
                // The pawn that moved last turn
                var enemyPawn = lastMove.Piece;
                var enemyPos = lastMove.To;

                // Check if current pawn is adjacent
                if (Mathf.Abs(enemyPos.x - pawn.Position.x) == 1 && 
                    enemyPos.y == pawn.Position.y)
                {
                    var capturePos = enemyPos + new Vector2Int(0, direction);
                    if (state.Board.IsInside(capturePos) && state.Board.GetPieceAt(capturePos) == null)
                    {
                        yield return new Move(pawn.Position, capturePos, pawn, isCapture: true);
                    }
                }
            }
        }
    }
}
