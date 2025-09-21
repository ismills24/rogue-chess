using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Bishop piece implementation.
    /// Moves diagonally until blocked.
    /// </summary>
    public class Bishop : PieceBase
    {
        public Bishop(PlayerColor owner, Vector2Int position)
            : base("Bishop", owner, position) { }

        public override int GetValue() => 3;

        public override IEnumerable<Move> GetPseudoLegalMoves(GameState state)
        {
            return MovementHelper.GetSlidingMoves(
                this,
                state,
                new Vector2Int(1, 1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, 1),
                new Vector2Int(-1, -1)
            );
        }

        protected override IPiece CreateClone() => new Bishop(Owner, Position);
    }
}
