using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Bishop piece implementation.
    /// Moves diagonally any number of squares.
    /// </summary>
    public class Bishop : PieceBase
    {
        public Bishop(PlayerColor owner, Vector2Int position) : base("Bishop", owner, position)
        {
        }

        public override int GetValue()
        {
            return 3; // Bishop value
        }

        public override IEnumerable<Move> GetPseudoLegalMoves(GameState state)
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
            return new Bishop(Owner, Position);
        }
    }
}
