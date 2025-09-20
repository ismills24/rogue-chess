using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Standard chess king piece.
    /// Moves one square in any direction.
    /// </summary>
    public class King : PieceBase
    {
        public King(PlayerColor owner, Vector2Int position)
            : base("King", owner, position) { }

        public override int GetValue() => 100; // high value

        public override IEnumerable<Move> GetPseudoLegalMoves(GameState state)
        {
            return MovementHelper.GetJumpMoves(
                this,
                state,
                new Vector2Int(1, 1),
                new Vector2Int(1, 0),
                new Vector2Int(1, -1),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 1),
                new Vector2Int(-1, 0),
                new Vector2Int(-1, -1)
            );
        }

        protected override IPiece CreateClone() => new King(Owner, Position);
    }
}
