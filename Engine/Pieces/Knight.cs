using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Knight piece implementation.
    /// Moves in an L-shape pattern.
    /// </summary>
    public class Knight : PieceBase
    {
        public Knight(PlayerColor owner, Vector2Int position) : base("Knight", owner, position)
        {
        }

        public override int GetValue()
        {
            return 3; // Knight value
        }

        public override IEnumerable<Move> GetPseudoLegalMoves(GameState state)
        {
            var moves = new[]
            {
                new Vector2Int(2, 1),   // Right-Up
                new Vector2Int(2, -1),  // Right-Down
                new Vector2Int(-2, 1),  // Left-Up
                new Vector2Int(-2, -1), // Left-Down
                new Vector2Int(1, 2),   // Up-Right
                new Vector2Int(1, -2),  // Down-Right
                new Vector2Int(-1, 2),  // Up-Left
                new Vector2Int(-1, -2)  // Down-Left
            };

            foreach (var move in moves)
            {
                var targetPos = Position + move;
                
                if (!state.Board.IsInBounds(targetPos))
                    continue;

                var pieceAtTarget = state.Board.GetPieceAt(targetPos);
                if (pieceAtTarget == null)
                {
                    yield return new Move(Position, targetPos, this);
                }
                else if (pieceAtTarget.Owner != Owner)
                {
                    yield return new Move(Position, targetPos, this, true);
                }
            }
        }

        protected override IPiece CreateClone()
        {
            return new Knight(Owner, Position);
        }
    }
}
