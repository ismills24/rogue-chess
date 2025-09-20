using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Queen piece implementation.
    /// Combines the movement patterns of Rook and Bishop.
    /// </summary>
    public class Queen : PieceBase
    {
        public Queen(PlayerColor owner, Vector2Int position) : base("Queen", owner, position)
        {
        }

        public override int GetValue()
        {
            return 9; // Most valuable piece
        }

        public override IEnumerable<Move> GetPseudoLegalMoves(GameState state)
        {
            var moves = new List<Move>();

            // Queen moves like both Rook and Bishop
            // Rook moves (horizontal and vertical)
            moves.AddRange(GetRookMoves(state));
            
            // Bishop moves (diagonal)
            moves.AddRange(GetBishopMoves(state));

            return moves;
        }

        private IEnumerable<Move> GetRookMoves(GameState state)
        {
            var directions = new[]
            {
                new Vector2Int(1, 0),   // Right
                new Vector2Int(-1, 0),  // Left
                new Vector2Int(0, 1),   // Up
                new Vector2Int(0, -1)   // Down
            };

            foreach (var direction in directions)
            {
                for (int distance = 1; distance < 8; distance++)
                {
                    var targetPos = new Vector2Int(
                        Position.X + direction.X * distance,
                        Position.Y + direction.Y * distance
                    );
                    
                    if (!state.Board.IsInBounds(targetPos))
                        break;

                    var pieceAtTarget = state.Board.GetPieceAt(targetPos);
                    if (pieceAtTarget != null)
                    {
                        if (pieceAtTarget.Owner != Owner)
                        {
                            yield return new Move(Position, targetPos, this, true);
                        }
                        break; // Can't move through pieces
                    }
                    
                    yield return new Move(Position, targetPos, this);
                }
            }
        }

        private IEnumerable<Move> GetBishopMoves(GameState state)
        {
            var directions = new[]
            {
                new Vector2Int(1, 1),   // Up-Right
                new Vector2Int(1, -1),  // Down-Right
                new Vector2Int(-1, 1),  // Up-Left
                new Vector2Int(-1, -1)  // Down-Left
            };

            foreach (var direction in directions)
            {
                for (int distance = 1; distance < 8; distance++)
                {
                    var targetPos = new Vector2Int(
                        Position.X + direction.X * distance,
                        Position.Y + direction.Y * distance
                    );
                    
                    if (!state.Board.IsInBounds(targetPos))
                        break;

                    var pieceAtTarget = state.Board.GetPieceAt(targetPos);
                    if (pieceAtTarget != null)
                    {
                        if (pieceAtTarget.Owner != Owner)
                        {
                            yield return new Move(Position, targetPos, this, true);
                        }
                        break; // Can't move through pieces
                    }
                    
                    yield return new Move(Position, targetPos, this);
                }
            }
        }

        protected override IPiece CreateClone()
        {
            return new Queen(Owner, Position);
        }
    }
}
