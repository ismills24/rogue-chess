using ChessRogue.Core.Events;
using ChessRogue.Core.StatusEffects;

namespace ChessRogue.Core.Board.Tiles
{
    public class ScorchedTile : StandardTile
    {
        public override IEnumerable<GameEvent> OnEnter(
            IPiece piece,
            Vector2Int pos,
            GameState state
        )
        {
            if (piece is IStatusEffectCarrier carrier)
            {
                carrier.AddStatus(new BurningStatus(2));
                yield return new GameEvent(
                    GameEventType.TileEffectTriggered,
                    piece,
                    piece.Position,
                    pos,
                    "Piece caught fire!"
                );
            }

            foreach (var ev in base.OnEnter(piece, pos, state))
                yield return ev;
        }
    }
}
