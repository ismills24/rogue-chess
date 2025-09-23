using RogueChess.Engine.Events;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Tiles
{
    /// <summary>
    /// A GuardianTile protects the piece standing on it once.
    /// - If a capture targets the occupant, the capture is cancelled,
    ///   the tile is consumed (replaced with Standard), and the turn ends.
    /// - If a move would land onto this tile while occupied, the move is cancelled,
    ///   the tile is consumed, and the turn ends.
    /// After triggering once, the tile becomes a StandardTile.
    /// </summary>
    public class GuardianTile : BaseTile, IInterceptor<CaptureEvent>, IInterceptor<MoveEvent>
    {
        public GuardianTile()
            : base() { }

        public GuardianTile(Vector2Int pos)
            : base(pos) { }

        public int Priority => 0;

        public IEventSequence Intercept(CaptureEvent ev, GameState state)
        {
            if (ev.Target.Position != Position)
                return new EventSequence(
                    System.Array.Empty<GameEvent>(),
                    FallbackPolicy.ContinueChain
                );

            var nextPlayer =
                state.CurrentPlayer == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;
            var nextTurn = state.TurnNumber + 1;

            var consumeTile = new TileChangedEvent(Position, new StandardTile(Position), ev.Actor);
            var endTurn = new TurnAdvancedEvent(nextPlayer, nextTurn);

            // Replace capture entirely: tile is consumed, turn ends
            return new EventSequence(
                new GameEvent[] { consumeTile, endTurn },
                FallbackPolicy.AbortChain
            );
        }

        public IEventSequence Intercept(MoveEvent ev, GameState state)
        {
            if (ev.To != Position)
                return new EventSequence(
                    System.Array.Empty<GameEvent>(),
                    FallbackPolicy.ContinueChain
                );

            var occupant = state.Board.GetPieceAt(Position);
            if (occupant == null)
                return new EventSequence(
                    System.Array.Empty<GameEvent>(),
                    FallbackPolicy.ContinueChain
                );

            var nextPlayer =
                state.CurrentPlayer == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;
            var nextTurn = state.TurnNumber + 1;

            var consumeTile = new TileChangedEvent(Position, new StandardTile(Position), ev.Actor);
            var endTurn = new TurnAdvancedEvent(nextPlayer, nextTurn);

            return new EventSequence(
                new GameEvent[] { consumeTile, endTurn },
                FallbackPolicy.AbortChain
            );
        }

        public override ITile Clone() => new GuardianTile(Position);
    }
}
