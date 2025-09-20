using RogueChess.Engine.Board;
using RogueChess.Engine.Events;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Pieces
{
    /// <summary>
    /// Decorator that makes a piece explode when captured, affecting surrounding tiles.
    /// </summary>
    public class ExplodingDecorator : PieceDecoratorBase
    {
        public ExplodingDecorator(IPiece inner) : base(inner)
        {
        }

        protected override IEnumerable<CandidateEvent> OnCaptureDecorator(GameState state)
        {
            // Explode into surrounding 8 tiles
            var adjacentOffsets = new[]
            {
                new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                new Vector2Int(-1, 0),                          new Vector2Int(1, 0),
                new Vector2Int(-1, 1),  new Vector2Int(0, 1),  new Vector2Int(1, 1)
            };

            foreach (var offset in adjacentOffsets)
            {
                var targetPos = Inner.Position + offset;
                
                if (state.Board.IsInBounds(targetPos))
                {
                    // Emit candidate event to scorch the tile
                    yield return new CandidateEvent(
                        GameEventType.TileEffectTriggered,
                        false, // Not a player action
                        new TileChangePayload(targetPos, new ScorchedTile { Position = targetPos })
                    );

                    // If a piece is standing there, apply burn immediately
                    var occupant = state.Board.GetPieceAt(targetPos);
                    if (occupant != null)
                    {
                        yield return new CandidateEvent(
                            GameEventType.StatusEffectTriggered,
                            false, // Not a player action
                            new StatusApplyPayload(occupant, new StatusEffects.BurningStatus())
                        );
                    }
                }
            }
        }

        protected override IPiece CreateDecoratorClone(IPiece inner)
        {
            return new ExplodingDecorator(inner);
        }
    }
}
