using System;
using RogueChess.Engine.Events;
using RogueChess.Engine.Primitives;

namespace RogueChess.Engine.Tiles
{
    /// <summary>
    /// Forces a piece to slide one extra step in its movement direction on entering.
    /// </summary>
    public class SlipperyTile : BaseTile, IInterceptor<MoveEvent>
    {
        public SlipperyTile()
            : base() { }

        public SlipperyTile(Vector2Int pos)
            : base(pos) { }

        public SlipperyTile(BaseTile original)
            : base(original) { }

        public int Priority => 0;

        public IEventSequence Intercept(MoveEvent ev, GameState state)
        {
            // If this move was already emitted by us, ignore it to prevent recursion.
            if (ev.SourceID == ID)
                return EventSequences.Continue;

            // Only trigger when the piece lands on this tile.
            if (ev.To != Position)
                return EventSequences.Continue;

            var dir = ev.To - ev.From;
            var step = new Vector2Int(Math.Sign(dir.X), Math.Sign(dir.Y));
            var next = ev.To + step;

            if (!state.Board.IsInBounds(next) || state.Board.GetPieceAt(next) != null)
                return EventSequences.Continue;

            Console.WriteLine($"[SlipperyTile] {ev.Piece.Name} slides {ev.To} → {next}");
            Console.WriteLine($"[Incoming Move ID] {ev.SourceID} and my id is {ID}");

            var slide = new MoveEvent(
                ev.To,
                next,
                ev.Piece,
                ev.Actor,
                ev.IsPlayerAction,
                sourceId: ID
            );

            // Create a new move event identical to the original but with our ID as source
            // This prevents infinite recursion when the event gets processed again
            var processedMove = new MoveEvent(
                ev.From,
                ev.To,
                ev.Piece,
                ev.Actor,
                ev.IsPlayerAction,
                sourceId: ID
            );

            // Emit both the processed move and the forced slide.
            return new EventSequence(
                new GameEvent[] { processedMove, slide },
                FallbackPolicy.ContinueChain
            );
        }
    }
}
