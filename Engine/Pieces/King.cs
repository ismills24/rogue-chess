using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Standard chess king piece.
    /// </summary>
    public class King : PieceBase
    {
        public King(PlayerColor owner, Vector2Int position) 
            : base("King", owner, position)
        {
        }

        public override IEnumerable<Move> GetPseudoLegalMoves(GameState state)
        {
            var moves = new List<Move>();
            
            // King moves one square in any direction
            var directions = new[]
            {
                new Vector2Int(1, 1),   // Up-right
                new Vector2Int(1, 0),   // Right
                new Vector2Int(1, -1),  // Down-right
                new Vector2Int(0, 1),   // Up
                new Vector2Int(0, -1),  // Down
                new Vector2Int(-1, 1),  // Up-left
                new Vector2Int(-1, 0),  // Left
                new Vector2Int(-1, -1)  // Down-left
            };

            foreach (var direction in directions)
            {
                var targetPos = Position + direction;
                
                if (state.Board.IsInBounds(targetPos))
                {
                    var pieceAtPos = state.Board.GetPieceAt(targetPos);
                    
                    if (pieceAtPos == null)
                    {
                        // Empty square - can move here
                        moves.Add(new Move(Position, targetPos, this));
                    }
                    else if (pieceAtPos.Owner != Owner)
                    {
                        // Enemy piece - can capture
                        moves.Add(new Move(Position, targetPos, this, IsCapture: true));
                    }
                }
            }

            return moves;
        }

        public override int GetValue()
        {
            return 100; // King value (very high)
        }

        protected override IPiece CreateClone()
        {
            return new King(Owner, Position);
        }
    }
}
