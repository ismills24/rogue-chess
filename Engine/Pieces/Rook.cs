using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Standard chess rook piece.
    /// </summary>
    public class Rook : PieceBase
    {
        public Rook(PlayerColor owner, Vector2Int position) 
            : base("Rook", owner, position)
        {
        }

        public override IEnumerable<Move> GetPseudoLegalMoves(GameState state)
        {
            var moves = new List<Move>();
            
            // Rook moves in straight lines (horizontal and vertical)
            var directions = new[]
            {
                new Vector2Int(1, 0),   // Right
                new Vector2Int(-1, 0),  // Left
                new Vector2Int(0, 1),   // Up
                new Vector2Int(0, -1)   // Down
            };

            foreach (var direction in directions)
            {
                var currentPos = Position + direction;
                
                while (state.Board.IsInBounds(currentPos))
                {
                    var pieceAtPos = state.Board.GetPieceAt(currentPos);
                    
                    if (pieceAtPos == null)
                    {
                        // Empty square - can move here
                        moves.Add(new Move(Position, currentPos, this));
                    }
                    else if (pieceAtPos.Owner != Owner)
                    {
                        // Enemy piece - can capture
                        moves.Add(new Move(Position, currentPos, this, IsCapture: true));
                        break; // Can't move further after capture
                    }
                    else
                    {
                        // Friendly piece - can't move here or further
                        break;
                    }
                    
                    currentPos += direction;
                }
            }

            return moves;
        }

        public override int GetValue()
        {
            return 5; // Rook value
        }

        protected override IPiece CreateClone()
        {
            return new Rook(Owner, Position);
        }
    }
}
