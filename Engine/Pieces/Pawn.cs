using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Standard chess pawn piece.
    /// </summary>
    public class Pawn : PieceBase
    {
        public Pawn(PlayerColor owner, Vector2Int position) 
            : base("Pawn", owner, position)
        {
        }

        public override IEnumerable<Move> GetPseudoLegalMoves(GameState state)
        {
            var moves = new List<Move>();
            var direction = Owner == PlayerColor.White ? 1 : -1;
            var startRank = Owner == PlayerColor.White ? 1 : 6;

            // Forward move (one square)
            var forwardOne = Position + new Vector2Int(0, direction);
            if (state.Board.IsInBounds(forwardOne) && state.Board.GetPieceAt(forwardOne) == null)
            {
                moves.Add(new Move(Position, forwardOne, this));
            }

            // Forward move (two squares from start position)
            if (Position.Y == startRank)
            {
                var forwardTwo = Position + new Vector2Int(0, direction * 2);
                if (state.Board.IsInBounds(forwardTwo) && 
                    state.Board.GetPieceAt(forwardTwo) == null &&
                    state.Board.GetPieceAt(forwardOne) == null)
                {
                    moves.Add(new Move(Position, forwardTwo, this));
                }
            }

            // Diagonal captures
            var leftCapture = Position + new Vector2Int(-1, direction);
            var rightCapture = Position + new Vector2Int(1, direction);

            foreach (var capturePos in new[] { leftCapture, rightCapture })
            {
                if (state.Board.IsInBounds(capturePos))
                {
                    var targetPiece = state.Board.GetPieceAt(capturePos);
                    if (targetPiece != null && targetPiece.Owner != Owner)
                    {
                        moves.Add(new Move(Position, capturePos, this, IsCapture: true));
                    }
                }
            }

            return moves;
        }

        public override int GetValue()
        {
            return 1; // Pawn value
        }

        protected override IPiece CreateClone()
        {
            return new Pawn(Owner, Position);
        }
    }
}
