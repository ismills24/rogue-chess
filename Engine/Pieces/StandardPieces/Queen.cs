using System;
using System.Collections.Generic;
using System.Linq;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Queen piece implementation.
    /// Moves like Rook + Bishop combined.
    /// </summary>
    public class Queen : PieceBase
    {
        public Queen(PlayerColor owner, Vector2Int position)
            : base("Queen", owner, position) { }

        public Queen(Queen original)
            : base(original) { }

        public override int GetValue() => 9;

        public override IEnumerable<Move> GetPseudoLegalMoves(GameState state)
        {
            return MovementHelper.GetSlidingMoves(
                this,
                state,
                // rook-like
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                // bishop-like
                new Vector2Int(1, 1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, 1),
                new Vector2Int(-1, -1)
            );
        }
    }
}
