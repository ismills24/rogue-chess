using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Rook piece implementation.
    /// Moves in straight lines until blocked.
    /// </summary>
    public class Rook : PieceBase
    {
        public Rook(PlayerColor owner, Vector2Int position)
            : base("Rook", owner, position) { }

        public override int GetValue() => 5;

        public override IEnumerable<Move> GetPseudoLegalMoves(GameState state)
        {
            return MovementHelper.GetSlidingMoves(
                this,
                state,
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            );
        }

        protected override IPiece CreateClone() => new Rook(Owner, Position);
    }
}



