using System;
using System.Collections.Generic;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Tiles
{
    /// <summary>
    /// Forces a piece to slide one extra step in its movement direction on entering.
    /// </summary>
    public class SlipperyTile : BaseTile
    {
        public SlipperyTile()
            : base() { }

        public SlipperyTile(Vector2Int position)
            : base(position) { }

        public override IEnumerable<CandidateEvent> OnEnter(
            IPiece piece,
            Vector2Int pos,
            GameState state
        )
        {
            // The move hasn't been applied yet.
            // Derive direction from where the piece currently is to the intended pos.
            var dir = new Vector2Int(pos.X - piece.Position.X, pos.Y - piece.Position.Y);
            if (dir.X == 0 && dir.Y == 0)
                yield break;

            // Normalize to a single step
            var step = new Vector2Int(
                dir.X == 0 ? 0 : Math.Sign(dir.X),
                dir.Y == 0 ? 0 : Math.Sign(dir.Y)
            );

            var next = pos + step;

            // Only slide if the next tile is empty and in bounds
            if (state.Board.IsInBounds(next) && state.Board.GetPieceAt(next) == null)
            {
                yield return new CandidateEvent(
                    GameEventType.TileEffectTriggered,
                    false,
                    new ForcedSlidePayload(piece, pos, next)
                );
            }
        }

        public override IEnumerable<CandidateEvent> OnTurnStart(
            IPiece piece,
            Vector2Int pos,
            GameState state
        )
        {
            yield break;
        }

        public override ITile Clone() => new SlipperyTile(Position);
    }
}
