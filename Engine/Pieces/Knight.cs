using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Knight piece implementation.
    /// Jumps in an L-shape.
    /// </summary>
    public class Knight : PieceBase
    {
        public Knight(PlayerColor owner, Vector2Int position)
            : base("Knight", owner, position) { }

        public override int GetValue() => 3;

        public override IEnumerable<Move> GetPseudoLegalMoves(GameState state)
        {
            return MovementHelper.GetJumpMoves(
                this,
                state,
                new Vector2Int(2, 1),
                new Vector2Int(2, -1),
                new Vector2Int(-2, 1),
                new Vector2Int(-2, -1),
                new Vector2Int(1, 2),
                new Vector2Int(1, -2),
                new Vector2Int(-1, 2),
                new Vector2Int(-1, -2)
            );
        }

        protected override IPiece CreateClone() => new Knight(Owner, Position);
    }
}
