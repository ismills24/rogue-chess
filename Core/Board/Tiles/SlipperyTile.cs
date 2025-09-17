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
            // Slide one more step in the same direction if possible
            var lastMove = state.MoveHistory.LastOrDefault();
            if (lastMove != null && lastMove.To == pos)
            {
                var dir = pos - lastMove.From;
                var slipTarget = pos + dir;

                if (
                    state.Board.IsInBounds(slipTarget)
                    && state.Board.GetPieceAt(slipTarget) == null
                )
                {
                    state.Board.MovePiece(pos, slipTarget);

                    yield return new GameEvent(
                        GameEventType.TileEffectTriggered,
                        piece,
                        pos,
                        slipTarget,
                        "Slipped!"
                    );
                }
            }

            // Call base to emit "Moved"
            foreach (var ev in base.OnEnter(piece, pos, state))
                yield return ev;
        }
    }
}
