using ChessRogue.Core.Board.Tiles;
using ChessRogue.Core.Events;
using ChessRogue.Core.StatusEffects;

namespace ChessRogue.Core.Pieces.Decorators
{
    public class ExplodingPieceDecorator : PieceDecoratorBase
    {
        public ExplodingPieceDecorator(IPiece inner)
            : base(inner) { }

        public override void OnCapture(GameState state)
        {
            base.OnCapture(state);

            // Explode into surrounding 8 tiles
            var adjacents = new[]
            {
                new Vector2Int(-1, -1),
                new Vector2Int(0, -1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, 0),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 1),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
            };

            foreach (var offset in adjacents)
            {
                var target = inner.Position + offset;
                if (!state.Board.IsInBounds(target))
                    continue;

                // Replace the tile with a ScorchedTile
                state.Board.SetTile(target, new ScorchedTile());

                state.EnqueueEvent(
                    new GameEvent(
                        GameEventType.TileEffectTriggered,
                        inner,
                        inner.Position,
                        target,
                        "Explosion scorched tile"
                    )
                );

                // If a piece is standing there, apply burn immediately
                var occupant = state.Board.GetPieceAt(target);
                if (occupant is IStatusEffectCarrier carrier)
                {
                    carrier.AddStatus(new BurningStatus(2));
                    state.EnqueueEvent(
                        new GameEvent(
                            GameEventType.StatusEffectTriggered,
                            occupant,
                            target,
                            target,
                            "Caught fire from explosion"
                        )
                    );
                }
            }
        }

        public override IPiece Clone()
        {
            return new ExplodingPieceDecorator(inner.Clone());
        }
    }
}
