// SlipperyTile.cs
using ChessRogue.Core.Events;

namespace ChessRogue.Core.Board.Tiles
{
    public class SlipperyTile : StandardTile
    {
        public override IEnumerable<GameEvent> OnEnter(
            IPiece piece,
            Vector2Int pos,
            GameState state
        )
        {
            // Find direction of the last *real* move that landed here
            var lastMove = state.MoveHistory.LastOrDefault();
            if (lastMove == null || lastMove.To != pos)
                yield break;
            var dir = pos - lastMove.From;
            if (dir.x == 0 && dir.y == 0)
                yield break;

            // Normalize to one square step
            if (dir.x != 0)
                dir.x = Math.Sign(dir.x);
            if (dir.y != 0)
                dir.y = Math.Sign(dir.y);

            var next = pos + dir;

            if (state.Board.IsInBounds(next) && state.Board.GetPieceAt(next) == null)
            {
                // Only emit — do not mutate here
                yield return new GameEvent(
                    GameEventType.TileEffectTriggered,
                    piece,
                    pos,
                    next,
                    "Slipped!"
                );
            }
        }
    }
}
