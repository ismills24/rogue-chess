using RogueChess.Engine.Events;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.StatusEffects;

namespace RogueChess.Engine.Tiles
{
    /// <summary>
    /// Applies Burning to pieces that enter or start on it.
    /// </summary>
    public class ScorchedTile : BaseTile, IInterceptor<MoveEvent>, IInterceptor<TurnStartEvent>
    {
        public ScorchedTile()
            : base() { }

        public ScorchedTile(Vector2Int pos)
            : base(pos) { }

        public int Priority => 0;

        // Apply burning when a piece moves onto this tile
        public IEventSequence Intercept(MoveEvent ev, GameState state)
        {
            if (ev.To != Position)
                return new EventSequence(
                    System.Array.Empty<GameEvent>(),
                    FallbackPolicy.ContinueChain
                );

            var burn = new StatusAppliedEvent(ev.Piece, new BurningStatus(), ev.Actor);

            // Only emit burn, let original move apply normally
            return new EventSequence(new[] { burn }, FallbackPolicy.ContinueChain);
        }

        // Apply burning to occupant at start of its turn
        // Apply burning to occupant at start of its turn
        public IEventSequence Intercept(TurnStartEvent ev, GameState state)
        {
            var occupant = state.Board.GetPieceAt(Position);
            if (occupant == null || occupant.Owner != ev.Player)
            {
                // Let the original event fall through
                return new EventSequence(
                    System.Array.Empty<GameEvent>(),
                    FallbackPolicy.ContinueChain
                );
            }

            var burn = new StatusAppliedEvent(occupant, new BurningStatus(), ev.Player);

            // Add burn but let the original TurnStartEvent fall through
            return new EventSequence(new[] { burn }, FallbackPolicy.ContinueChain);
        }

        public override ITile Clone() => new ScorchedTile(Position);
    }
}
